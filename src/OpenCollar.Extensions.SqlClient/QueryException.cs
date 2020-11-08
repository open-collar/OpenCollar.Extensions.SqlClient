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
using System.Runtime.Serialization;

#pragma warning disable CA1032 // Implement standard exception constructors

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Defines an exception thrown within the context of query on a database connection.
    /// </summary>
    /// <seealso cref="DatabaseException" />
    [Serializable]
    public class QueryException : DatabaseException
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the query that caused this exception can safely be re-run.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the query that caused this exception can safely be re-run; otherwise, <see langword="false" />.
        /// </value>
        public bool CanRetry { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryException" /> class.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string that defines the connection on which the error occurred.
        /// </param>
        /// <param name="canRetry">
        ///     <see langword="true" /> if the query that caused this exception can safely be re-run; otherwise,
        ///     <see langword="false" />. Defaults to <see langword="false" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public QueryException(string connectionString, bool canRetry = false) : base(connectionString)
        {
            CanRetry = canRetry;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryException" /> class.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string that defines the connection on which the error occurred.
        /// </param>
        /// <param name="canRetry">
        ///     <see langword="true" /> if the query that caused this exception can safely be re-run; otherwise,
        ///     <see langword="false" />. Defaults to <see langword="false" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public QueryException(string connectionString, string message, bool canRetry = false) : base(connectionString, message)
        {
            CanRetry = canRetry;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryException" /> class.
        /// </summary>
        /// <param name="message">
        ///     The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or <see langword="null" /> if no inner
        ///     exception is specified.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string that defines the connection on which the error occurred.
        /// </param>
        /// <param name="canRetry">
        ///     <see langword="true" /> if the query that caused this exception can safely be re-run; otherwise,
        ///     <see langword="false" />. Defaults to <see langword="false" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public QueryException(string connectionString, string message, Exception innerException, bool canRetry = false) : base(connectionString, message, innerException)
        {
            CanRetry = canRetry;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        protected QueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CanRetry = info.GetBoolean(nameof(CanRetry));
        }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(CanRetry), CanRetry);
        }
    }
}