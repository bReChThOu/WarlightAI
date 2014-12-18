// <copyright file="Board.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

            SuperRegions
                .Get(superRegionId)
                .AddChildRegion(region);
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
                                    .Where(neighbor => SuperRegions.Get(neighbor).ID != superregion.ID));


            var borderTerritories = superregion
               .ChildRegions
                .Where(region => region
                                    .Neighbours
                                    .Any(neighbor => SuperRegions.Get(neighbor).ID != superregion.ID));

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
                .OrderByDescending(region => SuperRegions.Get(region).Priority)
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
                    .GroupBy(region => SuperRegions.Get(region).ID)
                    .ToList(); // Prevent possible multiple enumerations

                int opponentTotalSuperRegions =
                    Regions
                    .Find(PlayerType.Opponent)
                    .GroupBy(region => SuperRegions.Get(region).ID)
                    .Count();

                if (myTotalSuperRegions.Count() == 3 && opponentTotalSuperRegions == 3)
                {
					Configuration.Current.SetStartRoundNumber(4);
                    placements.Add(new ArmyPlacement
                    {
                        Armies = 2, 
                        Region = Regions.Find(PlayerType.Me).First(r => SuperRegions.Get(r).ID == myTotalSuperRegions.First().Key)
                    });

                    placements.Add(new ArmyPlacement
                    {
                        Armies = 2, 
                        Region = Regions.Find(PlayerType.Me).First(r => SuperRegions.Get(r).ID == myTotalSuperRegions.Skip(1).First().Key)
                    });

                    placements.Add(new ArmyPlacement
                    {
                        Armies = 1, 
                        Region = Regions.Find(PlayerType.Me).First(r => SuperRegions.Get(r).ID == myTotalSuperRegions.Skip(2).First().Key)
                    });

                    UpdateRegions(placements);
                    return placements;
                }

				Configuration.Current.SetStartRoundNumber(2);

            }

            var primaryRegion = Regions
                .Find(PlayerType.Me)
                .Where(region => region.NbrOfArmies < 100)
                .OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)))
                .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                .ThenBy(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                .ThenBy(region => (SuperRegions.Get(region).ChildRegions.Count(child => child.Player.PlayerType == PlayerType.Me)))
                .ThenByDescending(region => region.NbrOfArmies)
                .FirstOrDefault();

            if (startingArmies == 5)
            {
                var secundaryRegion = Regions
                   .Find(PlayerType.Me)
                   .Where(region => SuperRegions.Get(region) != SuperRegions.Get(primaryRegion))
                   .Where(region => region.NbrOfArmies < 100)
                   .OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)))
                   .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => (SuperRegions.Get(region).ChildRegions.Count(child => child.Player.PlayerType == PlayerType.Me)))
                   .ThenByDescending(region => region.NbrOfArmies)
                   .FirstOrDefault();

                var armyplacement = new ArmyPlacement { Armies = 4, Region = primaryRegion };
                placements.Add(armyplacement);

                if (secundaryRegion == null)
                {
                    secundaryRegion = primaryRegion;
                }

                armyplacement = new ArmyPlacement { Armies = startingArmies - 4, Region = secundaryRegion };
                placements.Add(armyplacement);
            }
            if (startingArmies >= 7 && startingArmies <= 9)
            {
                var secundaryRegion = Regions
                   .Find(PlayerType.Me)
                   .Where(region => SuperRegions.Get(region) != SuperRegions.Get(primaryRegion))
                   .Where(region => region.NbrOfArmies < 100)
                   .OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)))
                   .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => (SuperRegions.Get(region).ChildRegions.Count(child => child.Player.PlayerType == PlayerType.Me)))
                   .ThenByDescending(region => region.NbrOfArmies)
                   .FirstOrDefault();

                var armyplacement = new ArmyPlacement { Armies = 5, Region = primaryRegion };
                placements.Add(armyplacement);

                if (secundaryRegion == null)
                {
                    secundaryRegion = primaryRegion;
                }

                armyplacement = new ArmyPlacement { Armies = startingArmies - 5, Region = secundaryRegion };
                placements.Add(armyplacement);
            }
            if (startingArmies > 9 && startingArmies <= 18)
            {
                var secundaryRegion = Regions
                   .Find(PlayerType.Me)
                   .Where(region => SuperRegions.Get(region) != SuperRegions.Get(primaryRegion))
                   .Where(region => region.NbrOfArmies < 100)
                   .OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)))
                   .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenByDescending(region => (SuperRegions.Get(region).ChildRegions.Count(child => child.Player.PlayerType == PlayerType.Me)))
                   .ThenByDescending(region => region.NbrOfArmies)
                   .FirstOrDefault();

                var armyplacement = new ArmyPlacement { Armies = 9, Region = primaryRegion };
                placements.Add(armyplacement);

                if (secundaryRegion == null)
                {
                    secundaryRegion = primaryRegion;
                }

                armyplacement = new ArmyPlacement { Armies = startingArmies - 9, Region = secundaryRegion };
                placements.Add(armyplacement);
            }
            if (startingArmies > 18)
            {
                var secundaryRegion = Regions
                   .Find(PlayerType.Me)
                   .Where(region => SuperRegions.Get(region) != SuperRegions.Get(primaryRegion))
                   .Where(region => region.NbrOfArmies < 100)
                   .OrderByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Opponent)))
                   .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                   .ThenBy(region => (SuperRegions.Get(region).ChildRegions.Count(child => child.Player.PlayerType == PlayerType.Me)))
                   .ThenByDescending(region => region.NbrOfArmies)
                   .FirstOrDefault();

                var armyplacement = new ArmyPlacement { Armies = startingArmies - 9, Region = primaryRegion };
                placements.Add(armyplacement);

                if (secundaryRegion == null)
                {
                    secundaryRegion = primaryRegion;
                }

                armyplacement = new ArmyPlacement { Armies = 9, Region = secundaryRegion };
                placements.Add(armyplacement);
            }

            UpdateRegions(placements);
            return placements;
        }

        public IEnumerable<ArmyTransfer> TransferArmies()
        {
            /*
             * Inspect Border Territories foreach super region
             * If there are no enemy armies sighted: let's conquer the continent
             * 
             * If there are enemy armies sighted: let's move some troops to defend those invasion paths
             * */
            var transfers = new List<ArmyTransfer>();
            
            SuperRegions.ForEach(
                (superregion) =>
                {
                    //Do i have any regions in this super region?
                    bool skipSuperRegion = !superregion
                        .ChildRegions
                        .Any(region => region.IsOccupiedBy(PlayerType.Me));

                    if (!skipSuperRegion)
                    {
                        int borderTerritoriesWithEnemyArmies = superregion.BorderTerritories
                            .Where(bt => bt.Neighbours.Any(btn => btn.IsOccupiedBy(PlayerType.Opponent)))
                            .Where(bt => (bt.IsOccupiedBy(PlayerType.Me)) || bt.Neighbours.Any(btn => btn.IsOccupiedBy(PlayerType.Me) && SuperRegions.Get(btn) == superregion))
                            .Count();

                        int regionsWithEnemyArmies = superregion.ChildRegions
                            .Where(region => region.IsOccupiedBy(PlayerType.Opponent))
                            .Where(region => region.Neighbours.Any(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)))
                            .Count();

                        bool transferDone = false;

                        /*
                         * There is nobody in our way, let's conquer the continent, or even explore a new continent.
                         * */
                        if (borderTerritoriesWithEnemyArmies == 0 && regionsWithEnemyArmies == 0)
                        {
                            Region targetRegion = null, sourceRegion = null;

                            var targetRegions = superregion
                                .ChildRegions
                                .Where(region => region.IsOccupiedBy(PlayerType.Neutral))
                                .OrderBy(region => region.Neighbours.Count(neighbor => SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                                .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                                .ThenByDescending(region =>
                                    region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum()
                                );

                            targetRegion = targetRegions.FirstOrDefault();

                            /* No neutral armies found, that should mean we own the continent.
                             * Let's explore the world and go to a new super region
                             * */
                            if (targetRegion == null)
                            {
                                targetRegions = superregion
                                   .InvasionPaths
                                   .Where(region => region.IsOccupiedBy(PlayerType.Neutral))
                                   .OrderByDescending(region =>
                                       region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum()

                                   );
                                targetRegion = targetRegions.FirstOrDefault();

                                if (targetRegion != null)
                                {
                                    //When we seem to be alone on this superregion, we'll want to make more than 1 move
                                    foreach (var cTargetRegion in targetRegions)
                                    {
                                        sourceRegion = cTargetRegion
                                        .Neighbours
                                        .Where(region => region.IsOccupiedBy(PlayerType.Me) && region.NbrOfArmies > 5)
                                        .Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0)
                                        .OrderByDescending(region => region.NbrOfArmies)
                                        .FirstOrDefault();

                                        transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion, transfers);
                                    }
                                }
                                else
                                {
                                    targetRegion = superregion
                                    .InvasionPaths
                                    .Where(region => region.IsOccupiedBy(PlayerType.Opponent))
                                    .OrderByDescending(region =>
                                            region.Neighbours
                                                .Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me))
                                                .Where(neighbor => neighbor.NbrOfArmies > 5)
                                                .Where(neighbor => neighbor.NbrOfArmies > region.NbrOfArmies * 2)
                                                .Select(reg => reg.NbrOfArmies).Sum()
                                    )
                                    .FirstOrDefault();

                                    if (targetRegion != null)
                                    {
                                        sourceRegion = targetRegion
                                        .Neighbours
                                        .Where(region => region.IsOccupiedBy(PlayerType.Me) && region.NbrOfArmies > 5)
                                        .OrderByDescending(region => region.NbrOfArmies)
                                        .FirstOrDefault();

                                        transferDone = AddCurrentPairToTransferList(sourceRegion, targetRegion, transfers);
                                    }
                                }
                            }


                            else
                            {
                                //When we seem to be alone on this superregion, we'll want to make more than 1 move
                                for(int i = 0; i < targetRegions.Count(); i++) 
                                {
                                    var cTargetRegion = targetRegions.ToArray()[i];
                                    sourceRegion = cTargetRegion
                                    .Neighbours
                                    .Where(region => SuperRegions.Get(region) == superregion)
                                    .Where(region => region.IsOccupiedBy(PlayerType.Me) && region.NbrOfArmies > 5)
                                    .Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0)
                                    .OrderByDescending(region => region.NbrOfArmies)
                                    .FirstOrDefault();

                                    if (sourceRegion != null)
                                    {

                                        //We can conquer multiple neutral regions with one army.
                                        if (sourceRegion.NbrOfArmies > 10)
                                        {
                                            i--;
                                            for (int n = sourceRegion.NbrOfArmies; n > 5 && i < targetRegions.Count() - 1; )
                                            {
                                                cTargetRegion = targetRegions.ToArray()[++i];
                                                int nbrOfArmies = 5;
                                                if (n < 10)
                                                {
                                                    nbrOfArmies = n - 1;
                                                }
                                                transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion, transfers, nbrOfArmies);
                                                n = n - nbrOfArmies;
                                            }
                                        }
										else
										{
											if (targetRegion.NbrOfArmies <= sourceRegion.NbrOfArmies)
											{
												transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion, transfers);
											}
										}
                                    }
                                }
                            }
                            /*
                             * Its not good to leave 3 armies on 1 region and 4 armies on another region.
                             * We have to move armies to the largest region to conquer the super region faster.
                             * For now, only do this when there are no enemies spotted
                             * */
                            var largestRegion = superregion
                                .ChildRegions
                                .Find(PlayerType.Me)
                                .Where(region => region.NbrOfArmies > 1)
                                .Where(region => !region.Neighbours.All(n => n.IsOccupiedBy(PlayerType.Me)))
                                .OrderByDescending(region => region.NbrOfArmies)
                                .FirstOrDefault();

                            if (largestRegion != null)
                            {
                                var qualifiedArmies = superregion
                                    .ChildRegions
                                    .Find(PlayerType.Me)
                                    .Where(region => region.NbrOfArmies > 1)
                                    .Where(region => largestRegion.Neighbours.Contains(region))
                                    .Where(region => !transfers.Any(t => t.SourceRegion.ID == region.ID));
                                if (qualifiedArmies.Any())
                                {
                                    foreach (var qualifiedArmy in qualifiedArmies)
                                    {
                                        transferDone = AddCurrentPairToTransferList(qualifiedArmy, largestRegion, transfers);
                                    }
                                }
                            }

                        }

                        /*
                         * There is an enemy army nearby. Let's not let them take this continent.
                         * */

                        if (borderTerritoriesWithEnemyArmies > 0 && !transferDone)
                        {
                            Region targetRegion = null, sourceRegion = null;

                            var invadingBorderTerritory = superregion
                                .InvasionPaths
                                .Where(invasionpath => invasionpath.IsOccupiedBy(PlayerType.Opponent))
                                .OrderByDescending(region => region.NbrOfArmies)
                                .FirstOrDefault();
                            /*
                             * We can't attack the biggest enemy region. Let's try another
                             * Every conquered region means the opponent might loose his super region bonus!
                             * */
                            if (invadingBorderTerritory == null)
                            {
                                invadingBorderTerritory = superregion
                                .InvasionPaths
                                .Where(invasionpath => invasionpath.IsOccupiedBy(PlayerType.Opponent))
                                .OrderBy(region => region.NbrOfArmies)
                                .FirstOrDefault();
                            }

                            if (invadingBorderTerritory != null)
                            {

                                int enemyArmies = invadingBorderTerritory.NbrOfArmies;

                                /* Let's see if we can attack. There is  60% change per attacking army. 
                                 * We will be extra safe and use a 50% chance.
                                 * This means we'll need at least double as much armies as our opponent.
                                 * If this isn't the case, we'll send more armies to this region and defend our grounds.
                                 * 
                                 * */

                                var possibleAttackingRegion = superregion
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
                                        .Where(region => SuperRegions.Get(region) == superregion)
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
                                }

                                transferDone = AddCurrentPairToTransferList(sourceRegion, targetRegion, transfers);

                                var targetRegions = superregion
                                    .ChildRegions
                                    .Where(region => region.IsOccupiedBy(PlayerType.Neutral))
                                    .OrderBy(region => region.Neighbours.Count(neighbor => SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                                    .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                                    .ThenByDescending(region =>
                                        region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum()
                                    );

                                //When we seem to be alone on this superregion, we'll want to make more than 1 move
                                foreach (var cTargetRegion in targetRegions)
                                {
                                    sourceRegion = cTargetRegion
                                    .Neighbours
                                    .Where(region => SuperRegions.Get(region) == superregion)
                                    .Where(region => region.IsOccupiedBy(PlayerType.Me) && region.NbrOfArmies > 5)
                                    .Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0)
                                    .OrderByDescending(region => region.NbrOfArmies)
                                    .FirstOrDefault();

                                    transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion, transfers);
                                }
                            }
                        }


                        /*
                         * There is an enemy army in this super region. Let's not let them take the whole continent.
                         * */
                        if (regionsWithEnemyArmies > 0 && !transferDone)
                        {
                            Region targetRegion = null, sourceRegion = null;

                            var hostileRegion = superregion
                                .ChildRegions
                                .Where(region => region.IsOccupiedBy(PlayerType.Opponent))
								.Where(region => region.Neighbours.Any(n => n.IsOccupiedBy(PlayerType.Me) && n.NbrOfArmies > 5 && (n.NbrOfArmies >= region.NbrOfArmies * 2 || n.NbrOfArmies > Configuration.Current.GetMaximumTreshold())))
                                .OrderBy(region => region.NbrOfArmies)
                                .FirstOrDefault();

                            if (hostileRegion != null)
                            {
                                int enemyArmies = hostileRegion.NbrOfArmies;

                                /* Let's see if we can attack. There is  60% change per attacking army. 
                                 * We will be extra safe and use a 50% chance.
                                 * This means we'll need at least double as much armies as our opponent.
                                 * If this isn't the case, we'll send more armies to this region and defend our grounds.
                                 * 
                                 * */

                                var possibleAttackingRegion = superregion
                                    .ChildRegions
                                    .Find(PlayerType.Me)
                                    .Where(region => region.Neighbours.Contains(hostileRegion))
									.Where(region => (region.NbrOfArmies >= enemyArmies * 2 || region.NbrOfArmies > Configuration.Current.GetMaximumTreshold()) && region.NbrOfArmies > 5)
                                    .OrderByDescending(region => region.NbrOfArmies)
                                    .FirstOrDefault();

                                //We can attack!
                                if (possibleAttackingRegion != null)
                                {
                                    targetRegion = hostileRegion;
                                    sourceRegion = possibleAttackingRegion;
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
                                        .Where(region => SuperRegions.Get(region) == superregion)
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
                                        var targetRegions = superregion
                                            .ChildRegions
                                            .Where(region => region.IsOccupiedBy(PlayerType.Neutral))
                                            .OrderBy(region => region.Neighbours.Count(neighbor => SuperRegions.Get(neighbor) != SuperRegions.Get(region)) > 0 ? 1 : 0)
                                            .ThenByDescending(region => region.Neighbours.Count(neighbor => neighbor.IsOccupiedBy(PlayerType.Neutral) && SuperRegions.Get(neighbor) == SuperRegions.Get(region)) > 0 ? 1 : 0)
                                            .ThenByDescending(region =>
                                                region.Neighbours.Where(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)).Select(reg => reg.NbrOfArmies).Sum()
                                            );

                                        foreach (var cTargetRegion in targetRegions)
                                        {
                                            sourceRegion = cTargetRegion
                                            .Neighbours
                                            .Where(region => SuperRegions.Get(region) == superregion)
                                            .Where(region => region.IsOccupiedBy(PlayerType.Me) && region.NbrOfArmies > 5)
                                            .Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0)
                                            .OrderByDescending(region => region.NbrOfArmies)
                                            .FirstOrDefault();

                                            if (sourceRegion != null)
                                            {
                                                transferDone = AddCurrentPairToTransferList(sourceRegion, cTargetRegion, transfers);
                                            }
                                        }
                                    }
                                }

                                if (!transferDone)
                                {
                                    transferDone = AddCurrentPairToTransferList(sourceRegion, targetRegion, transfers);
                                }
                            }
                        }
                        /*
                         * Let's see if we can move some troops away from the inland where they can't do anything
                         * besides being stuck
                         * */
                        var stuckArmies = superregion
                                            .ChildRegions
                                            .Find(PlayerType.Me)
                                            .Where(region => transfers.Count(t => t.SourceRegion.ID == region.ID) == 0)
                                            .Where(region => transfers.Count(t => t.TargetRegion.ID == region.ID) == 0)
                                            .Where(region => region.NbrOfArmies > 1 && region.Neighbours.All(neighbour => neighbour.IsOccupiedBy(PlayerType.Me)));

                        if (stuckArmies.Any())
                        {
                            foreach (var stuckArmie in stuckArmies)
                            {
                                //Basic path finding to move away
                                for (int degree = 1; degree < 6; degree++)
                                {
                                    var escapeRoutes = GetNthDegreeNeighbours(stuckArmie, degree);
                                    if (escapeRoutes.Any())
                                    {
                                        var freeway = escapeRoutes.First();
                                        var transfer = new ArmyTransfer
                                        {
                                            SourceRegion = stuckArmie, 
                                            TargetRegion = freeway, 
                                            Armies = GetRequiredArmies(stuckArmie, freeway)
                                        };

                                        transfers.Add(transfer);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            );

            //Don't attack the enemy if we are below the starting round, instead skip this move
			if (Configuration.Current.GetRoundNumber() < Configuration.Current.GetStartRoundNumber())
            {
                transfers.ForEach(
                    transfer =>
                    {
                        if (transfer.TargetRegion.IsOccupiedBy(PlayerType.Opponent))
                        {
                            transfers.Remove(transfer);
                        }
                    }
                );
            }

            return transfers;
        }

        private bool AddCurrentPairToTransferList(Region sourceRegion, Region targetRegion, List<ArmyTransfer> transfers)
        {
            if (sourceRegion == null || targetRegion == null)
            {
                return false;
            }

            return AddCurrentPairToTransferList(sourceRegion, targetRegion, transfers, GetRequiredArmies(sourceRegion, targetRegion));

        }

        private bool AddCurrentPairToTransferList(Region sourceRegion, Region targetRegion, List<ArmyTransfer> transfers, int nbrOfArmies)
        {
            if (sourceRegion == null || targetRegion == null)
            {
                return false;
            }

            if (sourceRegion.NbrOfArmies > 5 || (sourceRegion.Player.PlayerType == PlayerType.Me && targetRegion.Player.PlayerType == PlayerType.Me && sourceRegion.NbrOfArmies > 1))
            {
                var transfer = new ArmyTransfer
                {
                    SourceRegion = sourceRegion, 
                    TargetRegion = targetRegion, 
                    Armies = nbrOfArmies
                };

                transfers.Add(transfer);
				UpdateRegion(sourceRegion.ID, Configuration.Current.GetMyBotName(), sourceRegion.NbrOfArmies - nbrOfArmies);

                return true;
            }

            return false;
        }

        private static int GetRequiredArmies(Region sourceRegion, Region targetRegion)
        {
            return sourceRegion.NbrOfArmies - 1;
        }

        private static IEnumerable<Region> GetNthDegreeNeighbours(Region stuckArmie, int degree)
        {
            switch (degree)
            { 
                default:
                    return Enumerable.Empty<Region>();
                case 1:
                    return stuckArmie.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => !nn.IsOccupiedBy(PlayerType.Me)));
                case 2:
                    return stuckArmie.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => !nnn.IsOccupiedBy(PlayerType.Me))));
                case 3:
                    return stuckArmie.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => nnn.Neighbours
                                    .Any(nnnn => !nnnn.IsOccupiedBy(PlayerType.Me)))));
                case 4:
                    return stuckArmie.Neighbours
                        .Where(n => n.Neighbours
                            .Any(nn => nn.Neighbours
                                .Any(nnn => nnn.Neighbours
                                    .Any(nnnn => nnnn.Neighbours
                                        .Any(nnnnn => !nnnnn.IsOccupiedBy(PlayerType.Me))))));
                case 5:
                    return stuckArmie.Neighbours
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
