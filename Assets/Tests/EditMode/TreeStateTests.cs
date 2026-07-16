using System;
using NUnit.Framework;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class TreeStateTests
    {
        [Test]
        public void Constructor_InitializesMaximumAndCurrentHealth()
        {
            var treeState = new TreeState(100d);

            Assert.That(treeState.MaxHealth, Is.EqualTo(100d));
            Assert.That(treeState.CurrentHealth, Is.EqualTo(100d));
            Assert.That(treeState.IsFelled, Is.False);
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHealth()
        {
            var treeState = new TreeState(100d);

            treeState.ApplyDamage(1d);

            Assert.That(treeState.CurrentHealth, Is.EqualTo(99d));
        }

        [Test]
        public void ApplyDamage_WithExcessDamage_StopsAtZeroAndFellsTree()
        {
            var treeState = new TreeState(100d);

            treeState.ApplyDamage(101d);

            Assert.That(treeState.CurrentHealth, Is.EqualTo(0d));
            Assert.That(treeState.IsFelled, Is.True);
        }

        [Test]
        public void ApplyDamage_WithNegativeDamage_Throws()
        {
            var treeState = new TreeState(100d);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => treeState.ApplyDamage(-1d));
            Assert.That(treeState.CurrentHealth, Is.EqualTo(100d));
        }

        [TestCase(0d)]
        [TestCase(-1d)]
        public void Constructor_WithNonPositiveMaximumHealth_Throws(
            double maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new TreeState(maxHealth));
        }
    }
}
