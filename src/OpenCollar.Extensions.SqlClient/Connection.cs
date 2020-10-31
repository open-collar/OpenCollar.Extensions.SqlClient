﻿/*
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using JetBrains.Annotations;

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.Logging;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A managed database connection.
    /// </summary>
    /// <seealso cref="Disposable" />
    public sealed class Connection : Disposable
    {
        /// <summary>
        ///     The text to use in error messages and logging to indicate that a value has not been supplied.
        /// </summary>
        [NotNull]
        private const string ValueNotSet = @"[NOT SET]";

        /// <summary>
        ///     The text to use in error messages and logging to indicate that a value HAS been supplied, but will not
        ///     be revealed.
        /// </summary>
        [NotNull]
        private const string ValueSet = @"[VALUE PROVIDED]";

        /// <summary>
        ///     The underlying SQL connection represented by this object.
        /// </summary>
        [NotNull] private readonly SqlConnection _connection;

        /// <summary>
        ///     The connection pool to which this connection belongs.
        /// </summary>
        [NotNull] private readonly ConnectionPool _connectionPool;

        /// <summary>
        ///     The logger through which to record usage and other information.
        /// </summary>
        [CanBeNull] private readonly ILogger _log;

        /// <summary>
        ///     A list of the messages generated by the connection since the connection was created or recycled.
        /// </summary>
        [NotNull] private readonly List<SqlError> _messages = new List<SqlError>();

        /// <summary>
        ///     A lock used to control concurrent access to the <see cref="_messages" /> field.
        /// </summary>
        [NotNull] private readonly System.Threading.ReaderWriterLockSlim _messagesLock = new System.Threading.ReaderWriterLockSlim();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Connection" /> class.
        /// </summary>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="initializeAzureManagedIdentity">
        ///     If set to <see langword="true" /> the Azure managed identify is initialized for the connection.
        /// </param>
        /// <param name="connectionFactory">
        ///     The connection factory that owns this connection and is reponsible for initializing connections..
        /// </param>
        /// <param name="connectionPool">
        ///     The pool to which this connection belongs.
        /// </param>
        /// <param name="log">
        ///     The log with which to record.
        /// </param>
        /// <exception cref="ConnectionException">
        ///     No value found for MSI_ENDPOINT environment variable. or No value found for MSI_SECRET environment variable.
        /// </exception>
        /// <exception cref="ConnectionException">
        ///     No value found for MSI_ENDPOINT environment variable. or No value found for MSI_SECRET environment variable.
        /// </exception>
        internal Connection([NotNull] ConnectionKey key, bool initializeAzureManagedIdentity, [NotNull] ConnectionFactory connectionFactory, [NotNull] ConnectionPool connectionPool, [CanBeNull] ILogger<Connection> log)
        {
            _log = log;
            Key = key;

            var connection = new SqlConnection(key.ConnectionString);

            // Use MSI authentication if target database is Azure SQL
            if(initializeAzureManagedIdentity)
            {
                if(string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(@"MSI_ENDPOINT")))
                {
                    throw new ConnectionException(key.ConnectionString, @"No value found for MSI_ENDPOINT environment variable.");
                }

                if(string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(@"MSI_SECRET")))
                {
                    throw new ConnectionException(key.ConnectionString, @"No value found for MSI_SECRET environment variable.");
                }

                connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync(@"https://database.windows.net/").Result;
            }

            connection.Open();

            connectionFactory.InitializeConnection(key.Owner, connection);

            connection.FireInfoMessageEventOnUserErrors = true;
            connection.InfoMessage += OnInfo;

            _connection = connection;

            _connectionPool = connectionPool;
        }

        /// <summary>
        ///     Gets the underlying SQL connection.
        /// </summary>
        /// <value>
        ///     The underlying SQL connection.
        /// </value>
        [NotNull]
        public SqlConnection SqlConnection { get { return _connection; } }

        /// <summary>
        ///     Gets the connection pool to which this connection belongs.
        /// </summary>
        /// <value>
        ///     The connection pool to which this connection belongs.
        /// </value>
        [NotNull] internal ConnectionPool ConnectionPool => _connectionPool;

        /// <summary>
        ///     Gets the unique ID of this specific instance.
        /// </summary>
        /// <value>
        ///     The unique ID of this specific instance..
        /// </value>
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        ///     Gets the key identifying the connection and owner.
        /// </summary>
        /// <value>
        ///     The key identifying the connection and owner.
        /// </value>
        [NotNull]
        internal ConnectionKey Key { get; }

        /// <summary>
        ///     Gets the SQL associated with the last command executed on this connection.
        /// </summary>
        /// <value>
        ///     The SQL associated with the last command executed on this connection.
        /// </value>
        [CanBeNull] internal string? LastSql { get; private set; }

        /// <summary>
        ///     Gets the UTC date/time at which this connection last interacted with the server.
        /// </summary>
        /// <value>
        ///     The UTC date/time at which this connection last interacted with the server.
        /// </value>
        internal DateTime LastUsed { get; private set; }

        /// <summary>
        ///     Gets a read-only collection of the messages generated by the connection since the connection was created
        ///     or recycled.
        /// </summary>
        /// <value>
        ///     A read-only collection of the messages generated by the connection since the connection was created or recycled.
        /// </value>
        [NotNull]
        [ItemNotNull]
        internal IReadOnlyCollection<SqlError> Messages
        {
            get
            {
                _messagesLock.EnterReadLock();
                try
                {
                    return new ReadOnlyCollection<SqlError>(_messages.ToArray());
                }
                finally
                {
                    _messagesLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        ///     Checks for unreported errors on the connection (such as unrecognized stored procedures).
        /// </summary>
        /// <param name="command">
        ///     The command being executed.
        /// </param>
        /// <exception cref="DatabaseException">
        /// </exception>
        public void CheckForUnreportedErrors(SqlCommand? command = null)
        {
            if(Messages.Count <= 0)
            {
                return;
            }

            var details = string.Empty;
            var messages = string.Empty;

            foreach(var message in Messages)
            {
                if(message.Class >= 11)
                {
                    details = details + (details.Length > 0 ? "\r\n" : string.Empty) + DescribeSqlError(message);
                    messages = messages + (messages.Length > 0 ? "; " : string.Empty) + message.Message;
                }
            }

            if(details.Length > 0)
            {
                details = "Unhandled errors reported on connection.\r\n" + DescribeSqlCommand(command) + "\r\n" + details;

                // The contents of the 'Details' property are not reported in Application Insights, so instead we must
                // assign the details value to both fields to see the output.

                throw new DatabaseException(details, details);
            }
        }

        /// <summary>
        ///     Recycles this connection.
        /// </summary>
        /// <returns>
        /// </returns>
        internal bool Recycle()
        {
            if(_connection.State != System.Data.ConnectionState.Open)
            {
                return false;
            }

            try
            {
                _connectionPool.Factory.RecycleConnection(Key.Owner, _connection);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch(Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _log.SafeLogWarning(ex, $@"Unable to recycle connection: {ex.Message}");

                return false;
            }
            finally
            {
            }

            return true;
        }

        /// <summary>
        ///     Updates the <see cref="LastUsed" /> and <see cref="LastSql" /> with information about current usage.
        /// </summary>
        /// <param name="sql">
        ///     The SQL associated with the last command executed on this connection.
        /// </param>
        internal void UpdateUsage([NotNull] string? sql)
        {
            LastSql = sql;
            LastUsed = System.DateTime.UtcNow;
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
                ConnectionPool.RemoveConnection(this);
                _connection.Dispose();
                _messagesLock.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Writes the value given to the string builder supplied, formatted for use in an SQL expression.
        /// </summary>
        /// <param name="builder">
        ///     The string builder to which to write the value.
        /// </param>
        /// <param name="value">
        ///     The value to write.
        /// </param>
        private static void WriteValue([NotNull] StringBuilder builder, [CanBeNull] object value)
        {
            if(ReferenceEquals(value, null) || value == DBNull.Value)
            {
                builder.Append("NULL");
                return;
            }

            if(value is DataTable dt)
            {
                builder.Append("Table (");
                builder.Append(dt.TableName);
                builder.AppendLine("):");
                builder.Append("[ ");
                var first = true;
                foreach(DataColumn column in dt.Columns)
                {
                    if(first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(" | ");
                    }

                    builder.Append(column.ColumnName);
                }

                builder.AppendLine(" ]");
                foreach(DataRow row in dt.Rows)
                {
                    builder.Append("[ ");
                    first = true;
                    foreach(var cell in row.ItemArray)
                    {
                        if(first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.Append(" | ");
                        }

                        WriteValue(builder, cell);
                    }

                    builder.AppendLine(" ]");
                }

                return;
            }

            if(value is string)
            {
                builder.Append("N'");
                builder.Append(value);
                builder.Append('\'');
                return;
            }

            if(value is DateTime time)
            {
                builder.Append('\'');
                builder.Append(time.ToString("O", CultureInfo.InvariantCulture));
                builder.Append('\'');
                return;
            }

            if(value is Guid guid)
            {
                builder.Append('\'');
                builder.Append(guid.ToString("D", CultureInfo.InvariantCulture));
                builder.Append('\'');
                return;
            }

            builder.Append(value);
        }

        /// <summary>
        ///     Returns the details of the SQL command given, and the state of the environment in which it was run.
        /// </summary>
        /// <param name="command">
        ///     The command that was being executed.
        /// </param>
        private string DescribeSqlCommand(SqlCommand? command)
        {
            var builder = new StringBuilder();

            WriteConnectionDetails(builder);

            WriteCommandDetails(command, builder);

            return builder.ToString();
        }

        /// <summary>
        ///     Returns the details of the SQL error given, and the state of the environment when it was thrown.
        /// </summary>
        /// <param name="error">
        ///     The exception that was thrown.
        /// </param>
        private static string DescribeSqlError(SqlError? error)
        {
            var builder = new StringBuilder();

            WriteSqlError(error, builder);

            return builder.ToString();
        }

        /// <summary>
        ///     Returns the details of the SQL exception given, and the state of the environment when it was thrown.
        /// </summary>
        /// <param name="command">
        ///     The command that was being executed.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown.
        /// </param>
        private string DescribeSqlError(SqlCommand? command, [CanBeNull] SqlException exception)
        {
            var builder = new StringBuilder();

            builder.AppendLine(@"An unhandled database exception occurred.");

            WriteSqlException(exception, builder);

            WriteConnectionDetails(builder);

            WriteCommandDetails(command, builder);

            return builder.ToString();
        }

        /// <summary>
        ///     Called when information is returned by the connection.
        /// </summary>
        /// <param name="sender">
        ///     The sender of the message.
        /// </param>
        /// <param name="e">
        ///     The <see cref="SqlInfoMessageEventArgs" /> instance containing the event data.
        /// </param>
        private void OnInfo(object sender, SqlInfoMessageEventArgs e)
        {
            if(ReferenceEquals(e, null))
            {
                return;
            }

            _messagesLock.EnterWriteLock();
            try
            {
                foreach(SqlError error in e.Errors)
                {
                    _messages.Add(error);
                }
            }
            finally { _messagesLock.ExitWriteLock(); }
        }

        /// <summary>
        ///     Writes the details of the SQL command given to the builder supplied.
        /// </summary>
        /// <param name="command">
        ///     The command to be record.
        /// </param>
        /// <param name="builder">
        ///     The string builder to which to write the details.
        /// </param>
        private static void WriteCommandDetails(SqlCommand? command, StringBuilder builder)
        {
            builder.Append("SQL: ");
            if(ReferenceEquals(command, null))
            {
                builder.AppendLine(ValueNotSet);
                return;
            }

            builder.AppendLine(command.CommandText);

            builder.Append("Arguments: ");
            var n = 0;
            foreach(SqlParameter parameter in command.Parameters)
            {
                builder.Append("    ");
                builder.Append(n++);
                builder.Append(": ");
                builder.Append(parameter.ParameterName);
                builder.Append(" = ");
                WriteValue(builder, parameter.Value);
                builder.AppendLine();
            }

            builder.Append("Timeout: ");
            builder.AppendLine(command.CommandTimeout.ToString("D", CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///     Writes the full details of this connection to the string builder given.
        /// </summary>
        /// <param name="builder">
        ///     The string builder into which to write the details..
        /// </param>
        private void WriteConnectionDetails([NotNull] StringBuilder builder)
        {
            builder.Append("Connection string: ");
            builder.AppendLine(Key.ConnectionString);

            builder.Append("Owner: ");
            builder.AppendLine(string.IsNullOrWhiteSpace(Key.Owner) ? ValueNotSet : Key.Owner);

            builder.Append("Access token required: ");
            builder.AppendLine(_connectionPool.Factory.InitializeAzureManagedIdentity ? @"YES" : @"NO");

            builder.Append("Access token: ");
            builder.AppendLine(string.IsNullOrWhiteSpace(_connection?.AccessToken) ? ValueNotSet : _connection.AccessToken);

            builder.Append("MSI_ENDPOINT: ");
            var value = System.Environment.GetEnvironmentVariable("MSI_ENDPOINT");
            builder.AppendLine(string.IsNullOrWhiteSpace(value) ? ValueNotSet : value);

            builder.Append("MSI_SECRET: ");
            builder.AppendLine(string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("MSI_SECRET")) ? ValueNotSet : ValueSet);
        }

        /// <summary>
        ///     Writes the properties of the exception given.
        /// </summary>
        /// <param name="exception">
        ///     The exception to be recorded.
        /// </param>
        /// <param name="builder">
        ///     The string builder to which to write the details.
        /// </param>
        /// <param name="ignoreProperties">
        ///     The names of the properties to ignore.
        /// </param>
        private static void WriteExceptionProperties([NotNull] Exception exception, [NotNull] StringBuilder builder,
            [NotNull][ItemNotNull] params string[] ignoreProperties)
        {
            var ignore = ignoreProperties.ToDictionary(p => p);

            foreach(var property in exception.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if(ignore.ContainsKey(property.Name))
                {
                    continue;
                }

                if(!property.CanRead)
                {
                    continue;
                }

                if(property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                builder.Append($@"{property.Name}: ");
                var value = property.GetValue(exception);

                if(ReferenceEquals(value, null) || value == DBNull.Value)
                {
                    builder.AppendLine(@"NULL");
                }
                else
                {
                    if(property.PropertyType == typeof(string))
                    {
                        builder.Append('"');
                        builder.Append(value);
                        builder.AppendLine("\"");
                    }
                    else
                    {
                        builder.AppendLine(value.ToString());
                    }
                }
            }
        }

        /// <summary>
        ///     Writes the details of the SQL error given to the builder supplied.
        /// </summary>
        /// <param name="error">
        ///     The error to be record.
        /// </param>
        /// <param name="builder">
        ///     The string builder to which to write the details.
        /// </param>
        private static void WriteSqlError(SqlError? error, StringBuilder builder)
        {
            if(ReferenceEquals(error, null))
            {
                builder.AppendLine(@"Error: [NOT SET]");
                return;
            }

            builder.Append(@"Message: ");
            builder.AppendLine(error.Message);

            builder.Append(@"Procedure: ");
            builder.AppendLine(error.Procedure);

            builder.Append(@"Line: ");
            builder.AppendLine(error.LineNumber.ToString(@"D", CultureInfo.InvariantCulture));

            builder.Append(@"Server: ");
            builder.AppendLine(error.Server);
        }

        /// <summary>
        ///     Writes the details of the SQL exception given to the builder supplied.
        /// </summary>
        /// <param name="exception">
        ///     The exception to be record.
        /// </param>
        /// <param name="builder">
        ///     The string builder to which to write the details.
        /// </param>
        private void WriteSqlException([CanBeNull] SqlException exception, [NotNull] StringBuilder builder)
        {
            if(ReferenceEquals(exception, null))
            {
                builder.AppendLine(@"Exception: [NOT SET]");
                return;
            }

            builder.Append(@"Message: ");
            builder.AppendLine(exception.Message);

            builder.Append(@"Procedure: ");
            builder.AppendLine(exception.Procedure);

            builder.Append(@"Line: ");
            builder.AppendLine(exception.LineNumber.ToString(@"D", CultureInfo.InvariantCulture));

            builder.Append(@"Server: ");
            builder.AppendLine(exception.Server);

            builder.AppendLine(@"Errors: ");
            var n = 0;
            foreach(SqlError error in exception.Errors)
            {
                builder.Append("    ");
                builder.Append(n++);
                builder.Append(": ");
                builder.Append(error.Procedure);
                builder.Append(" [");
                builder.Append(error.LineNumber.ToString(@"D", CultureInfo.InvariantCulture));
                builder.Append("]: ");
                builder.AppendLine(error.Message);
            }

            var messages = Messages;
            if(messages.Count > 0)
            {
                builder.AppendLine(@"Messages: ");
                n = 0;
                foreach(var message in messages)
                {
                    if(message.Class >= 11 /* error */)
                    {
                        // This will already have been recorded above.
                        continue;
                    }

                    builder.Append("    ");
                    builder.Append(n++);
                    builder.Append(": ");
                    builder.Append(message.Procedure);
                    builder.Append(" [");
                    builder.Append(message.LineNumber.ToString(@"D", CultureInfo.InvariantCulture));
                    builder.Append("]: ");
                    builder.AppendLine(message.Message);
                }
            }

            WriteExceptionProperties(exception, builder, "Message", "Procedure", "LineNumber", "Server", "Errors");
        }
    }
}