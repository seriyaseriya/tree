using System;
using NUnit.Framework;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class PlayerGameStateTests
    {
        [Test]
        public void ApplyManualChop_UpdatesAllManualChopState()
        {
            var gameState = new PlayerGameState();

            gameState.ApplyManualChop(1d);

            Assert.That(gameState.OwnedLogs, Is.EqualTo(1d));
            Assert.That(gameState.TotalLogsEarned, Is.EqualTo(1d));
            Assert.That(gameState.TotalManualChops, Is.EqualTo(1L));
        }

        [Test]
        public void ApplyManualChop_WithNegativeLogs_Throws()
        {
            var gameState = new PlayerGameState();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => gameState.ApplyManualChop(-1d));

            Assert.That(gameState.OwnedLogs, Is.EqualTo(0d));
            Assert.That(gameState.TotalLogsEarned, Is.EqualTo(0d));
            Assert.That(gameState.TotalManualChops, Is.EqualTo(0L));
        }

        [Test]
        public void ApplySale_RemovesLogsAndAddsMoney()
        {
            var gameState = new PlayerGameState();
            gameState.ApplyManualChop(3d);

            gameState.ApplySale(3d, 3d);

            Assert.That(gameState.OwnedLogs, Is.EqualTo(0d));
            Assert.That(gameState.OwnedMoney, Is.EqualTo(3d));
            Assert.That(gameState.TotalMoneyEarned, Is.EqualTo(3d));
        }
    }
}
