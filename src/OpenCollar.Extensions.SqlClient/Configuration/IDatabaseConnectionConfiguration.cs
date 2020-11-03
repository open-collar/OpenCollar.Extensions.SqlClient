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

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Extensions.SqlClient.Configuration
{
    /// <summary>
    ///     A configuration object used to define the settings for a database connection.
    /// </summary>
    /// <seealso cref="IConfigurationObject" />
    public interface IDatabaseConnectionConfiguration : IConfigurationObject
    {
        /// <summary>
        ///     Gets the period of time for which idle connection are cached, measured in seconds.
        /// </summary>
        /// <value>
        ///     The period of time for which idle connection are cached, measured in seconds.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 120)]
        [Path(PathIs.Relative, @"ConnectionCacheTimeoutSeconds")]
        public int ConnectionCacheTimeoutSeconds { get; }

        /// <summary>
        ///     Gets the database connection string.
        /// </summary>
        /// <value>
        ///     The database connection string.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly)]
        [Path(PathIs.Relative, @"ConnectionString")]
        public string ConnectionString { get; }

        /// <summary>
        ///     Gets or sets the default number of retries to attempt, where premitted and necessary.
        /// </summary>
        /// <value>
        ///     The default number of retries to attempt, where premitted and necessary.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 3)]
        [Path(PathIs.Relative, @"DefaultRetries")]
        public int DefaultRetries
        {
            get; set;
        }

        /// <summary>
        ///     Gets a value indicating whether
        ///     <see href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview">
        ///     Azure Managed Identity </see> initializion should be performed before creating a connection (if an Azure
        ///     SQL Server connection is being created).
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if
        ///     <see href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview">
        ///     Azure Managed Identity </see> initializion should be performed before creating a connection (if an Azure
        ///     SQL Server connection is being created); otherwise, <see langword="false" />.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = false)]
        [Path(PathIs.Relative, @"InitializeAzureManagedIdentity")]
        public bool InitializeAzureManagedIdentity { get; }

        /// <summary>
        ///     Gets a value indicating whether this environment validation is enabled for connections.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if environment validation is enabled; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        ///     This defaults to <see langword="true" />. Environment validation uses the
        ///     <see cref="Environment.EnvironmentMetadataProvider" /> service to determine the application environment
        ///     and the database environment and with that information validate that connections between environments
        ///     are not be created accidentally. Where validation is not required the it can be disabled using this flag.
        /// </remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = true)]
        [Path(PathIs.Relative, @"IsEnvironmentValidationEnabled")]
        public bool IsEnvironmentValidationEnabled { get; }

        /// <summary>
        ///     Gets a value indicating whether this environment validation should permit uncertainty or not.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if environment validation should treat an uncertain outcome as a fail;
        ///     otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        ///     This defaults to <see langword="true" />. Environment validation uses the
        ///     <see cref="Environment.EnvironmentMetadataProvider" /> service to determine the application environment
        ///     and the database environment and with that information validate that connections between environments
        ///     are not be created accidentally. When a naming conventions are not always adhered to (for example on a
        ///     developers desktop) it may be desirable to ignore the mismatches that might be caused by and
        ///     unrecognized environment name.
        /// </remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = true)]
        [Path(PathIs.Relative, @"IsEnvironmentValidationStrict")]
        public bool IsEnvironmentValidationStrict { get; }
    }
}