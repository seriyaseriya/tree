using System;
using NUnit.Framework;
using WoodClicker.Domain.Characters;
using WoodClicker.Domain.Gacha;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class GachaServiceTests
    {
        private GachaService _service;
        private GachaDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _service = new GachaService();
            _definition = PrototypeGachaFactory.Create();
        }

        [Test]
        public void TryDraw_WithEnoughMoney_Succeeds()
        {
            GachaResult result = Draw(100d, 0d);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Cost, Is.EqualTo(100d));
        }

        [Test]
        public void SuccessfulDraw_SpendsCorrectAmount()
        {
            PlayerGameState state = CreateStateWithMoney(150d);
            GachaResult result = _service.TryDraw(
                _definition,
                state.OwnedMoney,
                state.GetOwnedCharacterIds(),
                new FixedRandomProvider(0d));

            Assert.That(state.TrySpendMoney(result.Cost), Is.True);
            Assert.That(state.OwnedMoney, Is.EqualTo(50d));
        }

        [Test]
        public void TryDraw_WithInsufficientMoney_Fails()
        {
            GachaResult result = Draw(99d, 0d);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.FailureReason,
                Is.EqualTo(GachaFailureReason.InsufficientMoney));
        }

        [Test]
        public void InsufficientMoney_DoesNotChangeState()
        {
            PlayerGameState state = CreateStateWithMoney(99d);
            double moneyBefore = state.OwnedMoney;
            int charactersBefore = state.OwnedCharacters.Count;

            GachaResult result = _service.TryDraw(
                _definition,
                state.OwnedMoney,
                state.GetOwnedCharacterIds(),
                new FixedRandomProvider(0d));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(state.OwnedMoney, Is.EqualTo(moneyBefore));
            Assert.That(state.OwnedCharacters.Count, Is.EqualTo(charactersBefore));
        }

        [Test]
        public void TryDraw_CanSelectCommon()
        {
            Assert.That(Draw(100d, 0.10d).Rarity, Is.EqualTo(CharacterRarity.Common));
        }

        [Test]
        public void TryDraw_CanSelectRare()
        {
            Assert.That(Draw(100d, 0.71d).Rarity, Is.EqualTo(CharacterRarity.Rare));
        }

        [Test]
        public void TryDraw_CanSelectSuperRare()
        {
            Assert.That(
                Draw(100d, 0.96d).Rarity,
                Is.EqualTo(CharacterRarity.SuperRare));
        }

        [Test]
        public void AcquireCharacter_AddsNewCharacter()
        {
            var state = new PlayerGameState();

            int count = state.AcquireCharacter("character_woodcutter_01");

            Assert.That(count, Is.EqualTo(1));
            Assert.That(state.OwnedCharacters.Count, Is.EqualTo(1));
            Assert.That(state.OwnedCharacters[0].FirstObtainedOrder, Is.EqualTo(1L));
        }

        [Test]
        public void AcquireCharacter_DuplicateIncrementsOwnedCount()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");

            int count = state.AcquireCharacter("character_woodcutter_01");

            Assert.That(count, Is.EqualTo(2));
            Assert.That(state.OwnedCharacters.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryDraw_ReportsNewAndDuplicateCorrectly()
        {
            GachaResult first = Draw(100d, 0d);
            GachaResult duplicate = _service.TryDraw(
                _definition,
                100d,
                new[] { first.CharacterId },
                new FixedRandomProvider(0d));

            Assert.That(first.IsNew, Is.True);
            Assert.That(duplicate.IsNew, Is.False);
        }

        [Test]
        public void Definition_WithInvalidTotalWeight_Throws()
        {
            var character = new CharacterDefinition(
                "test", "仮", CharacterRarity.Common);

            Assert.Throws<ArgumentException>(() => new GachaDefinition(
                "invalid", "不正", 100d,
                new GachaPoolEntry(character, 99d)));
        }

        [Test]
        public void Definition_WithEmptyPool_Throws()
        {
            Assert.Throws<ArgumentException>(() => new GachaDefinition(
                "empty", "空", 100d, Array.Empty<GachaPoolEntry>()));
        }

        private GachaResult Draw(double money, double randomValue)
        {
            return _service.TryDraw(
                _definition,
                money,
                Array.Empty<string>(),
                new FixedRandomProvider(randomValue));
        }

        private static PlayerGameState CreateStateWithMoney(double amount)
        {
            var state = new PlayerGameState();
            state.ApplyManualChop(amount);
            state.ApplySale(amount, amount);
            return state;
        }

        private sealed class FixedRandomProvider : IRandomProvider
        {
            private readonly double _value;

            public FixedRandomProvider(double value)
            {
                _value = value;
            }

            public double NextDouble()
            {
                return _value;
            }
        }
    }
}
