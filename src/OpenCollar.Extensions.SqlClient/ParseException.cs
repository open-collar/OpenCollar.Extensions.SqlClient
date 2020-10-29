/*
 * This file is part of OpenCollar.Extensions.
 *
 * OpenCollar.Extensions is free software: you can redistribute it
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
#pragma warning disable CA1032 // Add standard constructors.

    /// <summary>
    ///     A class used to represent an exception that occurs when a SQL identifier cannot be parsed .
    /// </summary>
    /// <seealso cref="ParseException" />
    [Serializable]
    public class ParseException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseException"> </see> class with a specified error message.
        /// </summary>
        /// <param name="characterPosition">
        ///     The zero-based position of the character at which the error was found.
        /// </param>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        public ParseException(int characterPosition, string message) : base(message)
        {
            CharacterPosition = characterPosition;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseException"> </see> class with a specified error
        ///     message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="characterPosition">
        ///     The zero-based position of the character at which the error was found.
        /// </param>
        /// <param name="message">
        ///     The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or <see langword="null" /> if no inner
        ///     exception is specified.
        /// </param>
        public ParseException(int characterPosition, string message, Exception innerException) : base(message, innerException)
        {
            CharacterPosition = characterPosition;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseException"> </see> class.
        /// </summary>
        /// <param name="characterPosition">
        ///     The zero-based position of the character at which the error was found.
        /// </param>
        public ParseException(int characterPosition) : base($@"Error found at character: ""{characterPosition}"".")
        {
            CharacterPosition = characterPosition;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseException"> </see> class with serialized data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="System.Runtime.Serialization.SerializationInfo"> </see> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="System.Runtime.Serialization.StreamingContext"> </see> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     The <paramref name="info"> info </paramref> parameter is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        ///     The class name is null or <see cref="System.Exception.HResult"> </see> is zero (0).
        /// </exception>
        protected ParseException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CharacterPosition = info.GetInt32(nameof(CharacterPosition));
        }

        /// <summary>
        ///     Gets the zero-based position of the character at which the error was found.
        /// </summary>
        /// <value>
        ///     The zero-based position of the character at which the error was found.
        /// </value>
        public int CharacterPosition { get; }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo">
        ///     </see> with information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="System.Runtime.Serialization.SerializationInfo"> </see> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="System.Runtime.Serialization.StreamingContext"> </see> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     The <paramref name="info"> info </paramref> parameter is <see langword="null" />.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.Validate(nameof(info), ObjectIs.NotNull);

            info.AddValue(nameof(CharacterPosition), CharacterPosition);
            base.GetObjectData(info, context);
        }
    }
}