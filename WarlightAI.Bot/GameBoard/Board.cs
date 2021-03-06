﻿// <copyright file="Board.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WarlightAI.Helpers;
using WarlightAI.Model;

namespace WarlightAI.GameBoard
{
    /// <summary>
    /// Map Factory class
    /// </summary>
    public class Board
    {
        private static Board _instance;

        /// <summary>
        /// The instance
        /// </summary>
        public static Board Current
        {
            [DebuggerStepThrough]
            get
            {
                if (_instance == null)
                {
                    _instance = new Board();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the super regions.
        /// </summary>
        /// <value>
        /// The super regions.
        /// </value>
        private SuperRegions SuperRegions { get; set; }

        /// <summary>
        /// Gets or sets the regions.
        /// </summary>
        /// <value>
        /// The regions.
        /// </value>
        private Regions Regions { get; set; }

        /// <summary>
        /// Gets or sets the transfers.
        /// </summary>
        /// <value>
        /// The transfers.
        /// </value>
        private List<ArmyTransfer> Transfers { get; set; } 

        /// <summary>
        /// Prevents a default instance of the <see cref="Board"/> class from being created.
        /// </summary>
        private Board()
        {
            SuperRegions = new SuperRegions();
            Regions = new Regions();
        }

        /// <summary>
        /// Adds the super region.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="reward">The reward.</param>
        public void AddSuperRegion(int id, int reward)
        {
            SuperRegions.Add(new SuperRegion { ID = id, Reward = reward });
        }

        /// <summary>
        /// Adds the region.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="superRegionId">The super region identifier.</param>
        public void AddRegion(int id, int superRegionId)
        {
            var region = new Region { ID = id };
            Regions.Add(region);

            var superRegion = SuperRegions.Get(superRegionId);

            superRegion.AddChildRegion(region);
            region.SuperRegion = superRegion;
        }

        /// <summary>
        /// Defines a region as wasteland
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        public void SetWasteland(int regionId)
        {
            Regions
                .Get(regionId)
                .IsWasteland = true;
        }

        /// <summary>
        /// Sets the region neighbors.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="neighbors">The neighbors.</param>
        public void SetRegionNeighbors(int id, String[] neighbors)
        {
            var currentRegion = Regions.Get(id);
           
            var neighbourRegions = neighbors
                    .Select(neighbour => Regions.Get(neighbour))
                    .ToList();

            currentRegion
                .Neighbours
                .AddRange(neighbourRegions);

            /* 
             * Neighbors are only given in one direction.
             * E.g.: A has neighbour B, but the game won't tell us B has neighbour A.
             * 
             * We have to define that relation explicitly
             * */
            neighbourRegions
                .ForEach(
                    neighbourRegion => neighbourRegion.Neighbours.Add(currentRegion));
        }

        /// <summary>
        /// Calculates the super regions borders.
        /// </summary>
        public void CalculateSuperRegionsBorders()
        {
            SuperRegions.ForEach(CalculateSuperRegionBorders);
        }
        /// <summary>
        /// Calculates the super region borders.
        /// </summary>
        /// <param name="superregion">The superregion.</param>
        private void CalculateSuperRegionBorders(SuperRegion superregion)
        {
            //Calculate invasion paths and border territories for each Super Region
            var invasionPaths = superregion
                .ChildRegions
                .SelectMany(region => region
                                    .Neighbours
                                    .Where(neighbor => neighbor.SuperRegion.ID != superregion.ID));


            var borderTerritories = superregion
               .ChildRegions
                .Where(region => region
                                    .Neighbours
                                    .Any(neighbor => neighbor.SuperRegion.ID != superregion.ID));

            superregion.InvasionPaths = invasionPaths;
            superregion.BorderTerritories = borderTerritories;
        }

        /// <summary>
        /// Marks the starting regions.
        /// </summary>
        /// <param name="regions">The regions.</param>
        public void MarkStartingRegions(String[] regions)
        {
            // Reset all marked starting regions
            Regions
                .ForEach(
                    region =>
                    {
                        if (region.RegionStatus == RegionStatus.PossibleStartingRegion)
                        {
                            region.RegionStatus = RegionStatus.Initialized;
                        }
                    });

            regions
                .ToList()
                .ForEach(
                    regionId =>
                    {
                        Regions
                            .Get(regionId)
                            .RegionStatus = RegionStatus.PossibleStartingRegion;
                    }
                );
        }

        /// <summary>
        /// Gets the region.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Region GetRegion(int id)
        {
            return Regions.Get(id);
        }

        /// <summary>
        /// Picks the favorite starting region.
        /// </summary>
        /// <returns></returns>
        public Region PickFavoriteStartingRegion()
        {
            /*
             * One key to victory is control over continents.
             * Players that hold continents at the beginning of a turn get bonus reinforcements 
             * in an amount roughly proportional to the size of the continent 
             * 
             * Thus, the key positions on the board are the territories on the borders of continents. 
             * 
             * 
             * Fase 1: Try to find the continents with the least regions
             * 
             * */
            var startingRegion = Regions
                .Where(region => region.RegionStatus == RegionStatus.PossibleStartingRegion)
                .OrderByDescending(region => region.SuperRegion.Priority)
                .First();

            startingRegion.RegionStatus = RegionStatus.StartingRegion;

            return startingRegion;
        }

        /// <summary>
        /// Clears the regions.
        /// </summary>
        public void ClearRegions()
        {
            Regions
                .ForEach(
                    region =>
                    {
                        region.Player = new Player { PlayerType = PlayerType.Unknown };
                        region.NbrOfArmies = 0;
                    }
                );
        }

        /// <summary>
        /// Updates the region.
        /// </summary>
        /// <param name="regionId">The region id.</param>
        /// <param name="playername">The player name.</param>
        /// <param name="nbrOfArmies">The number of armies.</param>
        public void UpdateRegion(int regionId, String playername, int nbrOfArmies)
        {
            Regions
                .Get(regionId)
                .Update(
                    player: Configuration.Current.GetPlayerByName(playername),
                    nbrOfArmies: nbrOfArmies);
        }

        /// <summary>
        /// Updates the regions.
        /// </summary>
        /// <param name="placements">The placements.</param>
        public void UpdateRegions(IEnumerable<ArmyPlacement> placements)
        {
            placements
                .ToList()
                .ForEach(
                    placement => UpdateRegion(
                        regionId: placement.Region.ID, 
                        playername: Configuration.Current.GetMyBotName(), 
                        nbrOfArmies: placement.Region.NbrOfArmies + placement.Armies)
            );
        }

        /// <summary>
        /// Places the armies.
        /// </summary>
        public IEnumerable<ArmyPlacement> PlaceArmies()
        {
            var placements = new List<ArmyPlacement>();
            int startingArmies = Configuration.Current.GetStartingArmies();

            /* If we start on 3 super regions with an opponent as neighbour on every region we need to change some tactics:
             *  - Don't attack until round 4
             *  - Place armies on all 3 super regions to avoid loss of one or more super regions
             * */
            if (Configuration.Current.GetRoundNumber() == 1)
            {
                var myTotalSuperRegions =
                    Regions
                    .Find(PlayerType.Me)
                    .GroupBy(region => region.SuperRegion)
                    .ToList(); // Prevent possible multiple enumerations

                int opponentTotalSuperRegions =
                    Regions
                    .Find(PlayerType.Opponent)
                    .GroupBy(region => region.SuperRegion.ID)
                    .Count();

                if (myTotalSuperRegions.Count() == 3 && opponentTotalSuperRegions == 3)
                {
                    Configuration.Current.SetStartRoundNumber(4);
                    placements.Add(new ArmyPlacement
                    {
                        Armies = 2, 
                        Region = Regions.Find(PlayerType.Me).First(r => r.SuperRegion == myTotalSuperRegions.First().Key)
                    });

                    placements.Add(new ArmyPlacement
                    {
                        Armies = 2,
                        Region = Regions.Find(PlayerType.Me).First(r => r.SuperRegion == myTotalSuperRegions.Skip(1).First().Key)
                    });

                    placements.Add(new ArmyPlacement
                    {
                        Armies = 1,
                        Region = Regions.Find(PlayerType.Me).First(r => r.SuperRegion == myTotalSuperRegions.Skip(2).First().Key)
                    });

                    UpdateRegions(placements);
                    return placements;
                } 
                else
                {
                    var regions = Regions.Find(PlayerType.Me);
                    if (regions.Count() <= startingArmies)
                    {
                        var armiesPerRegion = startingArmies / regions.Count();

                        foreach (var region in regions)
                        {
                            if (region == regions.Last())
                            {
                                armiesPerRegion = startingArmies - (armiesPerRegion * regions.Count());
                            }

                            placements.Add(new ArmyPlacement
                            {
                                Armies = armiesPerRegion,
                                Region = region
                            });
                        }
                    }

                }

                Configuration.Current.SetStartRoundNumber(2);

            }

            var primaryRegion = StrategyManager.GetPrimaryRegion(Regions);
            var secundaryRegion = StrategyManager.GetSecundaryRegion(Regions, primaryRegion);

            int primaryRegionArmies = 0;
            int secundaryRegionArmies = 0;

            if (startingArmies <= 6)
            {
                primaryRegionArmies = 3;
                secundaryRegionArmies = startingArmies - 3;
            }
            if (startingArmies >= 7 && startingArmies <= 9)
            {
                primaryRegionArmies = 5;
                secundaryRegionArmies = startingArmies - 5;
            }
            if (startingArmies > 9 && startingArmies <= 18)
            {
                primaryRegionArmies = 9;
                secundaryRegionArmies = startingArmies - 9;
            }
            if (startingArmies > 18)
            {
                primaryRegionArmies = startingArmies - 9;
                secundaryRegionArmies = 9;
            }

            var armyplacement = new ArmyPlacement { Armies = primaryRegionArmies, Region = primaryRegion };
            placements.Add(armyplacement);

            if (secundaryRegion == null)
            {
                secundaryRegion = primaryRegion;
            }

            armyplacement = new ArmyPlacement { Armies = secundaryRegionArmies, Region = secundaryRegion };
            placements.Add(armyplacement);

            UpdateRegions(placements);
            return placements;
        }

        /// <summary>
        /// Transfers the armies.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ArmyTransfer> TransferArmies()
        {
            /*
             * Inspect Border Territories foreach super region
             * If there are no enemy armies sighted: let's conquer the continent
             * 
             * If there are enemy armies sighted: let's move some troops to defend those invasion paths
             * */
            Transfers = new List<ArmyTransfer>();

            SuperRegions.ForEach(CalculateTransfers);

            //Don't attack the enemy if we are below the starting round, instead skip this move
            if (Configuration.Current.GetRoundNumber() < Configuration.Current.GetStartRoundNumber())
            {
                Transfers.ForEach(
                    transfer =>
                    {
                        if (transfer.TargetRegion.IsOccupiedBy(PlayerType.Opponent))
                        {
                            Transfers.Remove(transfer);
                        }
                    }
                );
            }

            return Transfers;
        }

        /// <summary>
        /// Calculates the transfers.
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        public void CalculateTransfers(SuperRegion superRegion)
        {
            bool skipSuperRegion = StrategyManager.SkipSuperRegion(superRegion);

            if (skipSuperRegion)
            {
                return;
            }

            bool transferDone = false;

            bool borderTerritoriesWithEnemyArmies = StrategyManager.EnemyBorderTerritories(superRegion, SuperRegions);
            bool regionsWithEnemyArmies = StrategyManager.EnemyRegions(superRegion);

            if (!borderTerritoriesWithEnemyArmies && !regionsWithEnemyArmies)
            {
                transferDone = CalculateForNoEnemies(superRegion);
            }

            if (borderTerritoriesWithEnemyArmies && !transferDone)
            {
                transferDone = CalculateForEnemyBorderTerritories(superRegion);
            }

            if (regionsWithEnemyArmies && !transferDone)
            {
                transferDone = CalculateForEnemyRegions(superRegion);
            }

            var stuckArmies = StrategyManager.GetStuckArmies(superRegion, Transfers);
            CalculateForStuckArmies(stuckArmies);
        }

        /// <summary>
        /// Calculates the strategy when there are no enemy regions in this super region.
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <returns></returns>
        public bool CalculateForNoEnemies(SuperRegion superRegion)
        {
            Region targetRegion = null, sourceRegion = null;
            bool transferDone = false;

            var targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.ConquerAll);

            /* No neutral armies found in this super region, that should mean we own the continent.
             * Let's explore the world and go to a new super region
             * */
            if (targetRegions.None())
            {
                targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.ConquerOtherSuperRegions);

                if (targetRegions.Any())
                {
                    //We'll want to make more than 1 move
                    foreach (var cTargetRegion in targetRegions)
                    {
                        sourceRegion = StrategyManager.GetSourceRegion(cTargetRegion, Transfers, SourceStrategy.DominateOtherSuperRegions);
                        transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion);
                    }
                }
                else
                {
                    targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.EnemyInvasionPaths);
                    targetRegion = targetRegions.FirstOrDefault();

                    if (targetRegion != null)
                    {
                        sourceRegion = StrategyManager.GetSourceRegion(targetRegion, Transfers, SourceStrategy.AttackEnemyInvasionPath);
                        transferDone = AddCurrentPairToTransferList(sourceRegion, targetRegion);
                    }
                }
            }

            // Neutral regions found in this super region
            else
            {
                StrategyManager.ConquerNeutralRegions(targetRegions, Transfers, SourceStrategy.DominateOtherSuperRegions);
                
            }
            
            return transferDone;
        }

        /// <summary>
        /// Calculates the strategy when there are enemy border territories.
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <returns></returns>
        public bool CalculateForEnemyBorderTerritories(SuperRegion superRegion)
        {
            Region targetRegion = null, sourceRegion = null;
            bool transferDone = false;

            var invadingBorderTerritories = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.EnemyInvasionPaths);
            var invadingBorderTerritory = invadingBorderTerritories.FirstOrDefault();

            if (invadingBorderTerritory != null)
            {
                int enemyArmies = invadingBorderTerritory.NbrOfArmies;

                /* Let's see if we can attack. There is  60% change per attacking army. 
                 * We will be extra safe and use a 50% chance.
                 * This means we'll need at least double as much armies as our opponent.
                 * If this isn't the case, we'll send more armies to this region and defend our grounds.
                 * 
                 * */
                var possibleAttackingRegion = superRegion
                    .ChildRegions
                    .Find(PlayerType.Me)
                    .Where(region => region.Neighbours.Contains(invadingBorderTerritory))
                    .Where(region => (region.NbrOfArmies >= enemyArmies * 2 || region.NbrOfArmies > Configuration.Current.GetMaximumTreshold()) && region.NbrOfArmies > 5)
                    .OrderByDescending(region => region.NbrOfArmies)
                    .FirstOrDefault();

                //We can attack!
                if (possibleAttackingRegion != null)
                {
                    targetRegion = invadingBorderTerritory;
                    sourceRegion = possibleAttackingRegion;
                }

                /* We can't attack, so let's defend.
                 * We'll send armies to the region that can be attacked with the least number of armies
                 * We'll prefer sending from regions that can't be attacked.
                 **/
                else
                {
                    targetRegion = invadingBorderTerritory
                        .Neighbours
                        .Find(PlayerType.Me)
                        .Where(region => region.SuperRegion == superRegion)
                        .OrderBy(region => region.NbrOfArmies)
                        .FirstOrDefault();

                    if (targetRegion != null)
                    {
                        // Dont transfer from regions that have enemy neighbours
                        sourceRegion = targetRegion
                            .Neighbours
                            .Find(PlayerType.Me)
                            .Where(region => region.Neighbours.None(neighbour => neighbour.IsOccupiedBy(PlayerType.Opponent)))
                            .OrderByDescending(region => region.NbrOfArmies)
                            .FirstOrDefault();
                    }
                }

                transferDone = AddCurrentPairToTransferList(sourceRegion, targetRegion);

                var targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.ConquerCurrentSuperRegion);
                StrategyManager.ConquerNeutralRegions(targetRegions, Transfers, SourceStrategy.DominateCurrentSuperRegion);
            }

            return transferDone;
        }

        /// <summary>
        /// Calculates the strategy when there are enemy regions in the super region
        /// </summary>
        /// <param name="superRegion">The super region.</param>
        /// <returns></returns>
        public bool CalculateForEnemyRegions(SuperRegion superRegion)
        {
            Region targetRegion = null, sourceRegion = null;
            bool transferDone = false;

            var hostileRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.HostileRegions);
            if (hostileRegions.Any())
            {
                foreach (var hostileRegion in hostileRegions)
                {
                    int enemyArmies = hostileRegion.NbrOfArmies;

                    /* Let's see if we can attack. There is  60% change per attacking army. 
                     * We will be extra safe and use a 50% chance.
                     * This means we'll need at least double as much armies as our opponent.
                     * If this isn't the case, we'll send more armies to this region and defend our grounds.
                     * 
                     * */
                    var possibleAttackingRegion = Regions
                        .Find(PlayerType.Me)
                        .Where(region => region.Neighbours.Contains(hostileRegion))
                        .Where(
                            region =>
                                (region.NbrOfArmies >= enemyArmies * 2 + 1 ||
                                 region.NbrOfArmies > Configuration.Current.GetMaximumTreshold()))
                        .OrderByDescending(region => region.NbrOfArmies)
                        .FirstOrDefault();

                    //We can attack!
                    if (possibleAttackingRegion != null)
                    {
                        targetRegion = hostileRegion;
                        sourceRegion = possibleAttackingRegion;

                        var nbrOfArmies = enemyArmies * 2;

                        //If we're enclosed by ourself, attack with everything
                        if (sourceRegion.Neighbours.Count(n => n.IsOccupiedBy(PlayerType.Me)) == sourceRegion.Neighbours.Count - 1)
                        {
                            nbrOfArmies = sourceRegion.NbrOfArmies - 1;
                        }

                        transferDone = transferDone || AddCurrentPairToTransferList(sourceRegion, targetRegion, nbrOfArmies);
                    }

                    /* We can't attack, so let's defend.
                     * We'll send armies to the region that can be attacked with the least number of armies
                     * We'll prefer sending from regions that can't be attacked.
                     **/
                    else
                    {
                        targetRegion = hostileRegion
                            .Neighbours
                            .Find(PlayerType.Me)
                            .Where(region => region.SuperRegion == superRegion)
                            .OrderBy(region => region.NbrOfArmies)
                            .FirstOrDefault();

                        if (targetRegion != null)
                        {

                            sourceRegion = targetRegion
                                .Neighbours
                                .Find(PlayerType.Me)
                                .OrderByDescending(region => region.NbrOfArmies)
                                .FirstOrDefault();
                        }
                        else
                        {
                            //We can't defend a region, probably because we don't have armies nearby, so let's conquer some regions instead
                            var targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.ConquerCurrentSuperRegion);
                            StrategyManager.ConquerNeutralRegions(targetRegions, Transfers, SourceStrategy.DominateCurrentSuperRegion);
                        }
                    }
                }
            }

            /*
             * There is a hostile region in this super regio but we can not attack it 
             * Maybe we dont have enough armies nearby
             * So lets try doing something else, like conquering neutral armies
             */
            else
            {
                var targetRegions = StrategyManager.GetTargetRegions(superRegion, Transfers, TargetStrategy.ConquerCurrentSuperRegion);
                StrategyManager.ConquerNeutralRegions(targetRegions, Transfers, SourceStrategy.DominateCurrentSuperRegion);
            }

            return transferDone;
        }

        /// <summary>
        /// Armies that are stuck serve no purpose. We need to move them away where they can be of any use.
        /// </summary>
        /// <param name="stuckArmies">The stuck armies.</param>
        public void CalculateForStuckArmies(IEnumerable<Region> stuckArmies)
        {
            if (stuckArmies.Any())
            {
                foreach (var stuckArmie in stuckArmies)
                {
                    //Basic path finding to move away
                    for (int degree = 1; degree < 6; degree++)
                    {
                        var escapeRoute = GetNthDegreeNeighbours(stuckArmie, degree).FirstOrDefault();
                        if (escapeRoute != null)
                        {
                            var transfer = new ArmyTransfer
                            {
                                SourceRegion = stuckArmie,
                                TargetRegion = escapeRoute,
                                Armies = GetRequiredArmies(stuckArmie, escapeRoute)
                            };

                            Transfers.Add(transfer);
                            break;
                        }
                    }
                }
            }
        }

        private bool AddCurrentPairToTransferList(Region sourceRegion, Region targetRegion)
        {
            if (sourceRegion == null || targetRegion == null)
            {
                return false;
            }

            return AddCurrentPairToTransferList(sourceRegion, targetRegion,  GetRequiredArmies(sourceRegion, targetRegion));
        }

        private bool AddCurrentPairToTransferList(Region sourceRegion, Region targetRegion, int nbrOfArmies)
        {
            if (sourceRegion == null || targetRegion == null)
            {
                return false;
            }

            if (sourceRegion.NbrOfArmies > 3 || (sourceRegion.IsOccupiedBy(PlayerType.Me) && targetRegion.IsOccupiedBy(PlayerType.Me) && sourceRegion.NbrOfArmies > 1))
            {
                var transfer = new ArmyTransfer
                {
                    SourceRegion = sourceRegion, 
                    TargetRegion = targetRegion, 
                    Armies = nbrOfArmies
                };

                Transfers.Add(transfer);
                UpdateRegion(sourceRegion.ID, Configuration.Current.GetMyBotName(), sourceRegion.NbrOfArmies - nbrOfArmies);

                return true;
            }

            return false;
        }

        private static int GetRequiredArmies(Region sourceRegion, Region targetRegion)
        {
            return sourceRegion.NbrOfArmies - 1;
        }

        private static IEnumerable<Region> GetNthDegreeNeighbours(Region region, int degree)
        {
            switch (degree)
            { 
                default:
                    return Enumerable.Empty<Region>();
                case 1:
                    return region.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => !nn.IsOccupiedBy(PlayerType.Me)));
                case 2:
                    return region.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => !nnn.IsOccupiedBy(PlayerType.Me))));
                case 3:
                    return region.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => nnn.Neighbours
                                    .Any(nnnn => !nnnn.IsOccupiedBy(PlayerType.Me)))));
                case 4:
                    return region.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => nnn.Neighbours
                                    .Any(nnnn => nnnn.Neighbours
                                        .Any(nnnnn => !nnnnn.IsOccupiedBy(PlayerType.Me))))));
                case 5:
                    return region.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => nnn.Neighbours
                                    .Any(nnnn => nnnn.Neighbours
                                        .Any(nnnnn => nnnnn.Neighbours
                                            .Any(nnnnnn => !nnnnnn.IsOccupiedBy(PlayerType.Me)))))));
            }
        }
    }
}
