// <copyright file="ArmyPlacement.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>14/03/2014</date>

namespace WarlightAI.Model
{
    /// <summary>
    /// Class that represents an army placement
    /// </summary>
    public class ArmyPlacement
    {
        /// <summary>
        /// Gets or sets the armies.
        /// </summary>
        /// <value>
        /// The armies.
        /// </value>
        public int Armies { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public Region Region { get; set; }
    }
}
