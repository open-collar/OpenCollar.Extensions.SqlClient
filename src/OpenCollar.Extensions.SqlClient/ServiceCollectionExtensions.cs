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

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenCollar.Extensions.Configuration;
using OpenCollar.Extensions.SqlClient.Configuration;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Extensions to the <see cref="IServiceCollection" /> class providing fluent methods to register the
    ///     <see cref="IConnectionService" /> service.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a database connection service to the service collection.
        /// </summary>
        /// <typeparam name="TConnectionFactory">
        ///     The type of the connection factory implementation to register. Must be derived from the
        ///     <see cref="ConnectionFactory" /> base class.
        /// </typeparam>
        /// <param name="serviceCollection">
        ///     The service collection to which to add the connection factory.
        /// </param>
        /// <returns>
        ///     The service collection given in the <paramref name="serviceCollection" />.
        /// </returns>
        public static IServiceCollection AddConnectionFactory<TConnectionFactory>([NotNull] this IServiceCollection serviceCollection) where TConnectionFactory : ConnectionFactory
        {
            serviceCollection.Validate(nameof(serviceCollection), ObjectIs.NotNull);

            var settings = new ConfigurationObjectSettings() { EnableNewtonSoftJsonSupport = false };

            serviceCollection.AddConfigurationReader<IDatabaseConfiguration>(settings);

            serviceCollection.TryAddSingleton<TConnectionFactory>();

            return serviceCollection;
        }
    }
}