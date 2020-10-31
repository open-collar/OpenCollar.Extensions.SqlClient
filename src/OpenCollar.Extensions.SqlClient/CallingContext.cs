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
using System.Collections.Generic;
using System.Threading;

using JetBrains.Annotations;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     Defines the context in which a call to a service is being made.
    /// </summary>
    public sealed class CallingContext : IEquatable<CallingContext>, IComparable<CallingContext>, IComparable
    {
        /// <summary>
        ///     Defines the context for the current thread.
        /// </summary>
        /// <remarks>
        ///     We use the <see cref="AsyncLocal{T}" /> class to ensure that the value is local to the current task.
        /// </remarks>
        [CanBeNull]
        private static AsyncLocal<CallingContext?> _threadContext = new AsyncLocal<CallingContext?>();

        /// <summary>
        ///     Gets the context for the current thread, initializing it from the state of the an existing context (e.g.
        ///     the parent thread's context when a child thread is created).
        /// </summary>
        /// <returns>
        ///     The context for the current thread.
        /// </returns>
        [NotNull]
        public static CallingContext Current()
        {
            if(!ReferenceEquals(_threadContext.Value, null))
            {
                return _threadContext.Value;
            }

            var context = new CallingContext();

            _threadContext.Value = context;

            return context;
        }

        /// <summary>
        ///     Sets the active query builder.
        /// </summary>
        /// <param name="builder">
        ///     The active query builder.
        /// </param>
        /// <returns>
        ///     Returns the calling context, allowing for fluent-style semantics.
        /// </returns>
        internal CallingContext SetActiveBuilder([CanBeNull] QueryBuilder? builder)
        {
            Builder = builder;
            return this;
        }

        [CanBeNull]
        public QueryBuilder? Builder { get; private set; }

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

            return obj is CallingContext other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(CallingContext)}");
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
        public int CompareTo(CallingContext other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }

            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(Owner, other.Owner, StringComparison.OrdinalIgnoreCase);
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
        public bool Equals(CallingContext other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Owner, other.Owner, StringComparison.OrdinalIgnoreCase);
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
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is CallingContext other && Equals(other);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode() => Owner != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Owner) : 0;

        /// <summary>
        ///     Returns a value that indicates whether the values of two
        ///     <see cref="CallingContext" /> objects are equal.
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
        public static bool operator ==(CallingContext left, CallingContext right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="CallingContext" />
        ///     value is greater than another <see cref="CallingContext" /> value.
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
        public static bool operator >(CallingContext left, CallingContext right) => Comparer<CallingContext>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="CallingContext" />
        ///     value is greater than or equal to another <see cref="CallingContext" /> value.
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
        public static bool operator >=(CallingContext left, CallingContext right) => Comparer<CallingContext>.Default.Compare(left, right) >= 0;

        /// <summary>
        ///     Returns a value that indicates whether two <see cref="CallingContext" />
        ///     objects have different values.
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
        public static bool operator !=(CallingContext left, CallingContext right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="CallingContext" />
        ///     value is less than another <see cref="CallingContext" /> value.
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
        public static bool operator <(CallingContext left, CallingContext right) => Comparer<CallingContext>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="CallingContext" />
        ///     value is less than or equal to another <see cref="CallingContext" /> value.
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
        public static bool operator <=(CallingContext left, CallingContext right) => Comparer<CallingContext>.Default.Compare(left, right) <= 0;

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var emailAddress = Owner;
            if(string.IsNullOrWhiteSpace(emailAddress))
            {
                return @"[null]";
            }

#pragma warning disable CA1308 // Normalize strings to uppercase
            return emailAddress.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}