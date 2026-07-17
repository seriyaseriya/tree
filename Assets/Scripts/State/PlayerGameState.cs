using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoodClicker.State
{
    [Serializable]
    public sealed class PlayerGameState
    {
        [SerializeField] private List<OwnedCharacter> _ownedCharacters = new();
        [SerializeField] private long _nextCharacterObtainedOrder = 1L;

        public double OwnedLogs { get; private set; }
        public double OwnedMoney { get; private set; }
        public double TotalLogsEarned { get; private set; }
        public double TotalMoneyEarned { get; private set; }
        public long TotalManualChops { get; private set; }
        public IReadOnlyList<OwnedCharacter> OwnedCharacters =>
            EnsureOwnedCharacters();

        public PlayerGameState()
        {
        }

        public PlayerGameState(
            double ownedLogs,
            double ownedMoney,
            double totalLogsEarned,
            double totalMoneyEarned,
            long totalManualChops,
            IEnumerable<OwnedCharacter> ownedCharacters)
        {
            if (ownedLogs < 0d || ownedMoney < 0d ||
                totalLogsEarned < 0d || totalMoneyEarned < 0d ||
                totalManualChops < 0L)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ownedLogs),
                    "Saved player values must not be negative.");
            }

            OwnedLogs = ownedLogs;
            OwnedMoney = ownedMoney;
            TotalLogsEarned = totalLogsEarned;
            TotalMoneyEarned = totalMoneyEarned;
            TotalManualChops = totalManualChops;
            _ownedCharacters = ownedCharacters == null
                ? new List<OwnedCharacter>()
                : new List<OwnedCharacter>(ownedCharacters);

            long highestOrder = 0L;
            foreach (OwnedCharacter character in _ownedCharacters)
            {
                if (character == null)
                {
                    throw new ArgumentException(
                        "Owned characters must not contain null.",
                        nameof(ownedCharacters));
                }

                highestOrder = Math.Max(
                    highestOrder,
                    character.FirstObtainedOrder);
            }

            _nextCharacterObtainedOrder = highestOrder + 1L;
        }

        public void ApplyManualChop(double earnedLogs)
        {
            if (earnedLogs < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(earnedLogs));
            }

            OwnedLogs += earnedLogs;
            TotalLogsEarned += earnedLogs;
            TotalManualChops++;
        }

        public void ApplySale(double soldLogs, double earnedMoney)
        {
            if (soldLogs <= 0d || soldLogs > OwnedLogs)
            {
                throw new ArgumentOutOfRangeException(nameof(soldLogs));
            }

            if (earnedMoney < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(earnedMoney));
            }

            OwnedLogs -= soldLogs;
            OwnedMoney += earnedMoney;
            TotalMoneyEarned += earnedMoney;
        }

        public bool TrySpendMoney(double amount)
        {
            if (amount < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (OwnedMoney < amount)
            {
                return false;
            }

            OwnedMoney -= amount;
            return true;
        }

        public int AcquireCharacter(string characterId)
        {
            List<OwnedCharacter> characters = EnsureOwnedCharacters();
            OwnedCharacter existing = FindOwnedCharacter(characterId);
            if (existing != null)
            {
                existing.IncrementOwnedCount();
                return existing.OwnedCount;
            }

            var ownedCharacter = new OwnedCharacter(
                characterId,
                _nextCharacterObtainedOrder++);
            characters.Add(ownedCharacter);
            return ownedCharacter.OwnedCount;
        }

        public int GetOwnedCharacterCount(string characterId)
        {
            OwnedCharacter character = FindOwnedCharacter(characterId);
            return character?.OwnedCount ?? 0;
        }

        public string[] GetOwnedCharacterIds()
        {
            List<OwnedCharacter> characters = EnsureOwnedCharacters();
            var ids = new string[characters.Count];
            for (int i = 0; i < characters.Count; i++)
            {
                ids[i] = characters[i].CharacterId;
            }

            return ids;
        }

        private OwnedCharacter FindOwnedCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                throw new ArgumentException(
                    "Character ID must not be empty.",
                    nameof(characterId));
            }

            foreach (OwnedCharacter character in EnsureOwnedCharacters())
            {
                if (character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }

        private List<OwnedCharacter> EnsureOwnedCharacters()
        {
            _ownedCharacters ??= new List<OwnedCharacter>();
            if (_nextCharacterObtainedOrder <= 0L)
            {
                _nextCharacterObtainedOrder = _ownedCharacters.Count + 1L;
            }

            return _ownedCharacters;
        }
    }
}
