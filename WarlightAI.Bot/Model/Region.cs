// <copyright file="Region.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;

namespace WarlightAI.Model
{
    /// <summary>
    /// Class that defines a Region.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }


        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        /// <value>
        /// The player.
        /// </value>
        public Player Player { get; set; }


        /// <summary>
        /// Gets or sets the number of armies.
        /// </summary>
        /// <value>
        /// The number of armies.
        /// </value>
        public int NbrOfArmies { get; set; }

        /// <summary>
        /// Gets or sets if this region is a wasteland.
        /// </summary>
        /// <value>
        /// True if this region is a wasteland, otherwise false.
        /// </value>
        public bool IsWasteland { get; set; }

        /// <summary>
        /// Gets or sets the neighbours.
        /// </summary>
        /// <value>
        /// The neighbours.
        /// </value>
        public Regions Neighbours { get; set; }


        /// <summary>
        /// Gets or sets the region status.
        /// </summary>
        /// <value>
        /// The region status.
        /// </value>
        public RegionStatus RegionStatus { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class.
        /// </summary>
        public Region()
        {
            Neighbours = new Regions();
            RegionStatus = RegionStatus.Initialized;
        }

        /// <summary>
        /// Updates the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="nbrOfArmies">The number of armies.</param>
        public void Update(Player player, int nbrOfArmies)
        {
            Player = player;
            NbrOfArmies = nbrOfArmies;
        }

        /// <summary>
        /// Determines whether this region [is occupied by] [the specified player type].
        /// </summary>
        /// <param name="playerType">The player type.</param>
        /// <returns></returns>
        public bool IsOccupiedBy (PlayerType playerType)
        {
            return Player != null && Player.PlayerType == playerType;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("ID: {0} - Player: {1} - Armies: {2}", ID, Player, NbrOfArmies);
        }

    }
}
