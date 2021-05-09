using System.Collections;
using System.Collections.Generic;

using Xunit;

namespace OpenCollar.Extensions.SqlClient.TESTS
{
    public sealed class QueryBuilderTests
    {
        [Fact]
        public void TestGetDefaultValue_Array()
        {
            Assert.IsAssignableFrom<IEnumerable<string>>(QueryBuilder.GetDefaultValue(typeof(string[])));
            Assert.IsAssignableFrom<IEnumerable<int>>(QueryBuilder.GetDefaultValue(typeof(int[])));
        }

        [Fact]
        public void TestGetDefaultValue_Enumerable()
        {
            Assert.IsAssignableFrom<IEnumerable<string>>(QueryBuilder.GetDefaultValue(typeof(IEnumerable<string>)));
            Assert.IsAssignableFrom<IEnumerable>(QueryBuilder.GetDefaultValue(typeof(IEnumerable)));
        }

        [Fact]
        public void TestGetDefaultValue_List()
        {
            Assert.IsAssignableFrom<IList<string>>(QueryBuilder.GetDefaultValue(typeof(IList<string>)));
            Assert.IsAssignableFrom<IList>(QueryBuilder.GetDefaultValue(typeof(IList)));
        }

        [Fact]
        public void TestGetDefaultValue_ReferenceType()
        {
            Assert.Null(QueryBuilder.GetDefaultValue(typeof(object)));
            Assert.Null(QueryBuilder.GetDefaultValue(typeof(string)));
        }

        [Fact]
        public void TestGetDefaultValue_ValueType()
        {
            Assert.Equal(0, QueryBuilder.GetDefaultValue(typeof(int)));
            Assert.Equal((char)0, QueryBuilder.GetDefaultValue(typeof(char)));
            Assert.Equal(false, QueryBuilder.GetDefaultValue(typeof(bool)));
        }
    }
}