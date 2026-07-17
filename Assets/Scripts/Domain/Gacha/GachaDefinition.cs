using System;
using System.Collections.Generic;

namespace WoodClicker.Domain.Gacha
{
    public sealed class GachaDefinition
    {
        private const double ExpectedTotalWeight = 100d;
        private const double WeightTolerance = 0.0001d;

        private readonly GachaPoolEntry[] _poolEntries;

        public string GachaId { get; }
        public string DisplayName { get; }
        public double Cost { get; }
        public IReadOnlyList<GachaPoolEntry> PoolEntries => _poolEntries;

        public GachaDefinition(
            string gachaId,
            string displayName,
            double cost,
            params GachaPoolEntry[] poolEntries)
        {
            if (string.IsNullOrWhiteSpace(gachaId))
            {
                throw new ArgumentException("Gacha ID must not be empty.", nameof(gachaId));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Display name must not be empty.",
                    nameof(displayName));
            }

            if (cost <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            if (poolEntries == null || poolEntries.Length == 0)
            {
                throw new ArgumentException(
                    "Gacha pool must contain at least one entry.",
                    nameof(poolEntries));
            }

            double totalWeight = 0d;
            foreach (GachaPoolEntry entry in poolEntries)
            {
                if (entry == null)
                {
                    throw new ArgumentException(
                        "Gacha pool must not contain null entries.",
                        nameof(poolEntries));
                }

                totalWeight += entry.WeightPercent;
            }

            if (Math.Abs(totalWeight - ExpectedTotalWeight) > WeightTolerance)
            {
                throw new ArgumentException(
                    $"Gacha weights must total {ExpectedTotalWeight} percent.",
                    nameof(poolEntries));
            }

            GachaId = gachaId;
            DisplayName = displayName;
            Cost = cost;
            _poolEntries = (GachaPoolEntry[])poolEntries.Clone();
        }
    }
}
