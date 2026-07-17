using System;

namespace WoodClicker.Domain.Characters
{
    public sealed class CharacterDefinition
    {
        public string CharacterId { get; }
        public string DisplayName { get; }
        public CharacterRarity Rarity { get; }

        public CharacterDefinition(
            string characterId,
            string displayName,
            CharacterRarity rarity)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                throw new ArgumentException(
                    "Character ID must not be empty.",
                    nameof(characterId));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Display name must not be empty.",
                    nameof(displayName));
            }

            CharacterId = characterId;
            DisplayName = displayName;
            Rarity = rarity;
        }
    }
}
