// <copyright file="Regions.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>18/12/2014</date>
using System;
using System.Collections.Generic;
using System.Linq;

namespace WarlightAI.Model
{
    /// <summary>
    /// A wrapper class for a collection of <see cref="Regions"/>
    /// </summary>
    public class Regions : List<Region>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Regions"/> class.
        /// </summary>
        public Regions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Regions"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public Regions(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Regions"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public Regions(IEnumerable<Region> collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets the specified region.
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        /// <returns></returns>
        public Region Get(int regionId)
        {
            return Find(region => region.ID == regionId);
        }

        /// <summary>
        /// Gets the specified region identifier.
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        /// <returns></returns>
        public Region Get(string regionId)
        {
            return Find(region => region.ID == Int32.Parse(regionId));
        }

        /// <summary>
        /// Finds all regions that are occupied by a specified player.
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns></returns>
        public IEnumerable<Region> Find(PlayerType playerType)
        {
            return this.Where(region => region.Player != null && region.Player.PlayerType == playerType);
        }
    }
}
