// <copyright file="GameSettings.cs">
//        Copyright (c) 2013 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>

namespace WarlightAI.GameBoard
{
    public class GameSettings
    {
        /// <summary>
        /// Gets or sets the starting armies.
        /// </summary>
        /// <value>
        /// The starting armies.
        /// </value>
        public int StartingArmies { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of rounds.
        /// </summary>
        /// <value>
        /// The maximum number of rounds.
        /// </value>
        public int MaxRounds { get; set; }

        /// <summary>
        /// Gets or sets the round number.
        /// </summary>
        /// <value>
        /// The round number
        /// </value>
        public int RoundNumber { get; set; }

        /// <summary>
        /// Gets or sets the start round number.
        /// </summary>
        /// <value>
        /// The start round number
        /// </value>
        public int StartRoundNumber { get; set; }
    }
}
