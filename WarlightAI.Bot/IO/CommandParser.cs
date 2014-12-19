// <copyright file="CommandParser.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.Linq;

using WarlightAI.GameBoard;

namespace WarlightAI.IO
{
    public class CommandParser
    {
        /// <summary>
        /// Parses the specified commandline.
        /// </summary>
        /// <param name="commandline">The commandline.</param>
        public void Parse(String commandline)
        {
            if (String.IsNullOrEmpty(commandline))
            {
                return;
            }

            String[] commandargs = commandline.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (commandargs.Length == 0)
            {
                return;
            }

            String command = commandargs[0].ToLowerInvariant();
            String subcommand = String.Empty;

            switch (command)
            {
                case "settings":
                    subcommand = commandargs[1].ToLowerInvariant();
                    switch (subcommand)
                    {
                        case "your_bot":
                            String mybotname = commandargs[2];
                            Configuration.Current.SetMyBotName(mybotname);
                            break;
                        case "opponent_bot":
                            String opponentbotname = commandargs[2];
                            Configuration.Current.SetOpponentBotName(opponentbotname);
                            break;
                        case "starting_armies":
                            int armies = Int32.Parse(commandargs[2]);
                            Configuration.Current.SetStartingArmies(armies);
                            break;
                        case "max_rounds":
                            int maxRounds = Int32.Parse(commandargs[2]);
                            Configuration.Current.SetMaxRounds(maxRounds);
                            break;
                    }
                    break;
                case "go":
                    subcommand = commandargs[1].Trim().ToLowerInvariant();
                    switch (subcommand)
                    {
                        case "place_armies":
                            CommandBuilder.OutputArmyPlacements(Board.Current.PlaceArmies());
                            break;
                        case "attack/transfer":
                            CommandBuilder.OutputArmyTransfers(Board.Current.TransferArmies());
                            break;
                    }
                    break;
                case "setup_map":
                    subcommand = commandargs[1].Trim().ToLowerInvariant();
                    switch (subcommand)
                    {
                        case "super_regions":
                            for (int i = 2; i < commandargs.Length; i++)
                            {
                                int id = Int32.Parse(commandargs[i]);
                                int reward = Int32.Parse(commandargs[++i]);
                                Board.Current.AddSuperRegion(id, reward);
                            }
                            break;
                        case "regions":
                            for (int i = 2; i < commandargs.Length; i++)
                            {
                                int id = Int32.Parse(commandargs[i]);
                                int superRegionId = Int32.Parse(commandargs[++i]);
                                Board.Current.AddRegion(id, superRegionId);
                            }
                            break;
                        case "neighbors":
                            for (int i = 2; i < commandargs.Length; i++)
                            {
                                int id = Int32.Parse(commandargs[i]);
                                String[] neighborstrings = commandargs[++i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                Board.Current.SetRegionNeighbors(id, neighborstrings);
                            }
                            Board.Current.CalculateSuperRegionsBorders();
                            break;
                        case "wastelands":
                            for (int i = 2; i < commandargs.Length; i++)
                            {
                                int id = Int32.Parse(commandargs[i]);
                                Board.Current.SetWasteland(id);
                            }
                            break;
                    }
                    break;
                case "pick_starting_region":
                    String[] regions = commandargs.Skip(2).ToArray();
                    Board.Current.MarkStartingRegions(regions);
                    CommandBuilder.OutputStartingRegion(Board.Current.PickFavoriteStartingRegion());
                    break;
                case "update_map":
                    Configuration.Current.SetRoundNumber(Configuration.Current.GetRoundNumber() + 1);
                    Board.Current.ClearRegions();
                    for (int i = 1; i < commandargs.Length; i++)
                    {
                        int regionid = Int32.Parse(commandargs[i]);
                        string player = commandargs[++i];
                        int nbrOfArmies = Int32.Parse(commandargs[++i]);
                        Board.Current.UpdateRegion(regionid, player, nbrOfArmies);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
