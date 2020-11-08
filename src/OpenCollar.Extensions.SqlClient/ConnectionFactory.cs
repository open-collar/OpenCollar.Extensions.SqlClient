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
using System.Collections.Concurrent;
using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.Environment;
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
        ///     A dictionary of connections pools, keyed on the connection string and user.
        /// </summary>

        private readonly ConcurrentDictionary<ConnectionKey, ConnectionPool> _connectionPoolCache = new ConcurrentDictionary<ConnectionKey, ConnectionPool>();

        /// <summary>
        ///     The logger through which to record informatkon about activity and other information.
        /// </summary>
        private readonly ILogger? _log;

        /// <summary>
        ///     The services provider from which to get resources such as loggers.
        /// </summary>
        private readonly IServiceProvider _services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        /// <param name="services">
        ///     The services provider from which to get resources such as loggers.
        /// </param>
        /// <param name="configuration">
        ///     The configuration for database connections as a whole.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        protected ConnectionFactory(IServiceProvider services, IDatabaseConfiguration configuration) : this(services, configuration, null)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionFactory" /> class.
        /// </summary>
        /// <param name="services">
        ///     The services provider from which to get resources such as loggers.
        /// </param>
        /// <param name="configuration">
        ///     The configuration for database connections as a whole.
        /// </param>
        /// <param name="environmentMetadataProvider">
        ///     The service that provides the environment metadata for a given application.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        protected ConnectionFactory(IServiceProvider services, IDatabaseConfiguration configuration, IEnvironmentMetadataProvider? environmentMetadataProvider)
        {
            configuration.Validate(nameof(configuration), ObjectIs.NotNull);

            EnvironmentMetadataProvider = environmentMetadataProvider;

            var connectionConfiguration = configuration.Connections[ConnectionKey];

            Configuration = connectionConfiguration;

            _services = services;

            _log = services.GetService<ILogger<ConnectionFactory>>();

            InitializeAzureManagedIdentity = Configuration.InitializeAzureManagedIdentity && IsMsiInitializationRequired(Configuration.ConnectionString);

            if(Configuration.IsEnvironmentValidationEnabled)
            {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
                ValidateConnection();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
            }
        }

        /// <summary>
        ///     Gets the configuration of the database connection.
        /// </summary>
        /// <value>
        ///     The configuration of the database connection.
        /// </value>
        public IDatabaseConnectionConfiguration Configuration { get; }

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
        protected abstract string ConnectionKey { get; }

        /// <summary>
        ///     Gets the default name of the owner.
        /// </summary>
        /// <value>
        ///     The default name of the owner.
        /// </value>
        protected virtual string DefaultOwnerName => @"Default";

        /// <summary>
        ///     Gets the service that provides the environment metadata for a given application. Can be
        ///     <see langword="null" /> if the host application does not support the environment
        ///     <see cref="IEnvironmentMetadataProvider" /> service.
        /// </summary>
        protected IEnvironmentMetadataProvider? EnvironmentMetadataProvider { get; }

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

        public ConnectionProxy GetConnection(string? owner = null)
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
        internal static bool IsMsiInitializationRequired(string connectionString)
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
        protected internal virtual Exception AnalyzeException(Exception exception, Connection connection, SqlCommand command) => exception;

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
        protected internal virtual void InitializeConnection(string? owner, SqlConnection connection)
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
        protected internal virtual void RecycleConnection(string? owner, SqlConnection connection)
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
        protected internal virtual void SetCommandTimemout(IDbCommand command)
        {
        }

        /// <summary>
        ///     Gets a string containing the environment to which the database defined by the connection string belongs.
        ///     This can be <see langword="null" /> if no environment metadata provider has been configured.
        /// </summary>
        /// <value>
        ///     A string containing the environment to which the database defined by the connection string belongs. This
        ///     can be <see langword="null" /> if no environment metadata provider has been configured.
        /// </value>
        protected string? DatabaseEnvironment
        {
            get
            {
                if(EnvironmentMetadataProvider is null)
                {
                    // We can't determine the enviornment if there is no metadata provider to do the work.
                    return null;
                }

                var builder = new SqlConnectionStringBuilder(Configuration.ConnectionString);

                return EnvironmentMetadataProvider.GetResourceEnvironment(builder.DataSource);
            }
        }

        /// <summary>
        ///     Validates the connection configuration before attempting to create an new connection.
        /// </summary>
        /// <exception cref="MismatchedEnvironmentException">
        ///     Database environment cannot be determined.
        /// </exception>
        /// <exception cref="MismatchedEnvironmentException">
        ///     Database environment does not match the application environment.
        /// </exception>
        /// <exception cref="MismatchedEnvironmentException">
        ///     Database environment could not be compared to the application environment, probably because it was not a
        ///     known envioronment.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         Environment validation uses the <see cref="Environment.EnvironmentMetadataProvider" /> service to
        ///         determine the application environment and the database environment and with that information
        ///         validate that connections between environments are not be created accidentally.
        ///     </para>
        ///     <para> This method can be overridden by factory classes to implement special behavior. </para>
        ///     <para> Environment validation is controlled by these following configuration properties:
        ///         <list type="table">
        ///             <listheader>
        ///                 <term> Property </term>
        ///                 <description> Description </description>
        ///             </listheader>
        ///             <item>
        ///                 <term> <see cref="IDatabaseConnectionConfiguration.IsEnvironmentValidationEnabled" /> </term>
        ///                 <description>
        ///                     This defaults to <see langword="true" />. Where validation is not required the it can be
        ///                     disabled using this flag.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <term> <see cref="IDatabaseConnectionConfiguration.IsEnvironmentValidationStrict" /> </term>
        ///                 <description>
        ///                     This defaults to <see langword="true" />. When a naming conventions are not always
        ///                     adhered to (for example on a developers desktop) it may be desirable to ignore the
        ///                     mismatches that might be caused by and unrecognized environment name.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        protected virtual void ValidateConnection()
        {
            if(EnvironmentMetadataProvider is null)
            {
                // We can't make comparisons if there is no metadata provider to do the work.
                return;
            }

            var appEnvironment = EnvironmentMetadataProvider.GetEnvironmentMetadata();

            if(appEnvironment is null)
            {
                // If the application has no defined environment then there is nothing to validate against.
                return;
            }

            var dbEnvironment = DatabaseEnvironment;

            if(dbEnvironment is null)
            {
                // If there is an app environment, but no DB environment.
                throw new MismatchedEnvironmentException($"Database environment cannot be determined.");
            }

            var compare = EnvironmentMetadataProvider.IsValidEnvironmentPairing(appEnvironment, dbEnvironment);

            if(!compare.HasValue)
            {
                if(Configuration.IsEnvironmentValidationStrict)
                {
                    throw new MismatchedEnvironmentException($"Database environment (\"{dbEnvironment}\") could not be compared to the application environment (\"{appEnvironment.Environment}\"), probably because it was not a known environment.");
                }

                return;
            }

            if(!compare.Value)
            {
                throw new MismatchedEnvironmentException($"Database environment (\"{dbEnvironment}\") does not match the application environment (\"{appEnvironment.Environment}\").");
            }
        }
    }
}