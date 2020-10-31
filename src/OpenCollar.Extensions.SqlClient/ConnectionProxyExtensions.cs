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

using System.Data;

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Extensions to the <see cref="ConnectionProxy" /> class providing fluent methods for creating commands.
    /// </summary>
    public static class ConnectionProxyExtensions
    {
        /// <summary>
        ///     Creates a new execution context builder, specifying that the stored procedure named will be executed on
        ///     the connection provided..
        /// </summary>
        /// <param name="connection">
        ///     The connection on which the command will be executed.
        /// </param>
        /// <param name="storedProcedure">
        ///     The name of the stored procedure to execute.
        /// </param>
        /// <returns>
        ///     An <see cref="QueryBuilder"> builder </see> that can be called with further extensions to register
        ///     additional details of how the command will be processed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="connection" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="storedProcedure" /> is <see langword="null" />.
        /// </exception>
        public static QueryBuilder QueryProcedure([NotNull] this ConnectionProxy connection, [NotNull] Model.Identifier storedProcedure)
        {
            connection.Validate(nameof(connection), ObjectIs.NotNull);
            storedProcedure.Validate(nameof(storedProcedure), ObjectIs.NotNull);

            return new QueryBuilder(connection, CommandType.StoredProcedure, storedProcedure);
        }
    }
}