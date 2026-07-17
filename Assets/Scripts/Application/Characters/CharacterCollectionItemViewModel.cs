using WoodClicker.Domain.Characters;

namespace WoodClicker.Application.Characters
{
    public readonly struct CharacterCollectionItemViewModel
    {
        public string CharacterId { get; }
        public string DisplayName { get; }
        public CharacterRarity Rarity { get; }
        public int OwnedCount { get; }
        public long AcquisitionOrder { get; }

        public CharacterCollectionItemViewModel(
            string characterId,
            string displayName,
            CharacterRarity rarity,
            int ownedCount,
            long acquisitionOrder)
        {
            CharacterId = characterId;
            DisplayName = displayName;
            Rarity = rarity;
            OwnedCount = ownedCount;
            AcquisitionOrder = acquisitionOrder;
        }
    }
}
