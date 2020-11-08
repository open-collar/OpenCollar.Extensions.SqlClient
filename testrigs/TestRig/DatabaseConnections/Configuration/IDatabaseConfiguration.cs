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

using JetBrains.Annotations;

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Extensions.SqlClient.Configuration
{
    /// <summary>
    ///     A configuration object used to define the settings for database connections.
    /// </summary>
    /// <seealso cref="IConfigurationObject" />
    public interface IDatabaseConfiguration : IConfigurationObject
    {
        /// <summary>
        ///     Gets or sets the dictionary of database connection definitions.
        /// </summary>
        /// <value>
        ///     The dictionary of database connection definitions.
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = null)]
        [Path(PathIs.Absolute, @"DatabaseConfiguration:Connections")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IConfigurationDictionary<IDatabaseConnectionConfiguration> Connections { get; set; }

#pragma warning restore CA2227 // Collection properties should be read only
    }
}