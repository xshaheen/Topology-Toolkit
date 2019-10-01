﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topology
{
    public class Topology
    {
        private static void Main()
        {
            var set = new HashSet<char>{'a', 'b', 'c'};
            Console.WriteLine("Topologies on: " + SetToString(set));
            Console.WriteLine("-----------------------------------");
            var topologies = Topologies(set);
            ///////////////////
            Console.WriteLine("new HashSet<HashSet<HashSet<char>>>\n{");
            foreach (var t in topologies)
            {
                Console.WriteLine("new HashSet<HashSet<char>>\n{");
                foreach (var e in t)
                {
                    Console.Write("new HashSet<char>{");
                    foreach (var c in e)
                    {
                        Console.Write($"'{c}',");
                    }

                    if (e.Count != 0) Console.Write("\b");
                    Console.WriteLine("},");
                }
                if (t.Count != 0) Console.Write("\b");
                Console.WriteLine("},");
            }

            Console.WriteLine("};");
            //////////////////
            Console.ReadLine();
        }

        /// <summary>
        /// Generates a all topology defined on a given set.
        /// </summary>
        /// Example:
        /// - Input: S = {'c', 'b', 'a'} // See unit test.
        ///   Output Pattern: any pattern also have 000 and 111
        ///   Single And Double
        ///   001
        ///       010
        ///           011
        ///               100
        ///                   101
        ///                       110
        /// 
        ///   Single-double (disjoint)
        ///   001                 110
        ///       010         101
        ///           011 100
        /// 
        ///   Single-double
        ///   001     011
        ///   001             101
        ///       010 011
        ///       010             110
        ///               100 101
        ///               100     110
        /// 
        ///   Single-single-double
        ///   001 010 011
        ///   001         100 101
        ///       010     100     110
        ///
        ///   Single-double-double
        ///   001     011     101
        ///       010 011         110
        ///               100 101 110
        ///
        ///   single-single-double-double 
        ///   001 010 011     101
        ///   001 010 011         110
        ///   001     011 100 101
        ///   001         100 101 110
        ///       010 011 100     110
        ///       010     100 101 110
        ///   
        ///
        ///   Power set
        ///   001 010 011 100 101 110
        /// - From this pattern the elements of the power set is exist or not
        ///   so by using a brute force tests we can get all topologies.
        /// - Fact:
        ///   Let T(n) denote the number of distinct topologies on a set with n points.
        ///   There is no known simple formula to compute T(n) for arbitrary n.
        /// - Theorem:
        ///   The number of subsets of size r (or r-combinations) that can be chosen
        ///   from a set of n elements, is given by the formula: nCr = !n / r!(n - r)!
        /// - Theorem:
        ///   The number of subsets of all size is 2^n
        /// <typeparam name="T">Set elements type.</typeparam>
        /// <param name="set">The set that a topologies dif</param>
        /// <returns>Set of all topologies that defined on <paramref name="set"/>.</returns>
        public static HashSet<HashSet<HashSet<T>>> Topologies<T>(HashSet<T> set)
        {
            var comparer = Comparer.GetIEqualityComparer(
                (HashSet<char> x, HashSet<char> y) => y.SetEquals(x));

            var setComparer = Comparer.GetIEqualityComparer(
                (HashSet<HashSet<T>> x, HashSet<HashSet<T>> y) => y.SetEquals(x));

            var powerSet = PowerSet(set);

            // add the trivial topology and the power set
            var topologies = new HashSet<HashSet<HashSet<T>>>(setComparer);
            // powerSet, 
            // new HashSet<HashSet<T>> {new HashSet<T>(), set}


            var generatedSet = PowerSet(powerSet);

            foreach (var t in generatedSet)
            {
                // t.Add(new HashSet<T>());
                // t.Add(set);
                if (IsTopology(t, set)) topologies.Add(t);
            }

            return topologies;
        }


        /// <summary>
        /// Determine if the <paramref name="t"/> is a topology of the <paramref name="set"/>.
        /// </summary>
        /// Topology Definition:
        /// let X be a set and let τ be a family of subsets of X. Then τ is called a topology on X if:
        /// * Both the empty set and X are elements of τ.
        /// * Any union of elements of τ is an element of τ.
        /// * Any intersection of finitely many elements of τ is an element of τ.
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        /// <param name="t">The candidate topology.</param>
        /// <param name="set">The set that candidate topology <paramref name="t"/> defined on.</param>
        /// <returns>Returns true if the <paramref name="t"/> if topology on the <paramref name="set"/>, otherwise return false.</returns>
        public static bool IsTopology<T>(HashSet<HashSet<T>> t, HashSet<T> set)
        {
            var setComparer = Comparer.GetIEqualityComparer((IEnumerable<T> x, IEnumerable<T> y)
                => ((HashSet<T>) x).SetEquals(y));

            if (!t.Contains(set, setComparer) ||
                !t.Contains(new HashSet<T>(), setComparer)) return false;

            // Todo: find a better data structure that support indexer can improve performance a little bit.
            return !(
                from e1 in t 
                from e2 in t
                let union = e1.Union(e2)
                let intersection = e1.Intersect(e2)
                where !t.Contains(union, setComparer) 
                      || !t.Contains(intersection, setComparer)
                select union).Any();
        }


        /// <summary>
        /// Generates the power set of the given set in O(2^set.length).
        /// </summary>
        /// Examples:
        /// - Input: S = {a, b}, Output: {}, {a}, {b}, S
        ///   Note the pattern:          00   01   10  11
        /// - Input: S = {a, b, c}, Output: {}, {a}, {b}, {a, b} {c}, {c, a}, {b, c}, S
        ///   Note the pattern:             000 001  010  011    100  101      110    111 
        /// - Input: S = {a, b, c, d}, Output: {}, {a} , {b}, {c}, {d}, {a,b}, {a,c}, {a,d},
        ///   {b,c}, {b,d}, {c,d}, {a,b,c}, {a,b,d}, {a,c,d}, {b,c,d}, S
        /// <typeparam name="T">The type of set elements.</typeparam>
        /// <param name="set">The target set.</param>
        /// <returns>The power set of the set.</returns>
        public static HashSet<HashSet<T>> PowerSet<T>(HashSet<T> set)
        {
            var elementComparer = Comparer.GetIEqualityComparer(
                (T x, T y) => y.Equals(x));

            var comparer = Comparer.GetIEqualityComparer(
                (HashSet<T> x, HashSet<T> y) => y.SetEquals(x));

            var powerSet = new HashSet<HashSet<T>>(comparer);

            // Fact: The number of subsets of a set contains n elements is 2^n.
            // loop to get all 2^n subset
            for (var i = 0; i < (1 << set.Count); i++)
            {
                var subset = new HashSet<T>(elementComparer);

                // loop though every element in the set and determine with number 
                // should present in the current subset.

                var j = 0;
                foreach (var e in set)
                    // if the jth element (bit) in the ith subset (binary number of i) add it.
                    if (((1 << j++) & i) > 0) subset.Add(e);

                powerSet.Add(subset);
            }

            return powerSet;
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<HashSet<HashSet<T>>> set)
        {
            var sb = new StringBuilder();
            var counter = 1;

            foreach (var e in set) sb.Append($"{counter++,4}. " + SetToString(e) + "\n");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<HashSet<T>> set)
        {
            var sb = new StringBuilder("{ ");

            foreach (var e in set) sb.Append(SetToString(e) + ", ");

            // remove the extra ", "
            var len = sb.Length;
            if (len > 1) sb.Remove(len - 2, 2);
            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<T> set)
        {
            var sb = new StringBuilder("{");

            foreach (var e in set) sb.Append(e + ", ");

            var len = sb.Length;
            // remove the extra ", "
            if (len > 1) sb.Remove(len - 2, 2);

            sb.Append("}");

            return sb.ToString();
        }
    }
}