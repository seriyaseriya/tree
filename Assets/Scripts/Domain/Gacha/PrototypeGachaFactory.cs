using WoodClicker.Domain.Characters;

namespace WoodClicker.Domain.Gacha
{
    public static class PrototypeGachaFactory
    {
        // Temporary data used only to verify the first gacha foundation.
        public static GachaDefinition Create()
        {
            var characters = PrototypeCharacterCatalog.All;
            return new GachaDefinition(
                "gacha_prototype_01",
                "仮ガチャ",
                100d,
                new GachaPoolEntry(
                    characters[0],
                    35d),
                new GachaPoolEntry(
                    characters[1],
                    35d),
                new GachaPoolEntry(
                    characters[2],
                    12.5d),
                new GachaPoolEntry(
                    characters[3],
                    12.5d),
                new GachaPoolEntry(
                    characters[4],
                    5d));
        }
    }
}
