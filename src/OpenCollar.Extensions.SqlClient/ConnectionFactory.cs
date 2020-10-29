﻿/*
 * This file is part of OpenCollar.Extensions.
 *
 * OpenCollar.Extensions is free software: you can redistribute it
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
using System.Collections.Concurrent;
using System.Data;

using JetBrains.Annotations;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.SqlClient.Configuration;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     The base class for objects defining database connection factories.
    /// </summary>
    public abstract class ConnectionFactory
    {
        /// <summary>
        ///     The configuration of the database connection.
        /// </summary>
        [NotNull]
        private readonly IDatabaseConnectionConfiguration _configuration;

        /// <summary>
        ///     A dictionary of connections pools, keyed on the connection string and user.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<ConnectionKey, ConnectionPool> _connectionPoolCache = new ConcurrentDictionary<ConnectionKey, ConnectionPool>();

        /// <summary>
        ///     The logger through which to record informatkon about activity and other information.
        /// </summary>
        [CanBeNull] private readonly ILogger _log;

        /// <summary>
        ///     The services provider from which to get resources such as loggers.
        /// </summary>
        [NotNull] private readonly IServiceProvider _services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        /// <param name="services">
        ///     The services provider from which to get resources such as loggers.
        /// </param>
        /// <param name="configuration">
        ///     The configuration for database connections as a whole.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        protected ConnectionFactory([NotNull] IServiceProvider services, [NotNull] IDatabaseConfiguration configuration)
        {
            configuration.Validate(nameof(configuration), ObjectIs.NotNull);

            _configuration = configuration.Connections[ConnectionKey];

            _services = services;

            _log = services.GetService<ILogger<ConnectionFactory>>();

            InitializeAzureManagedIdentity = _configuration.InitializeAzureManagedIdentity && IsMsiInitializationRequired(_configuration.ConnectionString);
        }

        /// <summary>
        ///     Gets the configuration of the database connection.
        /// </summary>
        /// <value>
        ///     The configuration of the database connection.
        /// </value>
        [NotNull] public IDatabaseConnectionConfiguration Configuration => _configuration;

        /// <summary>
        ///     Gets a value indicating whether
        ///     <see href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview">
        ///     Azure Managed Identity initializion </see> should be performed before creating a connection.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if
        ///     <see href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview">
        ///     Azure Managed Identity initializion </see> initializion should be performed before creating a
        ///     connection; otherwise, <see langword="false" />.
        /// </value>
        public bool InitializeAzureManagedIdentity { get; }

        /// <summary>
        ///     Gets the key in the <see cref="IDatabaseConfiguration.Connections" /> dictionary of the
        ///     <see cref="IDatabaseConnectionConfiguration" /> that defines the connection details.
        /// </summary>
        /// <value>
        ///     The key in the <see cref="IDatabaseConfiguration.Connections" /> dictionary of the
        ///     <see cref="IDatabaseConnectionConfiguration" /> that defines the connection details.
        /// </value>
        [NotNull] protected abstract string ConnectionKey { get; }

        /// <summary>
        ///     Gets the default name of the owner.
        /// </summary>
        /// <value>
        ///     The default name of the owner.
        /// </value>
        [NotNull] protected virtual string DefaultOwnerName { get { return @"Default"; } }

        /// <summary>
        ///     Gets a connection for the owner specified.
        /// </summary>
        /// <param name="owner">
        ///     A string that identifies the owner of the connection. Use <see langword="null" /> to use a default value.
        /// </param>
        /// <returns>
        ///     A fully initialized database connections.
        /// </returns>
        /// <exception cref="BadImplementationException">
        ///     <see cref="DefaultOwnerName" /> returned <see langword="null" />.
        /// </exception>
        [NotNull]
        public ConnectionProxy GetConnection([CanBeNull] string? owner = null)
        {
            if(owner is null)
            {
                owner = DefaultOwnerName;

                if(owner is null)
                {
                    throw new BadImplementationException(@$"'{DefaultOwnerName}' returned null.");
                }
            }

            var key = new ConnectionKey(owner, Configuration.ConnectionString);

            var pool = _connectionPoolCache.GetOrAdd(key, k => new ConnectionPool(_services, _services.GetService<ILogger<ConnectionPool>>(), key, this));

            return new ConnectionProxy(pool.GetConnection(), _log);
        }

        /// <summary>
        ///     Determines the whether to initialize MSI credentials.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string to examine.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if MSI should be initialized; otherwise, <see langword="false" />.
        /// </returns>
        internal static bool IsMsiInitializationRequired([NotNull] string connectionString)
        {
            // If it is to an Azure endpoint it might still be using SQL Server authentication, so check to see if the
            // connection string also contains the keys required to configure that.
            var terms = new[] { @"UserID", @"Password", @"PWD=", @"UID=" };
            foreach(var term in terms)
            {
                if(connectionString.Contains(term, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            // We can look to see if the connection is to an Azure endpoint.
            var builder = new SqlConnectionStringBuilder(connectionString);
            var dataSource = builder.DataSource;
            if(!dataSource.Contains(@".database.windows.net", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Provides the implementor with the opportunity to analyze any exception thrown by query execution before
        ///     and throw a new exception if necessary.
        /// </summary>
        /// <param name="exception">
        ///     The exception to analyze.
        /// </param>
        /// <param name="connection">
        ///     The connection on which the exception occurred.
        /// </param>
        /// <param name="command">
        ///     The command being executed when the exception was thrown.
        /// </param>
        /// <returns>
        /// </returns>
        protected internal virtual Exception AnalyzeException([NotNull] Exception exception, [NotNull] Connection connection, SqlCommand command)
        {
            return exception;
        }

        /// <summary>
        ///     Perform custom initialization on the connection specified.
        /// </summary>
        /// <param name="owner">
        ///     The owner of the connection (as defined by the caller).
        /// </param>
        /// <param name="connection">
        ///     The connection to initialize.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         If implemented, this method can be used to perform custom initialization (for example, initializing
        ///         a security context) on new connections as they are created.
        ///     </para>
        ///     <para>
        ///         All connections are created with an "owner". If nothing is explicitly specified the owner will be
        ///         "default"; otherwise it is a string that, typically, will contain a value that idenifies the unique
        ///         feature of the custom initialization performed (for example the calling user's email address) and
        ///         connections are recycled only when the owner is the same.
        ///     </para>
        /// </remarks>
        protected internal virtual void InitializeConnection([CanBeNull] string? owner, [NotNull] SqlConnection connection)
        {
        }

        /// <summary>
        ///     Recycles a connection that has previously been used.
        /// </summary>
        /// <param name="owner">
        ///     The owner of the connection (as defined by the caller). This is the owner that was specified when
        ///     <see cref="InitializeConnection(string?, SqlConnection)" /> was called, and will be the owner used when
        ///     the connection is reused.
        /// </param>
        /// <param name="connection">
        ///     The connection to recycle.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         If implemented, this method can be used to perform custom teardown on a connection that has been
        ///         freed by its current user. All connections are recycled for the see value of
        ///         <paramref name="owner" /> and should not normally require any custom actions when recycling.
        ///     </para>
        ///     <para>
        ///         All connections are created with an "owner". If nothing is explicitly specified the owner will be
        ///         "default"; otherwise it is a string that, typically, will contain a value that idenifies the unique
        ///         feature of the custom initialization performed (for example the calling user's email address) and
        ///         connections are recycled only when the owner is the same.
        ///     </para>
        /// </remarks>
        protected internal virtual void RecycleConnection([CanBeNull] string? owner, [NotNull] SqlConnection connection)
        {
        }

        /// <summary>
        ///     Called before the command is executed, providing an opportunity for the command's timeout to be
        ///     configured immediately before it is executed.
        /// </summary>
        /// <param name="command">
        ///     The command to be configured.
        /// </param>
        /// <remarks>
        ///     The command as a whole is provided allowing for any of its properties to be used to determine the
        ///     correct timeout value.
        /// </remarks>
        protected internal virtual void SetCommandTimemout([NotNull] IDbCommand command)
        {
        }
    }
}