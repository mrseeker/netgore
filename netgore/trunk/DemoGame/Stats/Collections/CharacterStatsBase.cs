using System.Linq;
using NetGore.RPGComponents;

namespace DemoGame
{
    /// <summary>
    /// Base class for a collection of stats for Characters.
    /// </summary>
    public abstract class CharacterStatsBase : FullStatCollection
    {
        protected CharacterStatsBase(StatCollectionType statCollectionType) : base(statCollectionType)
        {
        }
    }
}