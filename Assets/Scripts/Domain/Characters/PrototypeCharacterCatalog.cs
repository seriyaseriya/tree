using System.Collections.Generic;

namespace WoodClicker.Domain.Characters
{
    public static class PrototypeCharacterCatalog
    {
        // Temporary definitions shared by the prototype gacha and collection UI.
        private static readonly CharacterDefinition[] Definitions = {
            new CharacterDefinition(
                "character_woodcutter_01", "仮木こり・一", CharacterRarity.Common),
            new CharacterDefinition(
                "character_woodcutter_02", "仮木こり・二", CharacterRarity.Common),
            new CharacterDefinition(
                "character_woodcutter_03", "仮木こり・三", CharacterRarity.Rare),
            new CharacterDefinition(
                "character_woodcutter_04", "仮木こり・四", CharacterRarity.Rare),
            new CharacterDefinition(
                "character_woodcutter_05", "仮木こり・五", CharacterRarity.SuperRare)
        };

        public static IReadOnlyList<CharacterDefinition> All => Definitions;

        public static bool TryGet(
            string characterId,
            out CharacterDefinition definition)
        {
            foreach (CharacterDefinition candidate in Definitions)
            {
                if (candidate.CharacterId == characterId)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }
    }
}
