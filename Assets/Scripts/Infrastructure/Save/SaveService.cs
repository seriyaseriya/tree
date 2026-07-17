using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WoodClicker.State;

namespace WoodClicker.Infrastructure.Save
{
    public sealed class SaveService
    {
        public const string DefaultFileName = "wood-clicker-save.json";
        private const double DefaultTreeMaxHealth = 100d;

        private readonly string _filePath;

        public string FilePath => _filePath;

        public SaveService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Save file path must not be empty.",
                    nameof(filePath));
            }

            _filePath = filePath;
        }

        public LoadedGameState LoadOrCreate()
        {
            if (!File.Exists(_filePath))
            {
                return CreateNewState();
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.version != 1)
                {
                    throw new InvalidDataException("Unsupported save data.");
                }

                return Restore(data);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Failed to load save data. A new game state will be used. {exception.Message}");
                return CreateNewState();
            }
        }

        public void Save(PlayerGameState player, TreeState tree)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            SaveData data = Capture(player, tree);
            File.WriteAllText(_filePath, JsonUtility.ToJson(data, true));
        }

        private static SaveData Capture(PlayerGameState player, TreeState tree)
        {
            IReadOnlyList<OwnedCharacter> owned = player.OwnedCharacters;
            var characters = new OwnedCharacterSaveData[owned.Count];
            for (int i = 0; i < owned.Count; i++)
            {
                OwnedCharacter character = owned[i];
                characters[i] = new OwnedCharacterSaveData
                {
                    characterId = character.CharacterId,
                    ownedCount = character.OwnedCount,
                    firstObtainedOrder = character.FirstObtainedOrder
                };
            }

            return new SaveData
            {
                ownedLogs = player.OwnedLogs,
                ownedMoney = player.OwnedMoney,
                totalLogsEarned = player.TotalLogsEarned,
                totalMoneyEarned = player.TotalMoneyEarned,
                totalManualChops = player.TotalManualChops,
                ownedCharacters = characters,
                treeMaxHealth = tree.MaxHealth,
                treeCurrentHealth = tree.CurrentHealth
            };
        }

        private static LoadedGameState Restore(SaveData data)
        {
            OwnedCharacterSaveData[] savedCharacters =
                data.ownedCharacters ?? Array.Empty<OwnedCharacterSaveData>();
            var characters = new List<OwnedCharacter>(savedCharacters.Length);
            foreach (OwnedCharacterSaveData saved in savedCharacters)
            {
                if (saved == null)
                {
                    throw new InvalidDataException("Invalid owned character.");
                }

                characters.Add(new OwnedCharacter(
                    saved.characterId,
                    saved.ownedCount,
                    saved.firstObtainedOrder));
            }

            var player = new PlayerGameState(
                data.ownedLogs,
                data.ownedMoney,
                data.totalLogsEarned,
                data.totalMoneyEarned,
                data.totalManualChops,
                characters);
            var tree = new TreeState(
                data.treeMaxHealth,
                data.treeCurrentHealth);
            return new LoadedGameState(player, tree);
        }

        private static LoadedGameState CreateNewState()
        {
            return new LoadedGameState(
                new PlayerGameState(),
                new TreeState(DefaultTreeMaxHealth));
        }
    }
}
