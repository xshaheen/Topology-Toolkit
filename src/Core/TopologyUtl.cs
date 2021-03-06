﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Infra;

namespace Core
{
    /// <summary>
    /// Provides a set of static methods that related to topology.
    /// </summary>
    public class TopologyUtl
    {
        /// <summary>
        /// Determine if the <paramref name="t"/> is a topology of the <paramref name="set"/> in O(t.Count^2).
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
            if (set == null) throw new ArgumentNullException(nameof(set));
            if (t == null) throw new ArgumentNullException(nameof(t));

            var comparer = Comparer.GetIEqualityComparer((IEnumerable<T> x, IEnumerable<T> y)
                => ((HashSet<T>) x).SetEquals(y));

            if (!t.Contains(set, comparer) || !t.Contains(new HashSet<T>(), comparer))
                return false;

            foreach (var e1 in t)
            foreach (var e2 in t)
            {
                var union = e1.Union(e2);
                var intersection = e1.Intersect(e2);
                if (!t.Contains(union, comparer) || !t.Contains(intersection, comparer))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generates all topologies defined on a given set (where set.Count &lt; 6) in O(2^(2^set.Count -2)).
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
        /// <param name="set">The set that a topologies define.</param>
        /// <returns>Set of all topologies that defined on <paramref name="set"/>.</returns>
        public static IEnumerable<HashSet<HashSet<T>>> Topologies<T>(HashSet<T> set)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            // if > 6 will case overflow in the long type.
            if (set.Count > 5) 
                throw new Exception("Set elements must be less than 6 elements.");

            var powerSet = SetUtl.PowerSet(set);

            // remove the set and the empty set. for example, for set of 4 element this
            // make the complexity decrease from 2^(2^4)= 65,536 to 2^(2^4-2)= 16,384
            powerSet.RemoveWhere(s => s.Count == 0);         // O(2^set.Count)
            powerSet.RemoveWhere(s => s.Count == set.Count); // O(2^set.Count)

            var n = 1L << powerSet.Count;
            // loop to get all n subsets
            for (long i = 0; i < n; i++)
            {
                var subset = new HashSet<HashSet<T>>();

                // loop though every element in the set and determine with number 
                // should present in the current subset.
                var j = 0;
                foreach (var e in powerSet)
                    // if the jth element (bit) in the ith subset (binary number of i) add it.
                    if (((1L << j++) & i) > 0)
                        subset.Add(e);

                subset.Add(new HashSet<T>());
                subset.Add(set);
                if (IsTopology(subset, set)) yield return subset;
            }
        }

        /// <summary>
        /// Generates all topologies defined on a given set (where set.Count &lt; 6) in O(2^(2^set.Count -2)).
        /// </summary>
        /// <returns>Set of all topologies that defined on <paramref name="set"/>.</returns>
        public static IEnumerable<HashSet<HashSet<T>>> Topologies<T>(
            HashSet<T> set, IProgress<double> progress, CancellationToken ct)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            // if > 6 will case overflow in the long type.
            if (set.Count > 5) throw 
                new Exception("Set elements must be less than 6 elements.");

            progress.Report(0);

            var powerSet = SetUtl.PowerSet(set);

            // remove the set and the empty set.
            powerSet.RemoveWhere(s => s.Count == 0);         // O(2^set.Count)
            powerSet.RemoveWhere(s => s.Count == set.Count); // O(2^set.Count)

            var n = 1L << powerSet.Count;
            // loop to get all n subsets
            for (long i = 0; i < n; i++)
            {
                ct.ThrowIfCancellationRequested();

                if (i % 100 == 0)
                {
                    var x = i / (decimal)n;
                    var p = 100 * x;
                    progress.Report((double)p);
                }

                var subset = new HashSet<HashSet<T>>();

                // loop though every element in the set and determine with number 
                // should present in the current subset.
                var j = 0;
                foreach (var e in powerSet)
                    // if the jth element (bit) in the ith subset (binary number of i) add it.
                    if (((1L << j++) & i) > 0)
                        subset.Add(e);

                subset.Add(new HashSet<T>());
                subset.Add(set);
                if (IsTopology(subset, set)) yield return subset;
            }
            progress.Report(0);
        }

        /// <summary>
        /// Find the neighborhood for point in the <paramref name="set"/> for a given topology.
        /// </summary>
        /// Definition: Neighborhood of a point:
        ///   If X is a topological space and p ∈ X, a neighborhood of p is a subset N of X
        ///   that includes an open set O containing p, p∈O⊆N.
        /// Definition: The collection of all neighborhoods of a point is called the
        ///   neighborhood system at the point.
        public static HashSet<HashSet<T>> NeighbourhoodSystem<T>(
            HashSet<T> set, HashSet<HashSet<T>> topology, T point)
        {
            if (!IsTopology(topology, set)) throw new Exception(
                    "The given topology is not a valid topology on the set.");

            if (!set.Contains(point)) 
                throw new Exception("The set do not contain the point!");
            
            var powerSet = SetUtl.PowerSet(set);

            // The open sets that contain the point
            var enumerable = topology.Where(s => s.Contains(point));
            var openSets = enumerable as HashSet<T>[] ?? enumerable.ToArray();

            // The smallest open set that contain the point
            // Assume that the first element
            var o = openSets.First();

            foreach (var openSet in openSets)
                o.IntersectWith(openSet);

            var neighbourhood = new HashSet<HashSet<T>>();

            foreach (var s in powerSet.Where(s => o.IsSubsetOf(s)))
                neighbourhood.Add(s);

            return neighbourhood;
        }
    }
}