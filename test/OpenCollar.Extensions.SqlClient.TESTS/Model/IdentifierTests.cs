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

using OpenCollar.Extensions.SqlClient.Model;

using Xunit;

namespace OpenCollar.Extensions.SqlClient.TESTS.Model
{
    /// <summary>
    ///     Unit tests for the <see cref="Identifier" /> class.
    /// </summary>
    public class IdentifierTests
    {
        /// <summary>
        ///     Tests that the implicit conversion from <see cref="string" /> to <see cref="Identifier" /> function
        ///     performs as expected.
        /// </summary>
        [Fact]
        public void TestImplicitConversionFromString()
        {
            const string a = "[test]";
            const string b = "SCHEMA1.[test]";

            Identifier x = a;

            Assert.IsType<Identifier>(x);
            Assert.Equal(a, x.OriginalValue);
            Assert.Equal(a, x.ToString());

            Identifier y = b;

            Assert.IsType<Identifier>(y);
            Assert.Equal(b, y.OriginalValue);
            Assert.NotEqual(b, y.ToString());

            Identifier z = (string)null;
            Assert.Null(z);
        }

        /// <summary>
        ///     Tests that the implicit conversion from <see cref="Identifier" /> to <see cref="string" /> function
        ///     performs as expected.
        /// </summary>
        [Fact]
        public void TestImplicitConversionFromIdentifier()
        {
            const string a = "[test]";
            const string b = "SCHEMA1.[test]";

            string x = (new Identifier(a));

            Assert.IsType<string>(x);
            Assert.Equal((new Identifier(a)).OriginalValue, x);
            Assert.Equal((new Identifier(a)).ToString(), x);

            string y = (new Identifier(b));

            Assert.IsType<string>(y);
            Assert.NotEqual((new Identifier(b)).OriginalValue, y);
            Assert.Equal((new Identifier(b)).ToString(), y);

            string z = (Identifier)null;
            Assert.Null(z);
        }

        /// <summary>
        ///     Tests that the <see cref="Identifier.OriginalValue" /> property performs as expected.
        /// </summary>
        [Fact]
        public void TestOriginalValue()
        {
            const string a = "[test]";
            const string b = "SCHEMA1.[test]";

            Assert.Equal(a, (new Identifier(a)).OriginalValue);
            Assert.Equal(a, (new Identifier(a)).ToString());

            Assert.Equal(b, (new Identifier(b)).OriginalValue);
            Assert.NotEqual(b, (new Identifier(b)).ToString());
        }

        /// <summary>
        ///     Tests that the <see cref="Identifier.OriginalValue" /> property performs as expected.
        /// </summary>
        [Fact]
        public void TestCompareTo()
        {
            const string low = "AAAA";
            const string medium = "BBBB";
            const string high = "CCCC";

            var x = new Identifier(medium);
            Assert.Throws<ArgumentException>(() => ((IComparable)x).CompareTo((object)123));
            Assert.True(x.CompareTo(x) == 0);
            Assert.True(x.CompareTo((Identifier)null) > 0);

            Assert.True((new Identifier(medium)).CompareTo(new Identifier(medium)) == 0);
            Assert.True((new Identifier(low)).CompareTo(new Identifier(medium)) < 0);
            Assert.True((new Identifier(high)).CompareTo(new Identifier(medium)) > 0);

            Assert.True(x.CompareTo((object)x) == 0);
            Assert.True(x.CompareTo((object)null) > 0);

            Assert.True((new Identifier(medium)).CompareTo((object)(new Identifier(medium))) == 0);
            Assert.True((new Identifier(low)).CompareTo((object)(new Identifier(medium))) < 0);
            Assert.True((new Identifier(high)).CompareTo((object)(new Identifier(medium))) > 0);
        }

        /// <summary>
        ///     Tests that the <see cref="Identifier.OriginalValue" /> property performs as expected.
        /// </summary>
        [Fact]
        public void TestComparisonOperators()
        {
            const string low = "AAAA";
            const string medium = "BBBB";
            const string high = "CCCC";

            var x = new Identifier(medium);

            Assert.True(x == x);
            Assert.True(x != null);

            Assert.True((new Identifier(medium)) == (new Identifier(medium)));
            Assert.True((new Identifier(low)) < (new Identifier(medium)));
            Assert.True((new Identifier(low)) <= (new Identifier(medium)));
            Assert.True((new Identifier(high)) >= (new Identifier(medium)));
            Assert.True((new Identifier(high)) > (new Identifier(medium)));

            Assert.False(x != x);
            Assert.False(x == null);

            Assert.False((new Identifier(medium)) != (new Identifier(medium)));
            Assert.False((new Identifier(low)) >= (new Identifier(medium)));
            Assert.False((new Identifier(low)) > (new Identifier(medium)));
            Assert.False((new Identifier(high)) < (new Identifier(medium)));
            Assert.False((new Identifier(high)) <= (new Identifier(medium)));
        }

        /// <summary>
        ///     Tests that the <see cref="Identifier.Equals(object)" /> function performs as expected.
        /// </summary>
        [Fact]
        public void TestEquals()
        {
            var a = new Identifier("[test]");
            var b = new Identifier("[schema1].[test]");

            Assert.True(a.Equals(a));
            Assert.True(a.Equals(new Identifier("[test]")));
            Assert.True(a.Equals(new Identifier("[TEST]")));
            Assert.True(a.Equals(new Identifier("[TeSt]")));
            Assert.True(a.Equals(new Identifier("\"test\"")));
            Assert.True(a.Equals(new Identifier("\"TEST\"")));
            Assert.True(a.Equals(new Identifier("\"TeSt\"")));
            Assert.True(a.Equals(new Identifier("test")));
            Assert.True(a.Equals(new Identifier("TEST")));
            Assert.True(a.Equals(new Identifier("TeSt")));

            Assert.False(b.Equals((Identifier)null));
            Assert.False(b.Equals(a));
            Assert.False(b.Equals(new Identifier("[test]")));
            Assert.False(b.Equals(new Identifier("[TEST]")));
            Assert.False(b.Equals(new Identifier("[TeSt]")));
            Assert.False(b.Equals(new Identifier("\"test\"")));
            Assert.False(b.Equals(new Identifier("\"TEST\"")));
            Assert.False(b.Equals(new Identifier("\"TeSt\"")));
            Assert.False(b.Equals(new Identifier("test")));
            Assert.False(b.Equals(new Identifier("TEST")));
            Assert.False(b.Equals(new Identifier("TeSt")));

            Assert.True(a.Equals((object)a));
            Assert.True(a.Equals((object)(new Identifier("[test]"))));
            Assert.True(a.Equals((object)(new Identifier("[TEST]"))));
            Assert.True(a.Equals((object)(new Identifier("[TeSt]"))));
            Assert.True(a.Equals((object)(new Identifier("\"test\""))));
            Assert.True(a.Equals((object)(new Identifier("\"TEST\""))));
            Assert.True(a.Equals((object)(new Identifier("\"TeSt\""))));
            Assert.True(a.Equals((object)(new Identifier("test"))));
            Assert.True(a.Equals((object)(new Identifier("TEST"))));
            Assert.True(a.Equals((object)(new Identifier("TeSt"))));

            Assert.False(a.Equals(123));

            Assert.False(b.Equals((object)null));
            Assert.False(b.Equals((object)a));
            Assert.False(b.Equals((object)(new Identifier("[test]"))));
            Assert.False(b.Equals((object)(new Identifier("[TEST]"))));
            Assert.False(b.Equals((object)(new Identifier("[TeSt]"))));
            Assert.False(b.Equals((object)(new Identifier("\"test\""))));
            Assert.False(b.Equals((object)(new Identifier("\"TEST\""))));
            Assert.False(b.Equals((object)(new Identifier("\"TeSt\""))));
            Assert.False(b.Equals((object)(new Identifier("test"))));
            Assert.False(b.Equals((object)(new Identifier("TEST"))));
            Assert.False(b.Equals((object)(new Identifier("TeSt"))));
        }

        /// <summary>
        ///     Tests that the <see cref="Identifier.GetHashCode" /> function performs as expected.
        /// </summary>
        [Fact]
        public void TestGetHashCode()
        {
            var a = (new Identifier("[test]")).GetHashCode();
            var b = (new Identifier("[schema1].[test]")).GetHashCode();

            Assert.True(a.Equals(a));
            Assert.True(a.Equals((new Identifier("[test]").GetHashCode())));
            Assert.True(a.Equals((new Identifier("[TEST]").GetHashCode())));
            Assert.True(a.Equals((new Identifier("[TeSt]").GetHashCode())));
            Assert.True(a.Equals((new Identifier("\"test\"").GetHashCode())));
            Assert.True(a.Equals((new Identifier("\"TEST\"").GetHashCode())));
            Assert.True(a.Equals((new Identifier("\"TeSt\"").GetHashCode())));
            Assert.True(a.Equals((new Identifier("test").GetHashCode())));
            Assert.True(a.Equals((new Identifier("TEST").GetHashCode())));
            Assert.True(a.Equals((new Identifier("TeSt").GetHashCode())));

            Assert.False(b.Equals(a));
            Assert.False(b.Equals((new Identifier("[test]").GetHashCode())));
            Assert.False(b.Equals((new Identifier("[TEST]").GetHashCode())));
            Assert.False(b.Equals((new Identifier("[TeSt]").GetHashCode())));
            Assert.False(b.Equals((new Identifier("\"test\"").GetHashCode())));
            Assert.False(b.Equals((new Identifier("\"TEST\"").GetHashCode())));
            Assert.False(b.Equals((new Identifier("\"TeSt\"").GetHashCode())));
            Assert.False(b.Equals((new Identifier("test").GetHashCode())));
            Assert.False(b.Equals((new Identifier("TEST").GetHashCode())));
            Assert.False(b.Equals((new Identifier("TeSt").GetHashCode())));
        }

        /// <summary>
        ///     Tests that equality operator performs as expected.
        /// </summary>
        [Fact]
        public void TestEquality()
        {
            var a = new Identifier("[test]");
            var b = new Identifier("[schema1].[test]");

#pragma warning disable CS1718 // Comparison made to same variable

            // ReSharper disable once EqualExpressionComparison
            Assert.True(a == a);
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.True(a == new Identifier("[test]"));
            Assert.True(a == new Identifier("[TEST]"));
            Assert.True(a == new Identifier("[TeSt]"));
            Assert.True(a == new Identifier("\"test\""));
            Assert.True(a == new Identifier("\"TEST\""));
            Assert.True(a == new Identifier("\"TeSt\""));
            Assert.True(a == new Identifier("test"));
            Assert.True(a == new Identifier("TEST"));
            Assert.True(a == new Identifier("TeSt"));

            Assert.False(a == null);
            Assert.False(b == a);
            Assert.False(b == new Identifier("[test]"));
            Assert.False(b == new Identifier("[TEST]"));
            Assert.False(b == new Identifier("[TeSt]"));
            Assert.False(b == new Identifier("\"test\""));
            Assert.False(b == new Identifier("\"TEST\""));
            Assert.False(b == new Identifier("\"TeSt\""));
            Assert.False(b == new Identifier("test"));
            Assert.False(b == new Identifier("TEST"));
            Assert.False(b == new Identifier("TeSt"));
        }

        /// <summary>
        ///     Tests that inequality operator performs as expected.
        /// </summary>
        [Fact]
        public void TestInequality()
        {
            var a = new Identifier("[test]");
            var b = new Identifier("[schema1].[test]");

#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable S1764 // Identical expressions should not be used on both sides of a binary operator

            // ReSharper disable once EqualExpressionComparison
            Assert.False(a != a);
#pragma warning restore S1764 // Identical expressions should not be used on both sides of a binary operator
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.False(a != new Identifier("[test]"));
            Assert.False(a != new Identifier("[TEST]"));
            Assert.False(a != new Identifier("[TeSt]"));
            Assert.False(a != new Identifier("\"test\""));
            Assert.False(a != new Identifier("\"TEST\""));
            Assert.False(a != new Identifier("\"TeSt\""));
            Assert.False(a != new Identifier("test"));
            Assert.False(a != new Identifier("TEST"));
            Assert.False(a != new Identifier("TeSt"));

            Assert.True(a != null);
            Assert.True(b != a);
            Assert.True(b != new Identifier("[test]"));
            Assert.True(b != new Identifier("[TEST]"));
            Assert.True(b != new Identifier("[TeSt]"));
            Assert.True(b != new Identifier("\"test\""));
            Assert.True(b != new Identifier("\"TEST\""));
            Assert.True(b != new Identifier("\"TeSt\""));
            Assert.True(b != new Identifier("test"));
            Assert.True(b != new Identifier("TEST"));
            Assert.True(b != new Identifier("TeSt"));
        }

        /// <summary>
        ///     Validates that the parsing of identifiers is correct.
        /// </summary>
        [Fact]
        public void TestParsing()
        {
            Assert.Equal("[entity]", new Identifier("entity"));
            Assert.Equal("[entity]", new Identifier("[entity]"));
            Assert.Equal("[entity]", new Identifier("\"entity\""));
            Assert.Equal("[special]]]", new Identifier("[special]]]"));
            Assert.Equal("[special]]]", new Identifier("\"special]]\""));
            Assert.Equal("[special\"\"]", new Identifier("\"special\"\"\""));
            Assert.Equal("[schema.entity]", new Identifier("\"schema.entity\""));
            Assert.Equal("[schema.entity]", new Identifier("[schema.entity]"));
            Assert.Equal("[schema].[entity]", new Identifier("schema.entity"));
            Assert.Equal("[schema].[entity]", new Identifier("\"schema\".entity"));
            Assert.Equal("[schema].[entity]", new Identifier("schema.\"entity\""));
            Assert.Equal("[schema].[entity]", new Identifier("[schema].entity"));
            Assert.Equal("[schema].[entity]", new Identifier("schema.[entity]"));
            Assert.Equal("[schema].[entity]", new Identifier("[schema].\"entity\""));
            Assert.Equal("[schema].[entity]", new Identifier("\"schema\".[entity]"));

            Assert.Throws<ArgumentNullException>(() =>
            {
                var i = new Identifier(null);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                var i = new Identifier(string.Empty);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                var i = new Identifier("\t\r\n   ");
            });

            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema..entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema[.entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema].entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema\".entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("[schema.entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("\"schema.entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema.entity\"");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema.entity]");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema.entity[");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("schema.entity.");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier(".schema.entity");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("sche[ma");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("sche]ma");
            });
            Assert.Throws<ParseException>(() =>
            {
                var i = new Identifier("sche\"ma");
            });
        }
    }
}