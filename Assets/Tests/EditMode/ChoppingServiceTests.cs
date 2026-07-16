using NUnit.Framework;
using WoodClicker.Domain.Chopping;

namespace WoodClicker.Tests.EditMode
{
    public sealed class ChoppingServiceTests
    {
        private ChoppingService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new ChoppingService(new ChoppingConfig(1d, 1f));
        }

        [Test]
        public void TryChop_WhenAvailable_ReturnsSuccessfulResult()
        {
            ChoppingResult result = _service.TryChop(false, false);

            Assert.That(result.FailureReason, Is.EqualTo(ChoppingFailureReason.None));
            Assert.That(result.EarnedLogs, Is.EqualTo(1d));
            Assert.That(result.TreeDamage, Is.EqualTo(result.EarnedLogs));
            Assert.That(result.CooldownSeconds, Is.EqualTo(1f));
        }

        [Test]
        public void TryChop_DuringCooldown_ReturnsCooldownFailure()
        {
            ChoppingResult result = _service.TryChop(true, false);

            Assert.That(
                result.FailureReason,
                Is.EqualTo(ChoppingFailureReason.CooldownActive));
            Assert.That(result.EarnedLogs, Is.EqualTo(0d));
            Assert.That(result.TreeDamage, Is.EqualTo(0d));
            Assert.That(result.CooldownSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void TryChop_WhenTreeIsFelled_ReturnsTreeAlreadyFelledFailure()
        {
            ChoppingResult result = _service.TryChop(false, true);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.FailureReason,
                Is.EqualTo(ChoppingFailureReason.TreeAlreadyFelled));
            Assert.That(result.EarnedLogs, Is.EqualTo(0d));
            Assert.That(result.TreeDamage, Is.EqualTo(0d));
            Assert.That(result.CooldownSeconds, Is.EqualTo(0f));
        }
    }
}
