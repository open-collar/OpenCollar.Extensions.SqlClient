﻿/*
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

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

#pragma warning disable CA1032 // Implement standard exception constructors

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Defines an exception thrown within the context of a database connection.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class ConnectionException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionException" /> class.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string that defines the connection on which the error occurred.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public ConnectionException(string connectionString) : base()
        {
            connectionString.Validate(nameof(connectionString), StringIs.NotNullEmptyOrWhiteSpace);

            ConnectionString = connectionString;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionException" /> class.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string that defines the connection on which the error occurred.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public ConnectionException(string connectionString, string message) : base(message)
        {
            connectionString.Validate(nameof(connectionString), StringIs.NotNullEmptyOrWhiteSpace);

            ConnectionString = connectionString;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionException" /> class.
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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="connectionString" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="connectionString" /> is zero-length or contains only white-space characters.
        /// </exception>
        public ConnectionException(string connectionString, string message, Exception innerException) : base(message, innerException)
        {
            connectionString.Validate(nameof(connectionString), StringIs.NotNullEmptyOrWhiteSpace);

            ConnectionString = connectionString;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        protected ConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ConnectionString = info.GetString(nameof(ConnectionString));
        }

        /// <summary>
        ///     Gets the connection string that defines the connection on which the error occurred.
        /// </summary>
        /// <value>
        ///     The connection string that defines the connection on which the error occurred.
        /// </value>

        public string ConnectionString { get; }

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
            info.Validate(nameof(info), ObjectIs.NotNull);

            info.AddValue(nameof(ConnectionString), ConnectionString);
            base.GetObjectData(info, context);
        }
    }
}