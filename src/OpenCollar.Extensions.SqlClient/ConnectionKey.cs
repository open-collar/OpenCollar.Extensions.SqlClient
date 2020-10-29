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
using System.Collections.Generic;

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A class used to identify a connection created for an individual user.
    /// </summary>
    /// <seealso cref="System.IEquatable{T}" />
    /// <seealso cref="System.IComparable{T}" />
    /// <seealso cref="System.IComparable" />
    internal sealed class ConnectionKey : IEquatable<ConnectionKey>, IComparable<ConnectionKey>, IComparable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionKey" /> class.
        /// </summary>
        /// <param name="owner">
        ///     The owner of the connection, for example an email address. Used to manage the reuse of connections in
        ///     the connection pool. Can be left empty or <see langword="null" /> if the connection isn't tied to a
        ///     single user.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string used to connect to the database.
        /// </param>
        public ConnectionKey([CanBeNull] string owner, [NotNull] string connectionString)
        {
            connectionString.Validate(nameof(connectionString), StringIs.NotNullEmptyOrWhiteSpace);

            Owner = owner;
            ConnectionString = connectionString;
        }

        /// <summary>
        ///     Gets the connection string used to connect to the database.
        /// </summary>
        /// <value>
        ///     The connection string used to connect to the database.
        /// </value>
        [NotNull]
        public string ConnectionString { get; }

        /// <summary>
        ///     Gets the owner of the connection, for example an email address.
        /// </summary>
        /// <value>
        ///     The owner of the connection, for example an email address. Used to manage the reuse of connections in
        ///     the connection pool. Can be left empty or <see langword="null" /> if the connection isn't tied to a
        ///     single user.
        /// </value>
        [CanBeNull]
        public string Owner { get; }

        /// <summary>
        ///     Returns a value that indicates whether two <see cref="ConnectionKey" /> objects have different values.
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
        public static bool operator !=(ConnectionKey left, ConnectionKey right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ConnectionKey" /> value is less than another
        ///     <see cref="ConnectionKey" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.
        /// </returns>
        public static bool operator <(ConnectionKey left, ConnectionKey right) => Comparer<ConnectionKey>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ConnectionKey" /> value is less than or equal to
        ///     another <see cref="ConnectionKey" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.
        /// </returns>
        public static bool operator <=(ConnectionKey left, ConnectionKey right) => Comparer<ConnectionKey>.Default.Compare(left, right) <= 0;

        /// <summary>
        ///     Returns a value that indicates whether the values of two <see cref="ConnectionKey" /> objects are equal.
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
        public static bool operator ==(ConnectionKey left, ConnectionKey right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ConnectionKey" /> value is greater than another
        ///     <see cref="ConnectionKey" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.
        /// </returns>
        public static bool operator >(ConnectionKey left, ConnectionKey right) => Comparer<ConnectionKey>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="ConnectionKey" /> value is greater than or equal to
        ///     another <see cref="ConnectionKey" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.
        /// </returns>
        public static bool operator >=(ConnectionKey left, ConnectionKey right) => Comparer<ConnectionKey>.Default.Compare(left, right) >= 0;

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="obj">
        ///     An object to compare with this instance.
        /// </param>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///     <listheader>
        ///         <term> Value </term>
        ///         <description> Meaning </description>
        ///     </listheader>
        ///     <item>
        ///         <term> Less than zero </term>
        ///         <description> This instance precedes <paramref name="obj" /> in the sort order. </description>
        ///     </item>
        ///     <item>
        ///         <term> Zero </term>
        ///         <description> This instance occurs in the same position in the sort order as <paramref name="obj" />. </description>
        ///     </item>
        ///     <item>
        ///         <term> Greater than zero </term>
        ///         <description> This instance follows <paramref name="obj" /> in the sort order. </description>
        ///     </item>
        /// </list>
        /// <exception cref="System.ArgumentException">
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

            return obj is ConnectionKey other ? CompareTo(other) : throw new ArgumentException($@"Object must be of type {nameof(ConnectionKey)}");
        }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this instance.
        /// </param>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///     <listheader>
        ///         <term> Value </term>
        ///         <description> Meaning </description>
        ///     </listheader>
        ///     <item>
        ///         <term> Less than zero </term>
        ///         <description> This instance precedes <paramref name="other" /> in the sort order. </description>
        ///     </item>
        ///     <item>
        ///         <term> Zero </term>
        ///         <description> This instance occurs in the same position in the sort order as <paramref name="other" />. </description>
        ///     </item>
        ///     <item>
        ///         <term> Greater than zero </term>
        ///         <description> This instance follows <paramref name="other" /> in the sort order. </description>
        ///     </item>
        /// </list>
        public int CompareTo(ConnectionKey other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }

            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            var callersEmailAddressComparison = string.Compare(Owner, other.Owner, StringComparison.Ordinal);
            if(callersEmailAddressComparison != 0)
            {
                return callersEmailAddressComparison;
            }

            return string.Compare(ConnectionString, other.ConnectionString, StringComparison.Ordinal);
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
        public bool Equals(ConnectionKey other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return Owner == other.Owner && ConnectionString == other.ConnectionString;
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
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is ConnectionKey other && Equals(other);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Owner, ConnectionString);
    }
}