using System.Linq;
using NetGore;

// TODO: !! Try to remove

namespace DemoGame.Server.Queries
{
    public class SelectMapSpawnQueryValues
    {
        public byte Amount { get; private set; }
        public CharacterTemplateID CharacterTemplateID { get; private set; }
        public MapSpawnValuesID ID { get; private set; }
        public MapIndex MapIndex { get; private set; }
        public MapSpawnRect MapSpawnRect { get; private set; }

        public SelectMapSpawnQueryValues(MapSpawnValuesID id, CharacterTemplateID characterTemplateID, MapIndex mapIndex,
                                         byte amount, MapSpawnRect mapSpawnRect)
        {
            ID = id;
            CharacterTemplateID = characterTemplateID;
            MapIndex = mapIndex;
            Amount = amount;
            MapSpawnRect = mapSpawnRect;
        }
    }
}