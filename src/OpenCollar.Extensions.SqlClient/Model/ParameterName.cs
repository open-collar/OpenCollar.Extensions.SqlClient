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
using System.Collections.Generic;
using System.Diagnostics;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient.Model
{
    /// <summary>
    ///     Represents a valid, normalized, SQL argument name.
    /// </summary>
    /// <seealso cref="IEquatable{T}" />
    /// <seealso cref="IComparable{T}" />
    /// <seealso cref="IComparable" />
    [DebuggerDisplay("{" + nameof(_normalizedValue) + ",nq}")]
    public sealed class ParameterName : IEquatable<ParameterName>, IComparable<ParameterName>, IComparable
    {
        /// <summary>
        ///     The normalized value used in comparisons and generated SQL.
        /// </summary>

        private readonly string _normalizedValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParameterName" /> class.
        /// </summary>
        /// <param name="originalValue">
        ///     The original value given as the identifier.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="originalValue" /> was <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="originalValue" /> was zero-length or contains only white-space characters.
        /// </exception>
        public ParameterName(string originalValue)
        {
            originalValue.Validate(nameof(originalValue), StringIs.NotNullEmptyOrWhiteSpace);

            OriginalValue = originalValue;

            _normalizedValue = NormalizedValue(originalValue);
        }

        /// <summary>
        ///     Gets the original value passed to the constructor.
        /// </summary>
        /// <value>
        ///     The original value passed to the constructor.
        /// </value>

        public string OriginalValue { get; }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="ParameterName" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator ParameterName?(string? value) => string.IsNullOrWhiteSpace(value) ? null : new ParameterName(value);

        /// <summary>
        ///     Performs an implicit conversion from <see cref="ParameterName" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator string?(ParameterName? value) => ReferenceEquals(value, null) ? null : value.ToString();

        /// <summary>
        ///     Returns a value that indicates whether two <see cref="ParameterName" /> objects have different values.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(ParameterName left, ParameterName right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ParameterName" /> value is less than another
        ///     <see cref="ParameterName" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <(ParameterName left, ParameterName right) => Comparer<ParameterName>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ParameterName" /> value is less than or equal to
        ///     another <see cref="ParameterName" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <=(ParameterName left, ParameterName right) => Comparer<ParameterName>.Default.Compare(left, right) <= 0;

        /// <summary>
        ///     Returns a value that indicates whether the values of two <see cref="ParameterName" /> objects are equal.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have
        ///     the same value; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(ParameterName left, ParameterName right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ParameterName" /> value is greater than another
        ///     <see cref="ParameterName" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >(ParameterName left, ParameterName right) => Comparer<ParameterName>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ParameterName" /> value is greater than or equal to
        ///     another <see cref="ParameterName" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >=(ParameterName left, ParameterName right) => Comparer<ParameterName>.Default.Compare(left, right) >= 0;

        /// <summary>
        ///     Converts from <see cref="System.String" /> to <see cref="ParameterName" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static ParameterName? ToParameterName(string? value) => string.IsNullOrWhiteSpace(value) ? null : new ParameterName(value);

        /// <summary>
        ///     Converts from <see cref="ParameterName" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static string? ToString(ParameterName? value) => ReferenceEquals(value, null) ? null : value.ToString();

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="obj">
        ///     An object to compare with this instance.
        /// </param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value </term>
        ///             <description> Meaning </description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero </term>
        ///             <description> This instance precedes <paramref name="obj" /> in the sort order. </description>
        ///         </item>
        ///         <item>
        ///             <term> Zero </term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="obj" />. </description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero </term>
        ///             <description> This instance follows <paramref name="obj" /> in the sort order. </description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        public int CompareTo(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return 1;
            }

            if(ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ParameterName other ? CompareTo(other) : throw new ArgumentException($@"Object must be of type {nameof(ParameterName)}");
        }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this instance.
        /// </param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value </term>
        ///             <description> Meaning </description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero </term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order. </description>
        ///         </item>
        ///         <item>
        ///             <term> Zero </term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />. </description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero </term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order. </description>
        ///         </item>
        ///     </list>
        /// </returns>
        public int CompareTo(ParameterName other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }

            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(_normalizedValue, other._normalizedValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(ParameterName other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(_normalizedValue, other._normalizedValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">
        ///     The object to compare with the current object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is ParameterName other && Equals(other);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_normalizedValue);

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString() => _normalizedValue;

        /// <summary>
        ///     Normalizes the value given, standardizing the quotes and formatting.
        /// </summary>
        /// <param name="originalValue">
        ///     The original value to normalize.
        /// </param>
        /// <returns>
        ///     The normalized value with standardized quotes and formatting.
        /// </returns>

        private static string NormalizedValue(string originalValue)
        {
            if(originalValue.StartsWith("@", StringComparison.Ordinal))
            {
                return originalValue;
            }

            return string.Concat("@", originalValue);
        }
    }
}