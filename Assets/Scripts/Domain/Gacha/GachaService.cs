using System;
using System.Collections.Generic;

namespace WoodClicker.Domain.Gacha
{
    public sealed class GachaService
    {
        public GachaResult TryDraw(
            GachaDefinition definition,
            double ownedMoney,
            IReadOnlyCollection<string> ownedCharacterIds,
            IRandomProvider randomProvider)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (ownedMoney < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(ownedMoney));
            }

            if (ownedCharacterIds == null)
            {
                throw new ArgumentNullException(nameof(ownedCharacterIds));
            }

            if (randomProvider == null)
            {
                throw new ArgumentNullException(nameof(randomProvider));
            }

            if (ownedMoney < definition.Cost)
            {
                return GachaResult.Failure(GachaFailureReason.InsufficientMoney);
            }

            double randomValue = randomProvider.NextDouble();
            if (randomValue < 0d || randomValue >= 1d)
            {
                throw new InvalidOperationException(
                    "Random provider must return a value from 0 inclusive to 1 exclusive.");
            }

            double targetWeight = randomValue * 100d;
            double cumulativeWeight = 0d;
            GachaPoolEntry selected = null;
            foreach (GachaPoolEntry entry in definition.PoolEntries)
            {
                cumulativeWeight += entry.WeightPercent;
                if (targetWeight < cumulativeWeight)
                {
                    selected = entry;
                    break;
                }
            }

            selected ??= definition.PoolEntries[definition.PoolEntries.Count - 1];
            bool isNew = !ContainsId(
                ownedCharacterIds,
                selected.Character.CharacterId);
            return GachaResult.Success(
                selected.Character,
                isNew,
                definition.Cost);
        }

        private static bool ContainsId(
            IReadOnlyCollection<string> characterIds,
            string targetId)
        {
            foreach (string characterId in characterIds)
            {
                if (characterId == targetId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
