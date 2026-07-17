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
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                throw new ArgumentException(
                    "Character ID must not be empty.",
                    nameof(characterId));
            }

            _characterId = characterId;
            _ownedCount = 1;
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
