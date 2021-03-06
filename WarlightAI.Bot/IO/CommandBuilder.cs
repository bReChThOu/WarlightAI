﻿// <copyright file="CommandBuilder.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>10/03/2014</date>
using System;
using System.Collections.Generic;
using System.Linq;

using WarlightAI.GameBoard;
using WarlightAI.Helpers;
using WarlightAI.Model;

namespace WarlightAI.IO
{
    public class CommandBuilder
    {
        /// <summary>
        /// Outputs the starting region.
        /// </summary>
        /// <param name="region">The region.</param>
        public static void OutputStartingRegion(Region region)
        {
            Output(region.ID.ToString()); //.ToString() needed for mono compliance
        }

        /// <summary>
        /// Outputs the army placements.
        /// </summary>
        /// <param name="placements">The placements.</param>
        public static void OutputArmyPlacements(IEnumerable<ArmyPlacement> placements)
        {
            Output(String.Join(",", placements.Select(placement => String.Format("{0} place_armies {1} {2}", Configuration.Current.GetMyBotName(), placement.Region.ID, placement.Armies)).ToArray()));
        }

        /// <summary>
        /// Outputs the army transfers.
        /// </summary>
        /// <param name="transfers">The transfers.</param>
        public static void OutputArmyTransfers(IEnumerable<ArmyTransfer> transfers)
        {
            if (transfers.None())
            {
                Output("No moves");
                return;
            }
            Output(String.Join(",", transfers.Select(transfer => String.Format("{0} attack/transfer {1} {2} {3}", Configuration.Current.GetMyBotName(), transfer.SourceRegion.ID, transfer.TargetRegion.ID, transfer.Armies)).ToArray()));
        }


        /// <summary>
        /// Ouputs the specified line.
        /// </summary>
        /// <param name="line">The line.</param>
        private static void Output(String line)
        {
            Console.WriteLine(line);
        }
    }
}
