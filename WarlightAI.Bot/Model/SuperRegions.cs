// <copyright file="SuperRegions.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>18/12/2014</date>
using System.Collections.Generic;

namespace WarlightAI.Model
{
    /// <summary>
    /// A wrapper class for a collection of <see cref="SuperRegions"/>
    /// </summary>
    public class SuperRegions : List<SuperRegion>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperRegions"/> class.
        /// </summary>
        public SuperRegions() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperRegions"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public SuperRegions(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperRegions"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public SuperRegions(IEnumerable<SuperRegion> collection) : base (collection)
        {
        }

        /// <summary>
        /// Gets the specified super region.
        /// </summary>
        /// <param name="superRegionId">The super region identifier.</param>
        /// <returns></returns>
        public SuperRegion Get(int superRegionId)
        {
            return Find(superRegion => superRegion.ID == superRegionId);
        }

        /// <summary>
        /// Gets the super region for the region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public SuperRegion Get(Region region)
        {
            return Find(superRegion => superRegion.ChildRegions.Contains(region));
        }
    }
}