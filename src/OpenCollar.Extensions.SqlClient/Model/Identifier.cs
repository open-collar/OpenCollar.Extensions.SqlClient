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
using System.Diagnostics;
using System.Text;

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Extensions.SqlClient.Model
{
    /// <summary>
    ///     Represents a valid, normalized, SQL identifier.
    /// </summary>
    /// <seealso cref="IEquatable{T}" />
    /// <seealso cref="IComparable{T}" />
    /// <seealso cref="IComparable" />
    [DebuggerDisplay("{" + nameof(OriginalValue) + ",nq}")]
    public sealed class Identifier : IEquatable<Identifier>, IComparable<Identifier>, IComparable
    {
        /// <summary>
        ///     The normalized value used in comparisons and generated SQL.
        /// </summary>
        [NotNull]
        private readonly string _normalizedValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Identifier" /> class.
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
        /// <exception cref="ParseException">
        ///     0 - Invalid identifier, first character is '.'.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, last character is '.'.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: un-escaped '[' found, and brackets already inserted.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: closing '\"' found with no corresponding opening character.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: un-escaped ']' found, and no brackets yet inserted.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: closing ']' found with no corresponding '['.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, no closing quote found before end of string.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, no closing bracket found before of string.
        /// </exception>
        public Identifier([NotNull] string originalValue)
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
        [NotNull]
        public string OriginalValue { get; }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="Identifier" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        [CanBeNull]
        public static implicit operator Identifier(string value) => string.IsNullOrWhiteSpace(value) ? null : new Identifier(value);

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Identifier" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        [CanBeNull]
        public static implicit operator string(Identifier value) => ReferenceEquals(value, null) ? null : value.ToString();

        /// <summary>
        ///     Returns a value that indicates whether two
        ///     <see cref="Identifier" /> objects have different values.
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
        public static bool operator !=(Identifier left, Identifier right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="Identifier" />
        ///     value is less than another <see cref="Identifier" /> value.
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
        public static bool operator <(Identifier left, Identifier right) => Comparer<Identifier>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="Identifier" />
        ///     value is less than or equal to another <see cref="Identifier" /> value.
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
        public static bool operator <=(Identifier left, Identifier right) => Comparer<Identifier>.Default.Compare(left, right) <= 0;

        /// <summary>
        ///     Returns a value that indicates whether the values of two
        ///     <see cref="Identifier" /> objects are equal.
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
        public static bool operator ==(Identifier left, Identifier right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="Identifier" />
        ///     value is greater than another <see cref="Identifier" /> value.
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
        public static bool operator >(Identifier left, Identifier right) => Comparer<Identifier>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="Identifier" />
        ///     value is greater than or equal to another
        ///     <see cref="Identifier" /> value.
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
        public static bool operator >=(Identifier left, Identifier right) => Comparer<Identifier>.Default.Compare(left, right) >= 0;

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

            return obj is Identifier other ? CompareTo(other) : throw new ArgumentException($@"Object must be of type {nameof(Identifier)}");
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
        public int CompareTo(Identifier other)
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
        public bool Equals(Identifier other)
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
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Identifier other && Equals(other);

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
        /// <exception cref="ParseException">
        ///     0 - Invalid identifier, first character is '.'.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, last character is '.'.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: un-escaped '[' found, and brackets already inserted.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: closing '\"' found with no corresponding opening character.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: un-escaped ']' found, and no brackets yet inserted.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid character found: closing ']' found with no corresponding '['.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, no closing quote found before end of string.
        /// </exception>
        /// <exception cref="ParseException">
        ///     Invalid identifier, no closing bracket found before of string.
        /// </exception>
        [NotNull]
        private string NormalizedValue([NotNull] string originalValue)
        {
            var state = new IdentifierTokenizer();

            if(originalValue[0] == '.')
            {
                throw new ParseException(0, "Invalid identifier, first character is '.'.");
            }

            if(originalValue[originalValue.Length - 1] == '.')
            {
                throw new ParseException(originalValue.Length - 1, "Invalid identifier, last character is '.'.");
            }

            foreach(var c in originalValue)
            {
                state.Parse(c);
            }

            state.Complete();

            return state.ToString();
        }

        /// <summary>
        ///     A class used to maintain the state when parsing an SQL identifier.
        /// </summary>
        private class IdentifierTokenizer
        {
            /// <summary>
            ///     The string builder that will be used to construct the parse version of the identifier.
            /// </summary>
            private readonly StringBuilder _builder = new StringBuilder();

            /// <summary>
            ///     <see langword="true" /> when the last character was a '.'; otherwise, <see langword="false" />. Used
            ///     to prevent multiple separator sequences outside of the quotes.
            /// </summary>
            private bool _lastCharWasSeparator;

            /// <summary>
            ///     <see langword="true" /> when an open-square-bracket character has been read and no corresponding
            ///     close-square-bracket has yet been parsed; otherwise, <see langword="false" />.
            /// </summary>
            private bool _openBracketRead;

            /// <summary>
            ///     <see langword="true" /> when an open-square-bracket character has been written and no corresponding
            ///     close-square-bracket has yet been written; otherwise, <see langword="false" />.
            /// </summary>
            private bool _openBracketWritten;

            /// <summary>
            ///     <see langword="true" /> when a double-quote character has been read and no corresponding closing
            ///     double-quote has yet been parsed; otherwise, <see langword="false" />.
            /// </summary>
            private bool _openQuoteRead;

            /// <summary>
            ///     The index from which the character being parsed was read.
            /// </summary>
            private int _sourceIndex;

            /// <summary>
            ///     The "stack" - in this case a single entry representing the last "special" character read and not yet
            ///     written (used to allow escaped sequences).
            /// </summary>
            private char? _stack;

            /// <summary>
            ///     Completes any outstanding processing to be performed (e.g. closing quotes, etc.).
            /// </summary>
            /// <exception cref="ParseException">
            ///     Invalid identifier, no closing quote found before end of string.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid identifier, no closing bracket found before of string.
            /// </exception>
            public void Complete()
            {
                ProcessStack();

                if(_openBracketWritten)
                {
                    if(_openQuoteRead)
                    {
                        throw new ParseException(-1, "Invalid identifier, no closing quote found before end of string.");
                    }

                    if(_openBracketRead)
                    {
                        throw new ParseException(-1, "Invalid identifier, no closing bracket found before of string.");
                    }

                    _builder.Append(']');
                    _openBracketWritten = false;
                }
            }

            /// <summary>
            ///     Parses the next character from the source string.
            /// </summary>
            /// <param name="c">
            ///     The character to parse.
            /// </param>
            /// <exception cref="ParseException">
            ///     Invalid character found: multiple '\".' found, no opening quote found outside of quotes or brackets.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-matched '\"' found, no opening quote found.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-matched ']' found, no opening bracket found.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-escaped '[' found, and brackets already inserted.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: closing '\"' found with no corresponding opening character.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-escaped ']' found, and no brackets yet inserted.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: closing ']' found with no corresponding '['.
            /// </exception>
            public void Parse(char c)
            {
                /*
                 * entity               => [entity]
                 * [entity]             => [entity]
                 * "entity"             => [entity]
                 * [special]]]          => [special]]]
                 * "special]]"          => [special]]]
                 * "special"""          => [special""]
                 * "schema.entity"      => [schema.entity]
                 * [schema.entity]      => [schema.entity]
                 * schema.entity        => [schema].[entity]
                 * "schema".entity      => [schema].[entity]
                 * schema."entity"      => [schema].[entity]
                 * [schema].entity      => [schema].[entity]
                 * schema.[entity]      => [schema].[entity]
                 * [schema]."entity"    => [schema].[entity]
                 * "schema".[entity]    => [schema].[entity]
                 */
                switch(c)
                {
                    case '[':
                    case '\"':
                    case ']':
                        if(_stack.HasValue)
                        {
                            // If there is already a value on the stack then it must be processed, either as an escape
                            // or as a special character.
                            ProcessStack(c);
                        }
                        else
                        {
                            // We'll leave this in the stack and see if we need it later.
                            _stack = c;
                        }

                        break;

                    default:
                        ProcessNormal(c);
                        break;
                }

                ++_sourceIndex;
            }

            /// <summary>
            ///     Converts to string.
            /// </summary>
            /// <returns>
            ///     A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => _builder.ToString();

            /// <summary>
            ///     Processes the character given as a normal value that should be inserted (and escaped) as necessary.
            /// </summary>
            /// <param name="c">
            ///     The character to insert.
            /// </param>
            /// <exception cref="ParseException">
            ///     Invalid character found: multiple '\".' found, no opening quote found outside of quotes or brackets.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-matched '\"' found, no opening quote found.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-matched ']' found, no opening bracket found.
            /// </exception>
            private void ProcessNormal(char c)
            {
                // If there is anything waiting to be processed on the stack, it must be dealt with before we add the
                // character given.
                ProcessStack();

                switch(c)
                {
                    case '.':
                        if(_lastCharWasSeparator)
                        {
                            throw new ParseException(_sourceIndex,
                                $"Invalid character at position {_sourceIndex}: multiple '\".' found, no opening quote found outside of quotes or brackets.");
                        }

                        _lastCharWasSeparator = true;
                        if(_openBracketWritten && (_openQuoteRead || _openBracketRead))
                        {
                            // This is a point inside quotes, so just write it
                            _builder.Append('.');
                        }
                        else
                        {
                            if(_openBracketWritten)
                            {
                                _builder.Append("].");
                                _openBracketWritten = false;
                                _openQuoteRead = false;
                                _openBracketRead = false;
                            }
                            else
                            {
                                // There was no quote/bracket in the original string, so we inserted one. So we must
                                // close the bracket now.
                                _builder.Append('.');
                                _openBracketWritten = false;
                            }
                        }

                        break;

                    case '[':
                    case '\"':
                    case ']':
                        _lastCharWasSeparator = false;

                        // Escape special characters.
                        _builder.Append(c);
                        _builder.Append(c);
                        break;

                    default:
                        _lastCharWasSeparator = false;
                        if(!_openBracketWritten)
                        {
                            // If no quotes have yet been inserted then do so now.
                            _builder.Append('[');
                            _openBracketWritten = true;
                        }

                        _builder.Append(c);
                        break;
                }
            }

            /// <summary>
            ///     Processes the stack when a new escapable character is parsed.
            /// </summary>
            /// <param name="c">
            ///     The new escapable character.
            /// </param>
            private void ProcessStack(char c)
            {
                // Any of these characters could be an escape sequence, so let's check for doubling first. If the
                // previous character is the same then it is escaped and we write it verbatim.
                _stack = null;
                ProcessNormal(c);
            }

            /// <summary>
            ///     Processes the stack when a non-escapable character has been found and any stacked characters must be processed.
            /// </summary>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-escaped '[' found, and brackets already inserted.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: closing '\"' found with no corresponding opening character.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: un-escaped ']' found, and no brackets yet inserted.
            /// </exception>
            /// <exception cref="ParseException">
            ///     Invalid character found: closing ']' found with no corresponding '['.
            /// </exception>
            private void ProcessStack()
            {
                if(!_stack.HasValue)
                {
                    return;
                }

                // If there was something waiting in the stack then we can process it now, it's not escaped.
                switch(_stack)
                {
                    case '[':
                        if(_openBracketWritten)
                        {
                            throw new ParseException(_sourceIndex,
                                $"Invalid character at position {_sourceIndex}: un-escaped '[' found, and brackets already inserted.");
                        }

                        _builder.Append('[');
                        _openBracketWritten = true;
                        _openBracketRead = true;
                        _lastCharWasSeparator = false;
                        break;

                    case '"':
                        if(_openBracketWritten)
                        {
                            if(!_openQuoteRead)
                            {
                                throw new ParseException(_sourceIndex,
                                    $"Invalid character at position {_sourceIndex}: closing '\"' found with no corresponding opening character.");
                            }

                            _builder.Append(']');
                            _openBracketWritten = false;
                            _openQuoteRead = false;
                        }
                        else
                        {
                            _builder.Append('[');
                            _openBracketWritten = true;
                            _openQuoteRead = true;
                        }

                        _lastCharWasSeparator = false;
                        break;

                    case ']':
                        if(!_openBracketRead)
                        {
                            throw new ParseException(_sourceIndex,
                                $"Invalid character at position {_sourceIndex}: closing ']' found with no corresponding '['.");
                        }

                        _builder.Append(']');
                        _openBracketWritten = false;
                        _openBracketRead = false;
                        _lastCharWasSeparator = false;
                        break;
                }

                _stack = null;
            }
        }
    }
}