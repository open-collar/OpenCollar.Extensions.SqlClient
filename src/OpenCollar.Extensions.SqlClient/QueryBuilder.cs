/*
 * This file is part of OpenCollar.Extensions.SqlClient.
 *
 * OpenCollar.Extensions.SqlClient is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Extensions.SqlClient is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Extensions.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.Logging;
using OpenCollar.Extensions.SqlClient.Model;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A class that provides a repository of the information required to construct, execute and process a query
    ///     against the database.
    /// </summary>
    public sealed class QueryBuilder
    {
        /// <summary>
        ///     A dictionary, keyed on the case-insensitive argument name, of the parameters to add to the command.
        /// </summary>

        private readonly Dictionary<ParameterName, SqlParameter> _parameters = new Dictionary<ParameterName, SqlParameter>();

        /// <summary>
        ///     The readers to be executed against the results.
        /// </summary>

        private readonly List<Reader> _readers = new List<Reader>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryBuilder" /> class.
        /// </summary>
        /// <param name="connection">
        ///     The connection on which the command will be executed.
        /// </param>
        /// <param name="commandType">
        ///     The type of the command to execute, defining the meaning of the text in the <see cref="CommandText" /> property.
        /// </param>
        /// <param name="commandText">
        ///     The name of the stored procedure to execute.
        /// </param>
        internal QueryBuilder(ConnectionProxy connection, CommandType commandType, Identifier commandText)
        {
            Connection = connection;
            CommandType = commandType;
            CommandText = commandText;
        }

        /// <summary>
        ///     Gets the logger with which to record timings and trace information.
        /// </summary>
        /// <value>
        ///     The logger with which to record timings and trace information.
        /// </value>
        private ILogger? Logger { get { return Connection.Logger; } }

        /// <summary>
        ///     Gets the name of the stored procedure to execute.
        /// </summary>
        /// <value>
        ///     The name of the stored procedure to execute.
        /// </value>
        public Identifier CommandText { get; private set; }

        /// <summary>
        ///     Gets the type of the command to execute, defining the meaning of the text in the
        ///     <see cref="CommandText" /> property.
        /// </summary>
        /// <value>
        ///     The type of the command to execute, defining the meaning of the text in the <see cref="CommandText" /> property.
        /// </value>
        public CommandType CommandType { get; private set; }

        /// <summary>
        ///     Gets the connection on which the command will be executed..
        /// </summary>
        /// <value>
        ///     The connection on which the command will be executed.
        /// </value>
        public ConnectionProxy Connection { get; private set; }

        /// <summary>
        ///     Adds a parameter with the name and value specified.
        /// </summary>
        /// <param name="parameterName">
        ///     The name of the parameter.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the parameter.
        /// </param>
        /// <returns>
        ///     Returns a reference to this object, allowing further parameters or other methods to be called fluently.
        /// </returns>

        public QueryBuilder WithParameter(ParameterName parameterName, object? value)
        {
            parameterName.Validate(nameof(parameterName), ObjectIs.NotNull);

            _parameters.Add(parameterName, new SqlParameter(parameterName, value));

            return this;
        }

        /// <summary>
        ///     Executes the commnad a non-query, asynchronous.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute command with readers without a return value.
        /// </exception>

        public async Task ExecuteNonQueryAsync(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 0)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, "Attempt to execute command with readers without a return value.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            var attempt = (_maxRetries <= 0) ? 1 : _maxRetries;

            while(attempt > 0)
            {
                --attempt;

                try
                {
                    await ExecuteNonQueryCoreAsync(token).ConfigureAwait(true);
                }
                catch(QueryException ex) when(ex.CanRetry && (attempt > 0))
                {
                    Logger.SafeLogWarning(ex, "Attempt to execute query failed, retrying.");
                }
            }
        }

        /// <summary>
        ///     Executes the commnad a non-query, asynchronous.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute command with readers without a return value.
        /// </exception>

        private Task ExecuteNonQueryCoreAsync(CancellationToken cancellationToken)
        {
            if(_readers.Count != 0)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, "Attempt to execute command with readers without a return value.");
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            var command = InitializeCommand();
#pragma warning restore CA2000 // Dispose objects before losing scope

            CallingContext.Current().SetActiveBuilder(this);
            return Connection.ExecuteNonQueryAsync(command, cancellationToken).ContinueWith(t =>
            {
                command.Dispose();
                CallingContext.Current().SetActiveBuilder(null);
            }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Creates and initializes a new command with which to execute the command defined by the builder.
        /// </summary>
        /// <returns>
        ///     The fully initialized command object.
        /// </returns>

        private SqlCommand InitializeCommand()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var command = new SqlCommand
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                CommandText = CommandText,
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                CommandType = CommandType
            };
#pragma warning restore CA2000 // Dispose objects before losing scope

            foreach(var parameter in _parameters)
            {
                command.Parameters.Add(parameter.Value);
            }

            Connection.Factory.SetCommandTimemout(command);

            // Manually configured timeouts trump the automatically configured timeouts.
            if(Timeout == TimeSpan.Zero)
            {
                command.CommandTimeout = (int)Timeout.TotalSeconds;
            }

            return command;
        }

        /// <summary>
        ///     The maximum number of retries to permit.
        /// </summary>
        private int _maxRetries;

        /// <summary>
        ///     Specifies that retries are permitted and allows the maximum number of retries to be specified.
        /// </summary>
        /// <param name="maxRetries">
        ///     The maximum number of retries to permit.
        /// </param>
        /// <returns>
        /// </returns>

        public QueryBuilder WithRetries(int? maxRetries = null)
        {
            if(maxRetries.HasValue)
            {
                _maxRetries = maxRetries.Value;
            }
            else
            {
                _maxRetries = Connection.Factory.Configuration.DefaultRetries;
            }

            return this;
        }

        /// <summary>
        ///     Executes the command and returns a sequence of results, asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        public async Task<IReadOnlyList<object>> ExecuteQueryAsync(CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            var attempt = (_maxRetries <= 0) ? 1 : _maxRetries;

            while(attempt > 0)
            {
                --attempt;

                try
                {
                    return await ExecuteQueryCoreAsync(token).ConfigureAwait(true);
                }
                catch(QueryException ex) when(ex.CanRetry && (attempt > 0))
                {
                    Logger.SafeLogWarning(ex, "Attempt to execute query failed, retrying.");
                }
            }

            // This will never be reached, but for completeness.
            return (IReadOnlyList<object>)(new List<object>()).AsReadOnly();
        }

        /// <summary>
        ///     Executes the command and returns a sequence of results, asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        private Task<IReadOnlyList<object>> ExecuteQueryCoreAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var command = InitializeCommand();
#pragma warning restore CA2000 // Dispose objects before losing scope

            CallingContext.Current().SetActiveBuilder(this);
            var task = Connection.ExecuteReaderAsync(command, cancellationToken);

            var results = new List<object?>();

            var n = 0;
            foreach(var reader in _readers.ToArray())
            {
                if(n > 0)
                {
                    // If this is not the first result set, then append a task to move onto the next result set.
                    task = task.ContinueWith(dataReaderTask =>
                    {
                        var dataReader = dataReaderTask.Result;
                        if(!dataReader.NextResult())
                        {
                            if(reader.IsMandatory)
                            {
                                throw new QueryException(Connection.Key.ConnectionString, $"Mandatory result set missing for reader: {n.ToString("D", System.Globalization.CultureInfo.InvariantCulture)}.");
                            }
                            else
                            {
                                results.Add(GetDefaultValue(reader.ReturnType));
                                return dataReader;
                            }
                        }
                        else
                        {
                            // If there are results then append a task to process those results.
                            return ProcessReader(dataReaderTask.Result, results, n, reader);
                        }
                    }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
                }
                else
                {
                    // Append a task to process the results.
                    task = task.ContinueWith(dataReaderTask =>
                    {
                        return ProcessReader(dataReaderTask.Result, results, n, reader);
                    }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
                }
                ++n;
            }

            // Append a final task that will tidy everything up and return the aggregate results.
            return task.ContinueWith(t =>
            {
                command.Dispose();
                CallingContext.Current().SetActiveBuilder(this);
                return (IReadOnlyList<object>)results.AsReadOnly();
            }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Processes the reader given.
        /// </summary>
        /// <param name="dataReader">
        ///     The data reader from which to take data.
        /// </param>
        /// <param name="results">
        ///     The results to which to add the output.
        /// </param>
        /// <param name="recordSetIndex">
        ///     Index of the record set being read.
        /// </param>
        /// <param name="reader">
        ///     The reader with which to process the data.
        /// </param>
        /// <returns>
        ///     The data reader given.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Mandatory result set missing for reader.
        /// </exception>
        private SqlDataReader ProcessReader(SqlDataReader dataReader, List<object?> results, int recordSetIndex, Reader reader)
        {
            if(dataReader.HasRows)
            {
                results.Add(reader.Execute(dataReader));
            }
            else
            {
                if(reader.IsMandatory)
                {
                    throw new QueryException(Connection.Key.ConnectionString, $"Mandatory result set missing for reader: {recordSetIndex.ToString("D", System.Globalization.CultureInfo.InvariantCulture)}.");
                }
                else
                {
                    results.Add(GetDefaultValue(reader.ReturnType));
                }
            }

            return dataReader;
        }

        /// <summary>
        ///     Gets the default value for the specified type.
        /// </summary>
        /// <param name="type">
        ///     The type for which the default value is required.
        /// </param>
        /// <returns>
        ///     The default value for the type specified
        /// </returns>
        internal static object? GetDefaultValue(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            if(type.IsArray)
            {
                var emptyMethod = typeof(Array).GetMethod("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var typedEmptyMethod = emptyMethod.MakeGenericMethod(new[] { type.GetElementType() });
                return typedEmptyMethod.Invoke(null, Array.Empty<object>());
            }

            if(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
            {
                var emptyMethod = typeof(Array).GetMethod("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var typedEmptyMethod = emptyMethod.MakeGenericMethod(new[] { type.GetGenericArguments().First() });
                return typedEmptyMethod.Invoke(null, Array.Empty<object>());
            }

            if(!type.IsGenericType && type.Equals(typeof(IEnumerable)))
            {
                return Array.Empty<object>();
            }

            if(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(IList<>)))
            {
                var emptyMethod = typeof(Array).GetMethod("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var typedEmptyMethod = emptyMethod.MakeGenericMethod(new[] { type.GetGenericArguments().First() });
                return typedEmptyMethod.Invoke(null, Array.Empty<object>());
            }

            if(!type.IsGenericType && type.Equals(typeof(IList)))
            {
                return Array.Empty<object>();
            }

            return null;
        }

        /// <summary>
        ///     Executes the command and returns a sequence of results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a single reader command with another number of readers defined.
        /// </exception>
        public Task<T1> ExecuteQueryAsync<T1>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 1)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a single reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(token).ContinueWith(t =>
            {
                return (T1)t.Result.ToArray()[0];
            }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2}" /> object containing the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a two-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2>> ExecuteQueryAsync<T1, T2>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 2)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a two-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3}" /> object containing the
        ///     results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a three-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3>> ExecuteQueryAsync<T1, T2, T3>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 3)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a three-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4}" /> object containing the
        ///     results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a four-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4>> ExecuteQueryAsync<T1, T2, T3, T4>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 4)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a four-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5}" /> object containing the
        ///     results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a five-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5>> ExecuteQueryAsync<T1, T2, T3, T4, T5>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 5)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a five-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5, T6}" /> object containing
        ///     the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <typeparam name="T6">
        ///     The type of the sixth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a six-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5, T6>> ExecuteQueryAsync<T1, T2, T3, T4, T5, T6>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 6)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a six-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5, T6> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4], Results6 = (T6)t.Result[5] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5, T6, T7}" /> object
        ///     containing the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <typeparam name="T6">
        ///     The type of the sixth result returned.
        /// </typeparam>
        /// <typeparam name="T7">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a seven-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5, T6, T7>> ExecuteQueryAsync<T1, T2, T3, T4, T5, T6, T7>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 7)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a seven-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5, T6, T7> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4], Results6 = (T6)t.Result[5], Results7 = (T7)t.Result[6] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5, T6, T7, T8}" /> object
        ///     containing the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <typeparam name="T6">
        ///     The type of the sixth result returned.
        /// </typeparam>
        /// <typeparam name="T7">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <typeparam name="T8">
        ///     The type of the ninth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a eight-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5, T6, T7, T8>> ExecuteQueryAsync<T1, T2, T3, T4, T5, T6, T7, T8>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 8)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a eight-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5, T6, T7, T8> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4], Results6 = (T6)t.Result[5], Results7 = (T7)t.Result[6], Results8 = (T8)t.Result[7] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5, T6, T7, T8, T9}" />
        ///     object containing the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <typeparam name="T6">
        ///     The type of the sixth result returned.
        /// </typeparam>
        /// <typeparam name="T7">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <typeparam name="T8">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <typeparam name="T9">
        ///     The type of the ninth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a nine-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9>> ExecuteQueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 9)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a nine-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4], Results6 = (T6)t.Result[5], Results7 = (T7)t.Result[6], Results8 = (T8)t.Result[7], Results9 = (T9)t.Result[8] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the command and returns a <see cref="QueryResults{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}" />
        ///     object containing the results, asynchronously.
        /// </summary>
        /// <typeparam name="T1">
        ///     The type of the first result returned.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The type of the second result returned.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The type of the third result returned.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The type of the fourth result returned.
        /// </typeparam>
        /// <typeparam name="T5">
        ///     The type of the fifth result returned.
        /// </typeparam>
        /// <typeparam name="T6">
        ///     The type of the sixth result returned.
        /// </typeparam>
        /// <typeparam name="T7">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <typeparam name="T8">
        ///     The type of the seventh result returned.
        /// </typeparam>
        /// <typeparam name="T9">
        ///     The type of the ninth result returned.
        /// </typeparam>
        /// <typeparam name="T10">
        ///     The type of the tenth result returned.
        /// </typeparam>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     A task that performs the action.
        /// </returns>
        /// <exception cref="QueryException">
        ///     Attempt to execute a ten-value reader command with another number of readers defined.
        /// </exception>
        public Task<QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> ExecuteQueryAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(CancellationToken? cancellationToken = null)
        {
            if(_readers.Count != 10)
            {
                throw new QueryException(Connection.Factory.Configuration.ConnectionString, $"Attempt to execute a ten-value reader command with {_readers.Count} defined.");
            }

            var token = cancellationToken ?? CancellationToken.None;

            return ExecuteQueryAsync(cancellationToken).ContinueWith(t => { return new QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> { Results1 = (T1)t.Result[0], Results2 = (T2)t.Result[1], Results3 = (T3)t.Result[2], Results4 = (T4)t.Result[3], Results5 = (T5)t.Result[4], Results6 = (T6)t.Result[5], Results7 = (T7)t.Result[6], Results8 = (T8)t.Result[7], Results9 = (T9)t.Result[8], Results10 = (T10)t.Result[9] }; }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Adds a reader to the command builder.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the results returned.
        /// </typeparam>
        /// <param name="readAction">
        ///     The action that reads the data reader and returns a result.
        /// </param>
        /// <param name="isMandatory">
        ///     <see langword="true" /> if least one record must be returned in the recordset; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     Returns a reference to this object, allowing further parameters or other methods to be called fluently.
        /// </returns>
        public QueryBuilder Read<T>(Func<SqlDataReader, T> readAction, bool isMandatory = false)
        {
            _readers.Add(Reader.GetReader(readAction, isMandatory));

            return this;
        }

        /// <summary>
        ///     Adds a reader to the command builder that iterates across all the records in a recordset reading each of
        ///     them and return a result of type <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the results returned for each record.
        /// </typeparam>
        /// <param name="readAction">
        ///     The action that reads a single record from the data reader and returns a result.
        /// </param>
        /// <param name="isMandatory">
        ///     <see langword="true" /> if least one record must be returned in the recordset; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     Returns a reference to this object, allowing further parameters or other methods to be called fluently.
        /// </returns>
        public QueryBuilder ReadEach<T>(Func<SqlDataReader, T> readAction, bool isMandatory = false)
        {
            _readers.Add(Reader.GetReader(r =>
            {
                var results = new List<T>();
                while(r.Read())
                {
                    results.Add(readAction(r));
                }
                return results;
            }, isMandatory));

            return this;
        }

        /// <summary>
        ///     Gets the timeout manually configured.
        /// </summary>
        /// <value>
        ///     The timeout manually configured.
        /// </value>
        public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Adds a timeout to the commnad.
        /// </summary>
        /// <param name="timeout">
        ///     The timeout period.
        /// </param>
        /// <returns>
        ///     Returns a reference to this object, allowing further parameters or other methods to be called fluently.
        /// </returns>
        public QueryBuilder WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }
    }
}