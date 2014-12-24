using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarlightAI.GameBoard;
using WarlightAI.Helpers;

namespace WarlightAI.Tests
{
    [TestClass]
    public class HelpersTests
    {
        [TestMethod]
        public void ExtensionMethods_None()
        {
            //Arrange
            var filledList = new List<int> {1, 2, 3, 4, 5, 6};
            var emptyList = new List<int>();

            //Act
            var filledListIsEmpty = filledList.None();
            var emptyListIsEmpty = emptyList.None();

            //Assert
            Assert.IsFalse(filledListIsEmpty);
            Assert.IsTrue(emptyListIsEmpty);
        }

        [TestMethod]
        public void StrategyManager_FindEscapeRouteForStuckArmy()
        {
            var playerName = "player2";
            Configuration.Current.SetMyBotName(playerName);

            var sr = "setup_map super_regions 1 4 2 4 3 3 4 3";
            var r = "setup_map regions 1 1 2 1 3 1 4 1 5 1 6 1 7 2 8 2 9 2 10 2 11 3 12 3 13 3 14 4 15 4 16 4 17 4";
            var n = "setup_map neighbors 1 2,7,3 2 7,5,3,4,9,10 3 4 4 5,14,15 5 10,6,11,12,15,9 6 12 7 8,9 8 9 9 10,11 11 13,12 12 15,13,17 14 15,16 15 16,17 16 17";

            String[] commandargs = sr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 2; i < commandargs.Length; i++)
            {
                int id = Int32.Parse(commandargs[i]);
                int reward = Int32.Parse(commandargs[++i]);
                Board.Current.AddSuperRegion(id, reward);
            }

            commandargs = r.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < commandargs.Length; i++)
            {
                int id = Int32.Parse(commandargs[i]);
                int superRegionId = Int32.Parse(commandargs[++i]);
                Board.Current.AddRegion(id, superRegionId);
                Board.Current.UpdateRegion(id, playerName, 10);
            }

            commandargs = n.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 2; i < commandargs.Length; i++)
            {
                int id = Int32.Parse(commandargs[i]);
                String[] neighborstrings = commandargs[++i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                Board.Current.SetRegionNeighbors(id, neighborstrings);
            }
            Board.Current.CalculateSuperRegionsBorders();

            Board.Current.UpdateRegion(1, "neutral", 2);

            var escapeRoute = StrategyManager.FindEscapeRouteForStuckArmy(Board.Current.GetRegion(7));

        }
    }
}
