using System;
using System.Collections.Generic;
using WoodClicker.Domain.Characters;
using WoodClicker.State;

namespace WoodClicker.Application.Characters
{
    public sealed class CharacterCollectionBuilder
    {
        private const string UnknownCharacterName = "不明なキャラクター";

        public CharacterCollectionViewModel Build(
            PlayerGameState gameState,
            IReadOnlyList<CharacterDefinition> definitions)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            var items = new List<CharacterCollectionItemViewModel>();
            foreach (OwnedCharacter owned in gameState.OwnedCharacters)
            {
                CharacterDefinition definition = FindDefinition(
                    definitions,
                    owned.CharacterId);
                items.Add(new CharacterCollectionItemViewModel(
                    owned.CharacterId,
                    definition?.DisplayName ?? UnknownCharacterName,
                    definition?.Rarity ?? CharacterRarity.Common,
                    owned.OwnedCount,
                    owned.FirstObtainedOrder));
            }

            items.Sort((left, right) =>
                left.AcquisitionOrder.CompareTo(right.AcquisitionOrder));
            return new CharacterCollectionViewModel(items.ToArray());
        }

        private static CharacterDefinition FindDefinition(
            IReadOnlyList<CharacterDefinition> definitions,
            string characterId)
        {
            foreach (CharacterDefinition definition in definitions)
            {
                if (definition != null && definition.CharacterId == characterId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
