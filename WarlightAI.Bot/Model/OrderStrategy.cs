// <copyright file="OrderStrategy.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>19/12/2014</date>

namespace WarlightAI.Model
{
    /// <summary>
    /// Order strategy enumeration
    /// </summary>
    public enum OrderStrategy
    {
        /// <summary>
        /// Regions that have no neighbours in other super regions come first
        /// </summary>
        InternalFirst,

        /// <summary>
        /// Regions that have neutral neighbours in the same super region come first
        /// </summary>
        NeutralNeighboursFirst,

        /// <summary>
        /// Regions that have neutral neighbours in other super regions come first
        /// </summary>
        NeutralNeighboursOnOtherSuperRegionsFirst,

        /// <summary>
        /// Regions on small super regions come first
        /// </summary>
        SmallSuperRegionsFirst,

        /// <summary>
        /// Regions that have enemy neighbours come first
        /// </summary>
        EnemyNeighboursFirst,

        /// <summary>
        /// Regions are ordered by the number of armies i have nearby
        /// </summary>
        MostArmiesNearby,

        /// <summary>
        /// Regions are ordered by the number of qualified armies i have nearby
        /// </summary>
        MostQualifiedArmiesNearby,

        /// <summary>
        /// Regions are ordered by the number of armies
        /// </summary>
        NumberOfArmies,
    }
}
