using System;
using WoodClicker.Domain.Characters;

namespace WoodClicker.Domain.Gacha
{
    public sealed class GachaPoolEntry
    {
        public CharacterDefinition Character { get; }
        public double WeightPercent { get; }

        public GachaPoolEntry(
            CharacterDefinition character,
            double weightPercent)
        {
            Character = character ??
                throw new ArgumentNullException(nameof(character));
            if (weightPercent <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(weightPercent));
            }

            WeightPercent = weightPercent;
        }
    }
}
