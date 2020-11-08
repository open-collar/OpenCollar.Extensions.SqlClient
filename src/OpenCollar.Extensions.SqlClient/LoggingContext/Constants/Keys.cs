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

namespace OpenCollar.Extensions.SqlClient.Constants
{
    /// <summary>
    ///     Constant values used to identify the contextual information keys.
    /// </summary>
    public static class Keys
    {
        /// <summary>
        ///     An calling user's email address.
        /// </summary>
        public const string CallingUserEmailAddress = @"Cpt.Calling.User.Email";

        /// <summary>
        ///     The details of the database connection being used to access a database.
        /// </summary>
        public const string DatabaseConnection = @"Db.Connection";

        /// <summary>
        ///     The name of the database being access.
        /// </summary>
        public const string DatabaseName = @"Db.Name";

        /// <summary>
        ///     The SQL being executed.
        /// </summary>
        public const string DatabaseSql = @"Db.Sql";

        /// <summary>
        ///     The URI of the request being processed.
        /// </summary>
        public const string RequestUri = @"Request.Uri";

        /// <summary>
        ///     The name of the stored procedure being executed.
        /// </summary>
        public const string StoredProcedure = @"Db.Sql";
    }
}