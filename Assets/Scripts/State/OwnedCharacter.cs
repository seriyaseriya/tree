using System;
using UnityEngine;

namespace WoodClicker.State
{
    [Serializable]
    public sealed class OwnedCharacter
    {
        [SerializeField] private string _characterId;
        [SerializeField] private int _ownedCount;
        [SerializeField] private long _firstObtainedOrder;

        public string CharacterId => _characterId;
        public int OwnedCount => _ownedCount;
        public long FirstObtainedOrder => _firstObtainedOrder;

        public OwnedCharacter(string characterId, long firstObtainedOrder)
            : this(characterId, 1, firstObtainedOrder)
        {
        }

        public OwnedCharacter(
            string characterId,
            int ownedCount,
            long firstObtainedOrder)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                throw new ArgumentException(
                    "Character ID must not be empty.",
                    nameof(characterId));
            }

            if (ownedCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ownedCount));
            }

            if (firstObtainedOrder <= 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(firstObtainedOrder));
            }

            _characterId = characterId;
            _ownedCount = ownedCount;
            _firstObtainedOrder = firstObtainedOrder;
        }

        public void IncrementOwnedCount()
        {
            checked
            {
                _ownedCount++;
            }
        }
    }
}
