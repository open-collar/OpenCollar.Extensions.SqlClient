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

using System.Collections;
using System.Collections.Generic;

namespace OpenCollar.Extensions.SqlClient
{
    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    public class QueryResults<T1> : IEnumerable<object>
    {
        /// <summary>
        ///     Gets the first set of results.
        /// </summary>
        /// <value>
        ///     The first set of results.
        /// </value>
        public T1 Results1 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1 })).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)((IEnumerable)this).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    public class QueryResults<T1, T2> : QueryResults<T1>
    {
        /// <summary>
        ///     Gets the second set of results.
        /// </summary>
        /// <value>
        ///     The second set of results.
        /// </value>
        public T2 Results2 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3> : QueryResults<T1, T2>
    {
        /// <summary>
        ///     Gets the third set of results.
        /// </summary>
        /// <value>
        ///     The third set of results.
        /// </value>
        public T3 Results3 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4> : QueryResults<T1, T2, T3>
    {
        /// <summary>
        ///     Gets the fourth set of results.
        /// </summary>
        /// <value>
        ///     The fourth set of results.
        /// </value>
        public T4 Results4 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5> : QueryResults<T1, T2, T3, T4>
    {
        /// <summary>
        ///     Gets the fifth set of results.
        /// </summary>
        /// <value>
        ///     The fifth set of results.
        /// </value>
        public T5 Results5 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    /// <typeparam name="T6">
    ///     The type of the sixth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5, T6> : QueryResults<T1, T2, T3, T4, T5>
    {
        /// <summary>
        ///     Gets the sixth set of results.
        /// </summary>
        /// <value>
        ///     The sixth set of results.
        /// </value>
        public T6 Results6 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5, Results6 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    /// <typeparam name="T6">
    ///     The type of the sixth result set.
    /// </typeparam>
    /// <typeparam name="T7">
    ///     The type of the seventh result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5, T6, T7> : QueryResults<T1, T2, T3, T4, T5, T6>
    {
        /// <summary>
        ///     Gets the seventh set of results.
        /// </summary>
        /// <value>
        ///     The seventh set of results.
        /// </value>
        public T7 Results7 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5, Results6, Results7 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    /// <typeparam name="T6">
    ///     The type of the sixth result set.
    /// </typeparam>
    /// <typeparam name="T7">
    ///     The type of the seventh result set.
    /// </typeparam>
    /// <typeparam name="T8">
    ///     The type of the eighth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5, T6, T7, T8> : QueryResults<T1, T2, T3, T4, T5, T6, T7>
    {
        /// <summary>
        ///     Gets the eighth set of results.
        /// </summary>
        /// <value>
        ///     The eighth set of results.
        /// </value>
        public T8 Results8 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5, Results6, Results7, Results8 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    /// <typeparam name="T6">
    ///     The type of the sixth result set.
    /// </typeparam>
    /// <typeparam name="T7">
    ///     The type of the seventh result set.
    /// </typeparam>
    /// <typeparam name="T8">
    ///     The type of the eighth result set.
    /// </typeparam>
    /// <typeparam name="T9">
    ///     The type of the ninth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9> : QueryResults<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        /// <summary>
        ///     Gets the ninth set of results.
        /// </summary>
        /// <value>
        ///     The ninth set of results.
        /// </value>
        public T9 Results9 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5, Results6, Results7, Results8, Results9 })).GetEnumerator();
        }
    }

    /// <summary>
    ///     A simple, strongly-typed, container for the results of a query execution.
    /// </summary>
    /// <typeparam name="T1">
    ///     The type of the first result set.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The type of the second result set.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The type of the third result set.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The type of the fourth result set.
    /// </typeparam>
    /// <typeparam name="T5">
    ///     The type of the fifth result set.
    /// </typeparam>
    /// <typeparam name="T6">
    ///     The type of the sixth result set.
    /// </typeparam>
    /// <typeparam name="T7">
    ///     The type of the seventh result set.
    /// </typeparam>
    /// <typeparam name="T8">
    ///     The type of the eighth result set.
    /// </typeparam>
    /// <typeparam name="T9">
    ///     The type of the ninth result set.
    /// </typeparam>
    /// <typeparam name="T10">
    ///     The type of the ninth result set.
    /// </typeparam>
    public class QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : QueryResults<T1, T2, T3, T4, T5, T6, T7, T8, T9>
    {
        /// <summary>
        ///     Gets the tenth set of results.
        /// </summary>
        /// <value>
        ///     The tenth set of results.
        /// </value>
        public T10 Results10 { get; internal set; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)(new object[] { Results1, Results2, Results3, Results4, Results5, Results6, Results7, Results8, Results9, Results10 })).GetEnumerator();
        }
    }
}