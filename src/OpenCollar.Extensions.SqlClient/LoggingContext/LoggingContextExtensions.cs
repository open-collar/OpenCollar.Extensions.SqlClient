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

using Microsoft.Data.SqlClient;

namespace OpenCollar.Extensions.SqlClient.LoggingContext
{
    /// <summary>
    ///     Extensions methods for the <see cref="Logging.LoggingContext" /> class.
    /// </summary>
    public static class LoggingContextExtensions
    {
        /// <summary>
        ///     Adds the details of a submission to the logging context.
        /// </summary>
        /// <param name="loggingContext">
        ///     The logging context to which to add the details.
        /// </param>
        /// <param name="databaseConnection">
        ///     The name of the stored procedure being executed.
        /// </param>
        /// <returns>
        ///     The logging context provided, allowing fluent-style chaining of calls.
        /// </returns>
        public static Logging.LoggingContext? AddDatabaseConnection(this Logging.LoggingContext? loggingContext, string databaseConnection)
        {
            if(ReferenceEquals(loggingContext, null))
            {
                // No errors!
                return null;
            }

            loggingContext.AppendInfo(Constants.Keys.DatabaseConnection, databaseConnection);

            return loggingContext;
        }

        /// <summary>
        ///     Adds the details of a submission to the logging context.
        /// </summary>
        /// <param name="loggingContext">
        ///     The logging context to which to add the details.
        /// </param>
        /// <param name="storedProcedure">
        ///     The name of the stored procedure being executed.
        /// </param>
        /// <returns>
        ///     The logging context provided, allowing fluent-style chaining of calls.
        /// </returns>
        public static Logging.LoggingContext? AddStoredProcedure(this Logging.LoggingContext? loggingContext, string? storedProcedure)
        {
            if(ReferenceEquals(loggingContext, null))
            {
                // No errors!
                return null;
            }

            loggingContext.AppendInfo(Constants.Keys.StoredProcedure, storedProcedure);

            return loggingContext;
        }

        /// <summary>
        ///     Adds the details of a submission to the logging context.
        /// </summary>
        /// <param name="loggingContext">
        ///     The logging context to which to add the details.
        /// </param>
        /// <param name="command">
        ///     The SQL command from which to extract the stored procedure name.
        /// </param>
        /// <returns>
        ///     The logging context provided, allowing fluent-style chaining of calls.
        /// </returns>
        public static Logging.LoggingContext? AddStoredProcedure(this Logging.LoggingContext? loggingContext, SqlCommand? command)
        {
            return AddStoredProcedure(loggingContext, command?.CommandText);
        }
    }
}