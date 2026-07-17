using NUnit.Framework;
using WoodClicker.Domain.Chopping;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class ChoppingIntegrationTests
    {
        [Test]
        public void SuccessfulChop_UpdatesPlayerAndTreeState()
        {
            var playerState = new PlayerGameState();
            var treeState = new TreeState(100d);
            var service = new ChoppingService(new ChoppingConfig(1d, 1f));

            ChoppingResult result = service.TryChop(false, treeState.IsFelled);
            playerState.ApplyManualChop(result.EarnedLogs);
            treeState.ApplyDamage(result.TreeDamage);

            Assert.That(playerState.OwnedLogs, Is.EqualTo(1d));
            Assert.That(treeState.CurrentHealth, Is.EqualTo(99d));
        }

        [Test]
        public void FelledTree_DoesNotUpdatePlayerStateOrStartCooldown()
        {
            var playerState = new PlayerGameState();
            var treeState = new TreeState(100d);
            var service = new ChoppingService(new ChoppingConfig(1d, 1f));

            for (int i = 0; i < 100; i++)
            {
                ChoppingResult chop = service.TryChop(false, treeState.IsFelled);
                playerState.ApplyManualChop(chop.EarnedLogs);
                treeState.ApplyDamage(chop.TreeDamage);
            }

            ChoppingResult rejected = service.TryChop(false, treeState.IsFelled);
            if (rejected.Succeeded)
            {
                playerState.ApplyManualChop(rejected.EarnedLogs);
                treeState.ApplyDamage(rejected.TreeDamage);
            }

            Assert.That(treeState.CurrentHealth, Is.EqualTo(0d));
            Assert.That(playerState.OwnedLogs, Is.EqualTo(100d));
            Assert.That(playerState.TotalManualChops, Is.EqualTo(100L));
            Assert.That(
                rejected.FailureReason,
                Is.EqualTo(ChoppingFailureReason.TreeAlreadyFelled));
            Assert.That(rejected.CooldownSeconds, Is.EqualTo(0f));
        }
    }
}
