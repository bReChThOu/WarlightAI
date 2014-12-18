// <copyright file="SuperRegion.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System.Collections.Generic;

namespace WarlightAI.Model
{

    /// <summary>
    /// Class that defines a Super Region. A Super Region is a continent.
    /// </summary>
    public class SuperRegion
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the reward.
        /// </summary>
        /// <value>
        /// The reward.
        /// </value>
        public int Reward { get; set; }

        /// <summary>
        /// Gets or sets the invasion paths.
        /// </summary>
        /// <value>
        /// The invasion paths.
        /// </value>
        public IEnumerable<Region> InvasionPaths { get; set; }

        /// <summary>
        /// Gets or sets the border territories.
        /// </summary>
        /// <value>
        /// The border territories.
        /// </value>
        public IEnumerable<Region> BorderTerritories { get; set; }

        /// <summary>
        /// Gets the child regions.
        /// </summary>
        /// <value>
        /// The child regions.
        /// </value>
        public Regions ChildRegions { get; internal set;}

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperRegion"/> class.
        /// </summary>
        public SuperRegion()
        {
            ChildRegions = new Regions();
        }

        /// <summary>
        /// Adds the child region.
        /// </summary>
        /// <param name="region">The region.</param>
        public void AddChildRegion(Region region)
        {
            ChildRegions.Add(region);
            Priority = 1000 / ChildRegions.Count;
        }

        public override string ToString()
        {
            return string.Format("ID: {0} - ChildRegions: {1}", ID, ChildRegions.Count);
        }

        
    }
}
