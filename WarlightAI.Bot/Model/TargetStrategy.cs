// <copyright file="TargetStrategy.cs">
//        Copyright (c) 2014 All Rights Reserved
// </copyright>
// <author>Brecht Houben</author>
// <date>19/12/2014</date>

namespace WarlightAI.Model
{
    /// <summary>
    /// Target Strategy Enumeration
    /// </summary>
    public enum TargetStrategy
    {
        /// <summary>
        /// The conquer current super region strategy
        /// </summary>
        ConquerCurrentSuperRegion,

        /// <summary>
        /// The conquer all strategy
        /// </summary>
        ConquerAll,

        /// <summary>
        /// The conquer other super regions strategy
        /// </summary>
        ConquerOtherSuperRegions,

        /// <summary>
        /// Target enemy invasion paths strategy
        /// </summary>
        EnemyInvasionPaths,

    }
}
