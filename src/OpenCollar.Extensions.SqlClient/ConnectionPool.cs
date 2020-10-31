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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A class representing a pool of connections to a single connection string/owner.
    /// </summary>
    internal sealed class ConnectionPool
    {
        /// <summary>
        ///     A dictionary of all active connections, keyed on their <see cref="Connection.InstanceId" />.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, Connection> _activeConnections = new ConcurrentDictionary<Guid, Connection>();

        /// <summary>
        ///     The custom factory used to initialize connections before they are used.
        /// </summary>
        [NotNull]
        private readonly ConnectionFactory _connectionFactory;

        /// <summary>
        ///     A dictionary of all idle connections, keyed on their <see cref="Connection.InstanceId" />.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, Connection> _idleConnections = new ConcurrentDictionary<Guid, Connection>();

        /// <summary>
        ///     The logger through which to record usage and other information.
        /// </summary>
        [CanBeNull] private readonly ILogger _logger;

        /// <summary>
        ///     The services provider from which to get resource when intializing new connections.
        /// </summary>
        [NotNull] private readonly IServiceProvider _services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionPool" /> class.
        /// </summary>
        /// <param name="services">
        ///     The services provider from which to get resources such as loggers.
        /// </param>
        /// <param name="logger">
        ///     The logger through which to record usage and other information.
        /// </param>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="connectionFactory">
        ///     The method to call to initialize each new connection created. The action provides three arguments:
        ///     <list type="bullet">
        ///         <item>
        ///             <term> <c> connectionKey </c> </term>
        ///             <description> The key identifying the connection being created. </description>
        ///         </item>
        ///         <item>
        ///             <term> <c> owner </c> </term>
        ///             <description>
        ///                 A string specifying the owner of the connection being created. This can be
        ///                 <see langword="null" />. This will usually be the email address of the user on behalf of
        ///                 whom database calls are being made.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term> <c> sqlConnection </c> </term>
        ///             <description> The SQL connection to initialize. </description>
        ///         </item>
        ///     </list>
        /// </param>
        internal ConnectionPool([NotNull] IServiceProvider services, [CanBeNull] ILogger<ConnectionPool> logger, [NotNull] ConnectionKey key, [NotNull] ConnectionFactory connectionFactory)
        {
            _logger = logger;
            _services = services;
            Key = key;
            _connectionFactory = connectionFactory;

            // TODO: Free connections after a period of time rather than hanging on for ever.
        }

        /// <summary>
        ///     Gets the key identifying the type of connection supplied by this pool (connection string and owner).
        /// </summary>
        /// <value>
        ///     The key identifying the type of connection supplied by this pool (connection string and owner).
        /// </value>
        [NotNull]
        public ConnectionKey Key { get; }

        /// <summary>
        ///     Gets the connection factory to which this pool belongs.
        /// </summary>
        /// <value>
        ///     The connection factory to which this pool belongs.
        /// </value>
        public ConnectionFactory Factory => _connectionFactory;

        /// <summary>
        ///     Gets a new connection.
        /// </summary>
        /// <returns>
        /// </returns>
        [NotNull]
        internal Connection GetConnection()
        {
            var instanceId = _idleConnections.Keys.FirstOrDefault();

            if(_idleConnections.TryRemove(instanceId, out var connection))
            {
                return connection;
            }

            return new Connection(Key, _connectionFactory.InitializeAzureManagedIdentity, _connectionFactory, this, _services.GetService<ILogger<Connection>>());
        }

        /// <summary>
        ///     Recycles the connection given.
        /// </summary>
        /// <param name="connection">
        ///     The connection to recycle (must come from this pool).
        /// </param>
        internal void RecycleConnection([NotNull] Connection connection)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            if(_activeConnections.TryRemove(connection.InstanceId, out var removedConnection))
#pragma warning restore CA2000 // Dispose objects before losing scope
            {
                Debug.Assert(false, $@"Unable to remove connection from active connections because not item with the given key exists: {connection.InstanceId}.");

                Debug.Assert(!ReferenceEquals(removedConnection, null), $@"Connection remove from the active connections was null: {connection.InstanceId}.");

                Debug.Assert(ReferenceEquals(connection, removedConnection), $@"Connection remove from the active connections was not the same as the connection provided: {connection.InstanceId}.");
            }

            if(connection.Recycle())
            {
                if(!_idleConnections.TryAdd(connection.InstanceId, connection))
                {
                    Debug.Assert(false, $@"Unable to add connection to idle connections because an item with the same key already exists: {connection.InstanceId}.");
                }
            }
            else
            {
                connection.Dispose();
            }
        }

        /// <summary>
        ///     Permanently removes the connection given from the pool.
        /// </summary>
        /// <param name="connection">
        ///     The connection to remove.
        /// </param>
        internal void RemoveConnection([NotNull] Connection connection)
        {
            throw new NotImplementedException();
        }
    }
}