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
using System.Runtime.Serialization;

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     An exception that is thrown as a result of a database interaction.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
#pragma warning disable CA1032 // Implement standard exception constructors - other standard constructors are meaningless in this instance.
    public class DbException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbException"> </see> class.
        /// </summary>
        /// <param name="details">
        ///     The details of the error and the environment at the time.
        /// </param>
        public DbException([CanBeNull] string details)
        {
            Details = details;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbException"> </see> class with a specified error message.
        /// </summary>
        /// <param name="details">
        ///     The details of the error and the environment at the time.
        /// </param>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        public DbException([NotNull] string details, string message) : base(message)
        {
            Details = details;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbException"> </see> class with a specified error message
        ///     and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="details">
        ///     The details of the error and the environment at the time.
        /// </param>
        /// <param name="message">
        ///     The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or <see langword="null" /> if no inner
        ///     exception is specified.
        /// </param>
        public DbException([NotNull] string details, string message, Exception innerException) : base(message, innerException)
        {
            Details = details;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbException"> </see> class with serialized data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"> </see> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"> </see> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="info"> info </paramref> parameter is <see langword="null" />.
        /// </exception>
        /// <exception cref="SerializationException">
        ///     The class name is null or <see cref="Exception.HResult"> </see> is zero (0).
        /// </exception>
        protected DbException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Details = info.GetString(nameof(Details));
        }

        /// <summary>
        ///     Gets the details of the error and the environment at the time.
        /// </summary>
        /// <value>
        ///     The details of the error and the environment at the time.
        /// </value>
        [CanBeNull]
        public string Details { get; }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="SerializationInfo">
        ///     </see> with information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"> </see> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"> </see> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="info"> info </paramref> parameter is a <see langword="null" />.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.Validate(nameof(info), ObjectIs.NotNull);

            info.AddValue(nameof(Details), Details);

            base.GetObjectData(info, context);
        }
    }
}