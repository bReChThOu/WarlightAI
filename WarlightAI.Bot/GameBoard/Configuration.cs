using WarlightAI.Model;
// <copyright file="ConfigFactory.cs">
//        Copyright (c) 2013 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WarlightAI.GameBoard
{
    public class Configuration
    {
		private static Configuration _instance;

        /// <summary>
        /// The instance
        /// </summary>
		public static Configuration Current
        {
            [DebuggerStepThrough]
            get 
            {
                if (_instance == null)
                {
                    _instance = new Configuration();
                }
                
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        private List<Player> Players { get; set; }

        /// <summary>
        /// Gets or sets the game settings.
        /// </summary>
        /// <value>
        /// The game settings.
        /// </value>
        private GameSettings GameSettings { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Configuration"/> class from being created.
        /// </summary>
        private Configuration()
        {
            Players = new List<Player>();
            Players.Add(new Player() { PlayerType = PlayerType.Neutral, Name = "Neutral" });
            GameSettings = new GameSettings();
        }

        /// <summary>
        /// Gets the name of the player.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Player GetPlayerByName(String name)
        {
			return Players
				.FirstOrDefault(player => String.Equals(player.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Sets the name of my bot.
        /// </summary>
        /// <param name="botname">The botname.</param>
        public void SetMyBotName(String botname)
        {
            Players.Add(new Player() { PlayerType = PlayerType.Me, Name = botname });
        }

        public String GetMyBotName()
        {
            return Players
                .FirstOrDefault(player => player.PlayerType == PlayerType.Me)
                .Name;
        }

        /// <summary>
        /// Sets the name of the opponent bot.
        /// </summary>
        /// <param name="botname">The botname.</param>
        public void SetOpponentBotName(String botname)
        {
            Players.Add(new Player() { PlayerType = PlayerType.Opponent, Name = botname });
        }

        /// <summary>
        /// Sets the starting armies.
        /// </summary>
        /// <param name="startingArmies">The starting armies.</param>
        public void SetStartingArmies(int startingArmies)
        {
            GameSettings.StartingArmies = startingArmies;
        }

        /// <summary>
        /// Sets the maximum number of rounds.
        /// </summary>
        /// <param name="maxRounds">The maximum number of rounds.</param>
        public void SetMaxRounds(int maxRounds)
        {
            GameSettings.MaxRounds = maxRounds;
        }

        /// <summary>
        /// Gets the starting armies.
        /// </summary>
        /// <returns></returns>
        public int GetStartingArmies()
        {
            return GameSettings.StartingArmies;
        }

        /// <summary>
        /// Sets the round number.
        /// </summary>
        /// <param name="round">The round.</param>
        public void SetRoundNumber(int round)
        {
            GameSettings.RoundNumber = round;
        }

        /// <summary>
        /// Gets the round number.
        /// </summary>
        /// <returns></returns>
        public int GetRoundNumber()
        {
            return GameSettings.RoundNumber;
        }

        /// <summary>
        /// Sets the start round number.
        /// </summary>
        /// <param name="round">The round.</param>
        public void SetStartRoundNumber(int round)
        {
            GameSettings.StartRoundNumber = round;
        }

        /// <summary>
        /// Gets the start round number.
        /// </summary>
        /// <returns></returns>
        public int GetStartRoundNumber()
        {
            return GameSettings.StartRoundNumber;
        }

        /// <summary>
        /// Gets the maximum treshold.
        /// </summary>
        /// <returns></returns>
        public int GetMaximumTreshold()
        {
            if (GetRoundNumber() > 78)
            {
                return 300;
            }
            if (GetRoundNumber() > 65)
            {
                return 400;
            }
            if (GetRoundNumber() > 50)
            {
                return 500;
            }
            return 500;
        }
    }
}
