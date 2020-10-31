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
using System.Data.SqlTypes;

using Microsoft.Data.SqlClient;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Extensions methods that provide safe mechanisms for reading values from a <see cref="SqlDataReader" />.
    /// </summary>
    public static class SqlReaderExtensions
    {
        /// <summary>
        ///     Gets a date time offset value from a data reader.
        /// </summary>
        /// <param name="reader">
        ///     The data reader from which to take the value.
        /// </param>
        /// <param name="index">
        ///     The index of the field to read.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value to return if the field contains <see cref="DBNull" />.
        /// </param>
        /// <returns>
        ///     The value read from the field, as a datetime offset.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Unable to convert from field type to <see cref="DateTimeOffset" />.
        /// </exception>
        public static DateTimeOffset? GetDateTimeOffsetOrDefault(this SqlDataReader reader, int index, DateTimeOffset? defaultValue)
        {
            reader.Validate(nameof(reader), ObjectIs.NotNull);
            if(reader.IsDBNull(index))
            {
                return defaultValue;
            }

            var type = reader.GetFieldType(index);

            if(type == typeof(DateTimeOffset))
            {
                return reader.GetDateTimeOffset(index);
            }

            if(type == typeof(DateTime))
            {
                return DateTimeOffset.FromFileTime(reader.GetDateTime(index).ToFileTime());
            }

            throw new InvalidCastException($"Unable to convert from {type.FullName} to {typeof(DateTimeOffset).FullName}.");
        }

        /// <summary>
        ///     Safely reads a value from the field specified in the reader.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value returned.
        /// </typeparam>
        /// <param name="reader">
        ///     The reader from which to read the value.
        /// </param>
        /// <param name="index">
        ///     The index of the field to read.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value to return if the field contains <c> NULL </c>.
        /// </param>
        /// <returns>
        ///     The value of the field or <paramref name="defaultValue" /> if the field contains <see cref="DBNull" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="reader" /> was <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> must be in range.
        /// </exception>
        public static T GetValueOrDefault<T>(this SqlDataReader reader, int index, T defaultValue)
        {
            reader.Validate(nameof(reader), ObjectIs.NotNull);

            if(index >= reader.FieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    $"'index' must be in the range 0-{reader.FieldCount - 1}.  Dataset returned by \"{GetSql()}\".");
            }

            if(reader.IsDBNull(index))
            {
                return defaultValue;
            }

            return GetStronglyTypeFieldValue<T>(reader, index);
        }

        /// <summary>
        ///     Safely reads a value from the field specified in the reader or throws a detailed exception if it cannot
        ///     be read.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value returned.
        /// </typeparam>
        /// <param name="reader">
        ///     The reader from which to read the value.
        /// </param>
        /// <param name="index">
        ///     The index of the field to read.
        /// </param>
        /// <returns>
        ///     The value of the field.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="reader" /> was <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> must be in range.
        /// </exception>
        /// <exception cref="SqlNullValueException">
        ///     Field of dataset contains NULL.
        /// </exception>
        public static T GetValueOrFail<T>(this SqlDataReader reader, int index)
        {
            reader.Validate(nameof(reader), ObjectIs.NotNull);

            if(index >= reader.FieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    $"'index' must be in the range 0-{reader.FieldCount - 1}.  Dataset returned by \"{GetSql()}\".");
            }

            if(reader.IsDBNull(index))
            {
                throw new SqlNullValueException($@"Field {index} ([{reader.GetName(index)}]) of dataset returned by ""{GetSql()}"" contains NULL.");
            }

            try
            {
                return GetStronglyTypeFieldValue<T>(reader, index);
            }
            catch(InvalidCastException ex)
            {
                // Give a really detailed error, because this can be a pain to debug.
                var value = reader.GetValue(index).ToString();
                if(reader.GetFieldType(index) == typeof(string))
                {
                    value = string.Concat(@"""", value, @"""");
                }

                throw new InvalidCastException(
                    $@"Could not convert value in field {index} ([{reader.GetName(index)}] = {value}) of dataset returned by ""{GetSql()}"" from {reader.GetFieldType(index).FullName} to {typeof(T).FullName}.  See inner exception for further details.",
                    ex);
            }
        }

        /// <summary>
        ///     Gets the active SQL.
        /// </summary>
        /// <returns>
        ///     The active SQL.
        /// </returns>

        private static string GetSql()
        {
            var context = CallingContext.Current();
            if(context is null)
            {
                return @"[Unspecified]";
            }

            var query = context.Query;
            if(query is null)
            {
                return @"[Unspecified]";
            }

            var commandText = query.CommandText;
            if(commandText is null)
            {
                return @"[Unspecified]";
            }

            return commandText.ToString();
        }

        /// <summary>
        ///     Gets the value of a field as a strongly typed value.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the field to read.
        /// </typeparam>
        /// <param name="reader">
        ///     The reader from which to take the value.
        /// </param>
        /// <param name="index">
        ///     The index of the field to read.
        /// </param>
        /// <returns>
        ///     Returns the value of the field specified.
        /// </returns>
        private static T GetStronglyTypeFieldValue<T>(SqlDataReader reader, int index)
        {
            var t = typeof(T);

            if(t.IsGenericType && t.GenericTypeArguments.Length == 1 && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If the argument is nullable then use the correct underlying type to determine the conversion.
                t = t.GenericTypeArguments[0];
            }

            switch(Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                    return (T)(object)reader.GetBoolean(index);

                case TypeCode.Byte:
                    return (T)(object)reader.GetByte(index);

                case TypeCode.Char:
                    return (T)(object)reader.GetChar(index);

                case TypeCode.DateTime:
                    return (T)(object)reader.GetDateTime(index);

                case TypeCode.Decimal:
                    return (T)(object)reader.GetDecimal(index);

                case TypeCode.Double:
                    return (T)(object)reader.GetDouble(index);

                case TypeCode.Int16:
                    return (T)(object)reader.GetInt16(index);

                case TypeCode.Int32:
                    return (T)(object)reader.GetInt32(index);

                case TypeCode.Int64:
                    return (T)(object)reader.GetInt64(index);

                case TypeCode.Object:
                    return (T)reader.GetValue(index);

                case TypeCode.Single:
                    return (T)(object)reader.GetFloat(index);

                case TypeCode.String:
                    return (T)(object)reader.GetString(index);

                default:
                    if(t == typeof(DateTimeOffset))
                    {
                        return (T)(object)reader.GetDateTimeOffset(index);
                    }

                    if(t == typeof(Guid))
                    {
                        return (T)(object)reader.GetGuid(index);
                    }

                    if(t == typeof(TimeSpan))
                    {
                        return (T)(object)reader.GetTimeSpan(index);
                    }

                    break;
            }

            return (T)reader.GetValue(index);
        }
    }
}