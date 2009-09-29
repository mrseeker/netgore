using System.Linq;
using NetGore.RPGComponents;

namespace DemoGame.Server
{
    public abstract class CharacterStats : CharacterStatsBase
    {
        protected CharacterStats(StatCollectionType statCollectionType) : base(statCollectionType)
        {
        }
    }
}