/*
 * This file is part of OpenCollar.Extensions.SqlClient.
 *
 * OpenCollar.Extensions.SqlClient is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Extensions is distributed in the hope that it will be
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
using System.Data;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.Logging;
using OpenCollar.Extensions.SqlClient.LoggingContext;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A proxy that wraps the <see cref="Connection" /> class that provides simple <see cref="IDisposable" />
    ///     semantics and other niceties.
    /// </summary>
    /// <seealso cref="Disposable" />
    public sealed class ConnectionProxy : Disposable
    {
        /// <summary>
        ///     The to wrap and with which to work.
        /// </summary>
        [NotNull] private readonly Connection _connection;

        /// <summary>
        ///     The logger with which to record timings and trace information.
        /// </summary>
        [NotNull] private readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionProxy" /> class.
        /// </summary>
        /// <param name="connection">
        ///     The underly connection to wrap.
        /// </param>
        /// <param name="logger">
        ///     The logger with which to record timings and trace information.
        /// </param>
        internal ConnectionProxy([NotNull] Connection connection, [NotNull] ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <summary>
        ///     Gets the underlying connection to wrap.
        /// </summary>
        /// <value>
        ///     The underlying connection to wrap.
        /// </value>
        [NotNull]
        public SqlConnection SqlConnection { get { return _connection.SqlConnection; } }

        /// <summary>
        ///     Gets the factory used to create the connection.
        /// </summary>
        /// <value>
        ///     The factory used to create the connection.
        /// </value>
        internal ConnectionFactory Factory { get { return _connection.ConnectionPool.Factory; } }

        /// <summary>
        ///     Gets the key identifying the connection and owner.
        /// </summary>
        /// <value>
        ///     The key identifying the connection and owner.
        /// </value>
        [NotNull]
        internal ConnectionKey Key { get { return _connection.Key; } }

        /// <summary>
        ///     Gets the logger with which to record timings and trace information.
        /// </summary>
        /// <value>
        ///     The logger with which to record timings and trace information.
        /// </value>
        internal ILogger Logger => _logger;

        /// <summary>
        ///     Executes an SQL statement against the <see langword="Connection" /> object of a .NET Framework data
        ///     provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     The number of rows affected.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="command" /> is <see langword="null" />.
        /// </exception>
        public Task<int> ExecuteNonQueryAsync([NotNull] SqlCommand command, System.Threading.CancellationToken? cancellationToken = null)
        {
            command.Validate(nameof(command), ObjectIs.NotNull);

            var token = cancellationToken ?? System.Threading.CancellationToken.None;
            command.Connection = _connection.SqlConnection;

            var scope = InitializeLogging(command);

            var start = System.DateTime.UtcNow;

            return command.ExecuteNonQueryAsync(token).ContinueWith(t =>
            {
                CompleteExecution(command, start, scope);
                scope.Dispose();
                return t.Result;
            }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the <see cref="IDbCommand.CommandText" /> against the <see cref="IDbCommand.Connection" /> and
        ///     builds an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     An <see cref="IDataReader" /> object.
        /// </returns>
        public Task<SqlDataReader> ExecuteReaderAsync([NotNull] SqlCommand command, System.Threading.CancellationToken? cancellationToken = null)
        {
            command.Validate(nameof(command), ObjectIs.NotNull);

            var token = cancellationToken ?? System.Threading.CancellationToken.None;
            command.Connection = _connection.SqlConnection;

            var scope = InitializeLogging(command);

            var start = System.DateTime.UtcNow;

            return command.ExecuteReaderAsync(token).ContinueWith(t =>
            {
                CompleteExecution(command, start, scope);

                command.Dispose();
                return t.Result;
            }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the <see cref="IDbCommand.CommandText" /> against the <see cref="IDbCommand.Connection" />, and
        ///     builds an <see cref="IDataReader" /> using one of the <see cref="CommandBehavior" /> values.
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="behavior">
        ///     One of the <see cref="CommandBehavior" /> values.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     An <see cref="IDataReader" /> object.
        /// </returns>
        public Task<SqlDataReader> ExecuteReaderAsync([NotNull] SqlCommand command, CommandBehavior behavior, System.Threading.CancellationToken? cancellationToken = null)
        {
            command.Validate(nameof(command), ObjectIs.NotNull);

            var token = cancellationToken ?? System.Threading.CancellationToken.None;
            command.Connection = _connection.SqlConnection;

            var scope = InitializeLogging(command);

            var start = System.DateTime.UtcNow;

            return command.ExecuteReaderAsync(behavior, token).ContinueWith(t =>
            {
                CompleteExecution(command, start, scope);

                return t.Result;
            }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Executes the query, and returns the first column of the first row in the resultset returned by the
        ///     query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token to be used to request that the operation be abandoned before the command timeout elapses.
        /// </param>
        /// <returns>
        ///     The first column of the first row in the resultset.
        /// </returns>
        public Task<object> ExecuteScalarAsync([NotNull] SqlCommand command, System.Threading.CancellationToken? cancellationToken = null)
        {
            command.Validate(nameof(command), ObjectIs.NotNull);

            var token = cancellationToken ?? System.Threading.CancellationToken.None;
            command.Connection = _connection.SqlConnection;

            var scope = InitializeLogging(command);

            var start = System.DateTime.UtcNow;

            return command.ExecuteScalarAsync(token).ContinueWith(t =>
            {
                CompleteExecution(command, start, scope);

                command.Dispose();
                return t.Result;
            }, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to
        ///     release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _connection.ConnectionPool.RemoveConnection(_connection);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Checks the outcome of executing a command and tidies-up.
        /// </summary>
        /// <param name="command">
        ///     The command to check.
        /// </param>
        /// <param name="start">
        ///     The time at which the command started executed.
        /// </param>
        /// <param name="scope">
        ///     The logging scope within which the command is being executed.
        /// </param>
        private void CompleteExecution([NotNull] SqlCommand command, DateTime start, ITransientContextualInformationScope scope)
        {
            Logger.SafeLogTrace($"Executed: \"{command.CommandText}\".  Duration: {(System.DateTime.UtcNow.Subtract(start)).TotalMilliseconds.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}");
            try
            {
                _connection.CheckForUnreportedErrors(command);
            }
            catch(Exception ex)
            {
                var alternateException = _connection.ConnectionPool.Factory.AnalyzeException(ex, _connection, command);
                if(!(alternateException is null))
                {
                    if(ReferenceEquals(alternateException, ex))
                    {
                        throw alternateException;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            finally
            {
                command.Dispose();
                scope.Dispose();
            }
        }

        private ITransientContextualInformationScope InitializeLogging(SqlCommand command)
        {
            var scope = OpenCollar.Extensions.Logging.LoggingContext.Current().StartScope();
            scope.Context.AddDatabaseConnection(Key.ConnectionString).AddStoredProcedure(command);
            Logger.SafeLogTrace($"Executing: \"{command.CommandText}\".");
            return scope;
        }
    }
}