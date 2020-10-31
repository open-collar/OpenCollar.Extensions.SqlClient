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

using JetBrains.Annotations;

using OpenCollar.Extensions.Environment;
using OpenCollar.Extensions.SqlClient;
using OpenCollar.Extensions.SqlClient.Configuration;

namespace TestRig.DatabaseConnections
{
    /// <summary>
    ///     A connection factory that provodes connections to the default database for the application.
    /// </summary>
    /// <seealso cref="ConnectionFactory" />
    public sealed class DefaultConnectionFactory : ConnectionFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultConnectionFactory" /> class.
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
        public DefaultConnectionFactory(IServiceProvider services, IDatabaseConfiguration configuration, IEnvironmentMetadataProvider? environmentMetadataProvider) : base(services, configuration, environmentMetadataProvider)
        {
        }

        /// <summary>
        ///     Gets the name of the connection.
        /// </summary>
        /// <value>
        ///     The name of the connection.
        /// </value>
        protected override string ConnectionKey { get { return @"Default"; } }
    }
}