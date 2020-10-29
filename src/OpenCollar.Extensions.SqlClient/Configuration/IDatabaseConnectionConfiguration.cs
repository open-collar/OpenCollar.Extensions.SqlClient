/*
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

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Extensions.SqlClient.Configuration
{
    /// <summary>
    ///     A configuration object used to define the settings for a database connection.
    /// </summary>
    /// <seealso cref="OpenCollar.Extensions.Configuration.IConfigurationObject" />
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
        ///     Gets the connection string.
        /// </summary>
        /// <value>
        ///     The connection string.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly)]
        [Path(PathIs.Relative, @"ConnectionString")]
        public string ConnectionString { get; }

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
    }
}