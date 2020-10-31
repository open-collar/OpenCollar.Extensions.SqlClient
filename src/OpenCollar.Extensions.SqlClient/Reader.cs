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

using Microsoft.Data.SqlClient;

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Represnts a function that reads a set of results from a data reader.
    /// </summary>
    internal sealed class Reader
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Reader" /> class.
        /// </summary>
        /// <param name="function">
        ///     The function that will be executed to read data from a reader.
        /// </param>
        /// <param name="returnType">
        ///     Type of the returned value.
        /// </param>
        /// <param name="isMandatory">
        ///     <see langword="true" /> if least one record must be returned in the recordset; otherwise, <see langword="false" />.
        /// </param>
        private Reader(Func<SqlDataReader, object?> function, Type returnType, bool isMandatory)
        {
            Function = function;
            ReturnType = returnType;
            IsMandatory = isMandatory;
        }

        /// <summary>
        ///     Gets the function that will be executed to read data from a reader.
        /// </summary>
        /// <value>
        ///     The function that will be executed to read data from a reader.
        /// </value>
        public Func<SqlDataReader, object?> Function { get; }

        /// <summary>
        ///     Gets a value indicating whether at least one record must be returned in the recordset.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if least one record must be returned in the recordset; otherwise, <see langword="false" />.
        /// </value>
        public bool IsMandatory { get; }

        /// <summary>
        ///     Gets the type of the returned value.
        /// </summary>
        /// <value>
        ///     The type of the returned value.
        /// </value>
        public Type ReturnType { get; }

        /// <summary>
        ///     Gets a new reader for the values given.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the returned value.
        /// </typeparam>
        /// <param name="readAction">
        ///     The function that will be executed to read data from a reader.
        /// </param>
        /// <param name="isMandatory">
        ///     <see langword="true" /> if least one record must be returned in the recordset; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     The object representing the value given.
        /// </returns>
        internal static Reader GetReader<T>(Func<SqlDataReader, T> readAction, bool isMandatory = false)
        {
            var returnType = typeof(T);
            Func<SqlDataReader, object?> reader = (r) => readAction(r);
            return new Reader(reader, returnType, isMandatory);
        }

        /// <summary>
        ///     Executes the function against the specified reader.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the result returned.
        /// </typeparam>
        /// <param name="reader">
        ///     The data reader against which to execute the function..
        /// </param>
        /// <returns>
        ///     The value read.
        /// </returns>
        internal T Execute<T>(SqlDataReader reader)
        {
            if(ReturnType.IsAssignableFrom(typeof(T)))
            {
                throw new TypeMismatchException("The type specified does not match that specified at the time of creation.");
            }

            return (T)Function(reader);
        }

        /// <summary>
        ///     Executes the specified reader.
        /// </summary>
        /// <param name="reader">
        ///     The reader.
        /// </param>
        /// <returns>
        /// </returns>
        internal object? Execute(SqlDataReader reader)
        {
            var result = Function(reader);

            if(!(result is null) && !ReturnType.IsAssignableFrom(result.GetType()))
            {
                throw new TypeMismatchException("The type specified does not match that specified at the time of creation.");
            }

            return result;
        }
    }
}