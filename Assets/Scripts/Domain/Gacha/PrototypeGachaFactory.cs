using WoodClicker.Domain.Characters;

namespace WoodClicker.Domain.Gacha
{
    public static class PrototypeGachaFactory
    {
        // Temporary data used only to verify the first gacha foundation.
        public static GachaDefinition Create()
        {
            return new GachaDefinition(
                "gacha_prototype_01",
                "仮ガチャ",
                100d,
                new GachaPoolEntry(
                    new CharacterDefinition(
                        "character_woodcutter_01", "仮木こり・一", CharacterRarity.Common),
                    35d),
                new GachaPoolEntry(
                    new CharacterDefinition(
                        "character_woodcutter_02", "仮木こり・二", CharacterRarity.Common),
                    35d),
                new GachaPoolEntry(
                    new CharacterDefinition(
                        "character_woodcutter_03", "仮木こり・三", CharacterRarity.Rare),
                    12.5d),
                new GachaPoolEntry(
                    new CharacterDefinition(
                        "character_woodcutter_04", "仮木こり・四", CharacterRarity.Rare),
                    12.5d),
                new GachaPoolEntry(
                    new CharacterDefinition(
                        "character_woodcutter_05", "仮木こり・五", CharacterRarity.SuperRare),
                    5d));
        }
    }
}
