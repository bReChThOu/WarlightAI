// <copyright file="ExtensionMethods.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>19/12/2014</date>
using System;
using System.Collections.Generic;
using System.Linq;
using WarlightAI.Model;

namespace WarlightAI.Helpers
{
    /// <summary>
    /// Class that contains extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Determines whether a sequence contains no elements
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        /// <summary>
        /// Determines whether a sequence contains no elements
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }

        /// <summary>
        /// Returns all elements from a sequence that are occuped by
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="playerType">Type of the player.</param>
        /// <returns></returns>
        public static IEnumerable<Region> OccupiedBy(this IEnumerable<Region> source, PlayerType playerType)
        {
            return source.Where(region => region.IsOccupiedBy(playerType));
        }

        /// <summary>
        /// Returns all elements from a sequence where all the neighbours are occupied by a specified player.
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns></returns>
        public static IEnumerable<Region> AllNeighboursOccupiedBy(this IEnumerable<Region> source, PlayerType playerType)
        {
            return source.Where(region => region.Neighbours.All(neighbour => neighbour.IsOccupiedBy(playerType)));
        }

        /// <summary>
        /// Returns all elements from a sequence that are not enclosed by a specified player.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="playerType">Type of the player.</param>
        /// <returns></returns>
        public static IEnumerable<Region> NotEnclosedBy(this IEnumerable<Region> source, PlayerType playerType)
        {
            return source.Where(region => !region.AllNeighboursAreOccupiedBy(playerType));
        }

        /// <summary>
        /// Returns all elements from a sequence that have the minimum threshold, i.e. 6 or more armies
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<Region> WithMinimumThreshold(this IEnumerable<Region> source)
        {
            return source.Where(region => region.NbrOfArmies > 5);
        }

        /// <summary>
        /// Returns all elements from a sequence that can be used for transfer, i.e. 2 or more armies.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<Region> CanBeUsedForTransfer(this IEnumerable<Region> source)
        {
            return source.Where(region => region.NbrOfArmies > 1);
        }

        /// <summary>
        /// Returns all elements from a sequence that have the same super region as a given region
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="otherRegion">The other region.</param>
        /// <returns></returns>
        public static IEnumerable<Region> OnSameSuperRegion(this IEnumerable<Region> source, Region otherRegion)
        {
            return source.Where(region => region.SuperRegion == otherRegion.SuperRegion);
        }

        /// <summary>
        /// Returns all elements from a sequence that are not used yet as a source region
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="transfers">The transfers.</param>
        /// <returns></returns>
        public static IEnumerable<Region> NoSourceYet(this IEnumerable<Region> source, IEnumerable<ArmyTransfer> transfers)
        {
            return source.Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0);
        }

        /// <summary>
        /// Returns all elements from a sequence that are not used yet as a target region
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="transfers">The transfers.</param>
        /// <returns></returns>
        public static IEnumerable<Region> NoTargetYet(this IEnumerable<Region> source, IEnumerable<ArmyTransfer> transfers)
        {
            return source.Where(region => transfers.Count(t => t.TargetRegion.ID == region.ID) == 0);
        }
    }
}
