using System;
using NUnit.Framework;
using WoodClicker.Domain.Selling;

namespace WoodClicker.Tests.EditMode
{
    public sealed class SellingServiceTests
    {
        private SellingService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new SellingService(1d);
        }

        [Test]
        public void SellAll_WithLogs_ReturnsAllLogsAtOneMoneyEach()
        {
            SellingResult result = _service.SellAll(3d);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.SoldLogs, Is.EqualTo(3d));
            Assert.That(result.EarnedMoney, Is.EqualTo(3d));
        }

        [Test]
        public void SellAll_WithNoLogs_ReturnsNoLogsFailure()
        {
            SellingResult result = _service.SellAll(0d);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(SellingFailureReason.NoLogs));
            Assert.That(result.SoldLogs, Is.EqualTo(0d));
            Assert.That(result.EarnedMoney, Is.EqualTo(0d));
        }

        [Test]
        public void SellAll_WithNegativeLogs_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.SellAll(-1d));
        }
    }
}
