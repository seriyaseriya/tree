using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WoodClicker.Infrastructure.Save;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class SaveServiceTests
    {
        private string _testDirectory;
        private string _savePath;
        private SaveService _service;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(
                Path.GetTempPath(),
                "WoodClickerSaveTests",
                Guid.NewGuid().ToString("N"));
            _savePath = Path.Combine(_testDirectory, "save.json");
            _service = new SaveService(_savePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void Save_WritesJsonFile()
        {
            _service.Save(new PlayerGameState(), new TreeState(100d));

            Assert.That(File.Exists(_savePath), Is.True);
            Assert.That(File.ReadAllText(_savePath), Does.Contain("ownedLogs"));
        }

        [Test]
        public void LoadOrCreate_WithSavedData_RestoresValues()
        {
            PlayerGameState player = CreatePopulatedPlayer();
            var tree = new TreeState(100d);
            tree.ApplyDamage(7d);
            _service.Save(player, tree);

            LoadedGameState loaded = _service.LoadOrCreate();

            Assert.That(loaded.Player.OwnedLogs, Is.EqualTo(2d));
            Assert.That(loaded.Player.OwnedMoney, Is.EqualTo(1d));
            Assert.That(loaded.Tree.CurrentHealth, Is.EqualTo(93d));
            Assert.That(
                loaded.Player.GetOwnedCharacterCount("character_test"),
                Is.EqualTo(2));
        }

        [Test]
        public void LoadOrCreate_WithoutFile_CreatesNewState()
        {
            LoadedGameState loaded = _service.LoadOrCreate();

            Assert.That(loaded.Player.OwnedLogs, Is.Zero);
            Assert.That(loaded.Player.OwnedMoney, Is.Zero);
            Assert.That(loaded.Player.OwnedCharacters, Is.Empty);
            Assert.That(loaded.Tree.MaxHealth, Is.EqualTo(100d));
            Assert.That(loaded.Tree.CurrentHealth, Is.EqualTo(100d));
        }

        [Test]
        public void LoadOrCreate_WithBrokenJson_DoesNotThrowAndCreatesNewState()
        {
            Directory.CreateDirectory(_testDirectory);
            File.WriteAllText(_savePath, "{ broken json");

            LoadedGameState loaded = null;
            LogAssert.Expect(
                LogType.Warning,
                new System.Text.RegularExpressions.Regex(
                    "Failed to load save data.*"));
            Assert.DoesNotThrow(() => loaded = _service.LoadOrCreate());
            Assert.That(loaded.Player.OwnedLogs, Is.Zero);
            Assert.That(loaded.Tree.CurrentHealth, Is.EqualTo(100d));
        }

        [Test]
        public void SaveThenLoad_PreservesCompletePlayerAndTreeState()
        {
            PlayerGameState originalPlayer = CreatePopulatedPlayer();
            var originalTree = new TreeState(100d);
            originalTree.ApplyDamage(25d);
            _service.Save(originalPlayer, originalTree);

            LoadedGameState loaded = _service.LoadOrCreate();

            Assert.That(loaded.Player.OwnedLogs, Is.EqualTo(originalPlayer.OwnedLogs));
            Assert.That(loaded.Player.OwnedMoney, Is.EqualTo(originalPlayer.OwnedMoney));
            Assert.That(loaded.Player.TotalLogsEarned, Is.EqualTo(originalPlayer.TotalLogsEarned));
            Assert.That(loaded.Player.TotalMoneyEarned, Is.EqualTo(originalPlayer.TotalMoneyEarned));
            Assert.That(loaded.Player.TotalManualChops, Is.EqualTo(originalPlayer.TotalManualChops));
            Assert.That(loaded.Player.OwnedCharacters.Count, Is.EqualTo(1));
            Assert.That(loaded.Player.OwnedCharacters[0].OwnedCount, Is.EqualTo(2));
            Assert.That(loaded.Player.OwnedCharacters[0].FirstObtainedOrder, Is.EqualTo(1L));
            Assert.That(loaded.Tree.MaxHealth, Is.EqualTo(originalTree.MaxHealth));
            Assert.That(loaded.Tree.CurrentHealth, Is.EqualTo(originalTree.CurrentHealth));
        }

        private static PlayerGameState CreatePopulatedPlayer()
        {
            var player = new PlayerGameState();
            player.ApplyManualChop(3d);
            player.ApplySale(1d, 1d);
            player.AcquireCharacter("character_test");
            player.AcquireCharacter("character_test");
            return player;
        }
    }
}
