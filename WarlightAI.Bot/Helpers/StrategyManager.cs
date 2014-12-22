// <copyright file="StrategyCalculator.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>19/12/2014</date>

using System.Collections.Generic;
using System.Linq;
using WarlightAI.Model;

namespace WarlightAI.Helpers
{
    public static class StrategyManager
    {
        /// <summary>
        /// Calculates if there are border territories with enemy armies in this super region
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <param name="allSuperRegions">All super regions.</param>
        /// <returns></returns>
        public static bool EnemyBorderTerritories(SuperRegion superRegion, SuperRegions allSuperRegions)
        {
            /*
             * Searches all border territories with neighbours occupied by the opponent
             * Then checks if these border territories are occupied by me OR have neighbours in the same super region that are occuped by me
             * */
            return superRegion.BorderTerritories
                .Where(bt => bt.Neighbours.Any(btn => btn.IsOccupiedBy(PlayerType.Opponent)))
                .Any(bt => (bt.IsOccupiedBy(PlayerType.Me)) || bt.Neighbours.Any(btn => btn.IsOccupiedBy(PlayerType.Me) && allSuperRegions.Get(btn) == superRegion));
        }

        /// <summary>
        /// Calculates if there are regions in this super region that are occupied by the opponent
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <returns></returns>
        public static bool EnemyRegions(SuperRegion superRegion)
        {
            /*
             * Searches all regions in this super region that are occupied by the opponent
             * Then checks if I have neighbours next to those enemy ergions
             * */
            return superRegion.ChildRegions
                .Where(region => region.IsOccupiedBy(PlayerType.Opponent))
                .Any(region => region.Neighbours.Any(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)));
        }

        /// <summary>
        /// Calculates if the super region should be skipped
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <returns></returns>
        public static bool SkipSuperRegion(SuperRegion superRegion)
        {
            /*
             * Searches for regions in this super region that are occupied by me
             * Or, where I have regions nearby
             * */
            return superRegion
                     .ChildRegions
                     .None(region => region.IsOccupiedBy(PlayerType.Me)) &&
                   superRegion
                     .ChildRegions
                     .None(region => region.Neighbours.Any(n => n.IsOccupiedBy(PlayerType.Me)));
        }

        /// <summary>
        /// Gets the source region based on the given source strategy and existing transfers.
        /// </summary>
        /// <param name="targetRegion">The target region.</param>
        /// <param name="transfers">The transfers.</param>
        /// <param name="sourceStrategy">The source strategy.</param>
        /// <returns></returns>
        public static Region GetSourceRegion(Region targetRegion, IEnumerable<ArmyTransfer> transfers, SourceStrategy sourceStrategy)
        {
            switch (sourceStrategy)
            {
                case SourceStrategy.DominateOtherSuperRegions:
                    return targetRegion
                        .Neighbours
                        .OccupiedBy(PlayerType.Me)
                        .WithMinimumThreshold()
                        .NoSourceYet(transfers)
                        .OrderRegions(OrderStrategy.NumberOfArmies)
                        .FirstOrDefault();

                case SourceStrategy.DominateCurrentSuperRegion:
                    return targetRegion
                        .Neighbours
                        .OnSameSuperRegion(targetRegion)
                        .OccupiedBy(PlayerType.Me)
                        .WithMinimumThreshold()
                        .NoSourceYet(transfers)
                        .OrderRegions(OrderStrategy.NumberOfArmies)
                        .FirstOrDefault();

                case SourceStrategy.AttackEnemyInvasionPath:
                    return targetRegion
                        .Neighbours
                        .OccupiedBy(PlayerType.Me)
                        .WithMinimumThreshold()
                        .OrderRegions(OrderStrategy.NumberOfArmies)
                        .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets the target regions based on the given target strategy.
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <param name="allSuperRegions">All super regions.</param>
        /// <param name="transfers">The transfers.</param>
        /// <param name="targetStrategy">The target strategy.</param>
        /// <returns></returns>
        public static IEnumerable<Region> GetTargetRegions(SuperRegion superRegion, IEnumerable<ArmyTransfer> transfers, TargetStrategy targetStrategy)
        {
            IEnumerable<Region> targetRegions = new List<Region>();

            switch (targetStrategy)
            {
                case TargetStrategy.ConquerCurrentSuperRegion:
                    /*
                     * Searches for regions in this super region that are not occupied (neutral)
                     * Order by:
                     *  - Take regions away from the borders first
                     *  - Regions with neutral armies first, to ensure our expansion drift
                     *  - The amount of our regions nearby
                     * */
                    targetRegions = superRegion
                        .ChildRegions
                        .OccupiedBy(PlayerType.Neutral)
                        .OrderRegions(OrderStrategy.InternalFirst)
                        .ThenOrderRegions(OrderStrategy.NeutralNeighboursFirst)
                        .ThenOrderRegions(OrderStrategy.MostArmiesNearby);

                    break;

                case TargetStrategy.ConquerAll:
                    /*
                     * Searches for regions in all super regions that are not occupied (neutral)
                     * Order by:
                     *  - Take regions away from the borders first
                     *  - Regions with neutral armies first, to ensure our expansion drift
                     *  - The amount of our regions nearby
                     * */
                    var internalRegions = superRegion
                        .ChildRegions
                        .OccupiedBy(PlayerType.Neutral);

                    var externalRegions = superRegion
                        .ChildRegions
                        .OccupiedBy(PlayerType.Me)
                        .SelectMany(region => region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Neutral) && neighbour.SuperRegion != superRegion));

                    targetRegions = internalRegions.Concat(externalRegions)
                        .OrderRegions(OrderStrategy.InternalFirst)
                        .ThenOrderRegions(OrderStrategy.NeutralNeighboursFirst)
                        .ThenOrderRegions(OrderStrategy.MostArmiesNearby);

                    break;

                case TargetStrategy.ConquerOtherSuperRegions:
                    /*
                     * Searches for regions in other super regions that are not occupied (neutral)
                     * */
                    targetRegions = superRegion
                        .InvasionPaths
                        .OccupiedBy(PlayerType.Neutral)
                        .OrderRegions(OrderStrategy.MostArmiesNearby);

                    break;

                case TargetStrategy.EnemyInvasionPaths:
                    /*
                     * Searches for enemy invasion paths
                     * Skip regions that are a target already, we should have conquered them already in earlier moves
                     * */
                    targetRegions = superRegion
                        .InvasionPaths
                        .OccupiedBy(PlayerType.Opponent)
                        .NoTargetYet(transfers)
                        .OrderRegions(OrderStrategy.NumberOfArmies);

                    break;
            }

            return targetRegions;
        }

        /// <summary>
        /// Gets the primary region.
        /// </summary>
        /// <param name="regions">The regions.</param>
        /// <returns></returns>
        public static Region GetPrimaryRegion(Regions regions)
        {
            return regions
                .Find(PlayerType.Me)
                .NotEnclosedBy(PlayerType.Me)
                .OrderRegions(OrderStrategy.EnemyNeighboursFirst)
                .ThenOrderRegions(OrderStrategy.VeryLargeRegionsLast)
                .ThenOrderRegions(OrderStrategy.NeutralNeighboursFirst)
                .ThenOrderRegions(OrderStrategy.NeutralNeighboursOnOtherSuperRegionsFirst)
                .ThenOrderRegions(OrderStrategy.SmallSuperRegionsFirst)
                .ThenOrderRegions(OrderStrategy.NumberOfArmies)
                .FirstOrDefault();
        }

        public static Region GetSecundaryRegion(Regions regions, Region primaryRegion)
        {
            return regions
                .Find(PlayerType.Me)
                .NotEnclosedBy(PlayerType.Me)
                .OnOtherSuperRegion(primaryRegion)
                .Where(region => region.NbrOfArmies < 100)
                .OrderRegions(OrderStrategy.EnemyNeighboursFirst)
                .ThenOrderRegions(OrderStrategy.NeutralNeighboursFirst)
                .ThenOrderRegions(OrderStrategy.NeutralNeighboursOnOtherSuperRegionsFirst)
                .ThenOrderRegions(OrderStrategy.SmallSuperRegionsFirst)
                .ThenOrderRegions(OrderStrategy.NumberOfArmies)
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Gets the stuck armies.
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <param name="transfers">The transfers.</param>
        /// <returns></returns>
        public static List<Region> GetStuckArmies(SuperRegion superRegion, IEnumerable<ArmyTransfer> transfers)
        {
            return superRegion
                .ChildRegions
                .Find(PlayerType.Me)
                .NoSourceYet(transfers)
                .NoTargetYet(transfers)
                .CanBeUsedForTransfer()
                .AllNeighboursOccupiedBy(PlayerType.Me)
                .ToList();
        }

        private static IOrderedEnumerable<Region> OrderRegions(this IEnumerable<Region> regions, OrderStrategy orderStrategy)
        {
            switch (orderStrategy)
            {
                default:
                case OrderStrategy.NumberOfArmies:
                    return regions.OrderByDescending(region => region.NbrOfArmies);
                case OrderStrategy.MostQualifiedArmiesNearby:
                    return regions.OrderByDescending(region =>
                            region.Neighbours
                                .Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me))
                                .Where(neighbor => neighbor.NbrOfArmies > 5)
                                .Where(neighbor => neighbor.NbrOfArmies > region.NbrOfArmies * 2)
                                .Select(reg => reg.NbrOfArmies).Sum()
                        );
                case OrderStrategy.InternalFirst:
                    return regions.OrderBy(region => region.Neighbours.Any(neighbor => neighbor.SuperRegion != region.SuperRegion) ? 1 : 0);
                case OrderStrategy.NeutralNeighboursFirst:
                    return regions.OrderByDescending(region => region.Neighbours.Any(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && neighbor.SuperRegion == region.SuperRegion) ? 1 : 0);
                case OrderStrategy.EnemyNeighboursFirst:
                    return regions.OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)));
                case OrderStrategy.MostArmiesNearby:
                    return regions.OrderByDescending(region => region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum());
            }
        }

        private static IOrderedEnumerable<Region> ThenOrderRegions(this IOrderedEnumerable<Region> regions, OrderStrategy orderStrategy)
        {
            switch (orderStrategy)
            {
                default:
                    return regions;
                case OrderStrategy.NumberOfArmies:
                    return regions.ThenByDescending(region => region.NbrOfArmies);
                case OrderStrategy.VeryLargeRegionsLast:
                    return regions.ThenByDescending(region => region.NbrOfArmies < 100 ? 1 : 0);
                case OrderStrategy.SmallSuperRegionsFirst:
                    return regions.ThenBy(region => (region.SuperRegion.ChildRegions.OccupiedBy(PlayerType.Me).Count()));
                case OrderStrategy.NeutralNeighboursFirst:
                    return regions.ThenByDescending(region => region.Neighbours.Any(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && neighbor.SuperRegion == region.SuperRegion) ? 1 : 0);
                case OrderStrategy.NeutralNeighboursOnOtherSuperRegionsFirst:
                    return regions.ThenByDescending(region => region.Neighbours.Any(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && neighbor.SuperRegion != region.SuperRegion) ? 1 : 0);
                case OrderStrategy.EnemyNeighboursFirst:
                    return regions.ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)));
                case OrderStrategy.MostArmiesNearby:
                    return regions.ThenByDescending(region => region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum());
            }
        }
    }
}
