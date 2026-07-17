using NUnit.Framework;
using WoodClicker.Application.Characters;
using WoodClicker.Domain.Characters;
using WoodClicker.State;

namespace WoodClicker.Tests.EditMode
{
    public sealed class CharacterCollectionBuilderTests
    {
        private CharacterCollectionBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new CharacterCollectionBuilder();
        }

        [Test]
        public void Build_WithNoOwnedCharacters_ReturnsEmptyCollection()
        {
            CharacterCollectionViewModel model = Build(new PlayerGameState());

            Assert.That(model.IsEmpty, Is.True);
            Assert.That(model.Items, Is.Empty);
        }

        [Test]
        public void Build_WithOneCharacter_CreatesOneItem()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items.Count, Is.EqualTo(1));
            Assert.That(
                model.Items[0].CharacterId,
                Is.EqualTo("character_woodcutter_01"));
        }

        [Test]
        public void Build_SortsByFirstAcquisitionOrder()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_03");
            state.AcquireCharacter("character_woodcutter_01");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items[0].AcquisitionOrder, Is.EqualTo(1L));
            Assert.That(model.Items[0].CharacterId, Is.EqualTo("character_woodcutter_03"));
            Assert.That(model.Items[1].AcquisitionOrder, Is.EqualTo(2L));
        }

        [Test]
        public void Build_DuplicateCharacterProducesSingleItem()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");
            state.AcquireCharacter("character_woodcutter_01");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void Build_ReflectsDuplicateOwnedCount()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");
            state.AcquireCharacter("character_woodcutter_01");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items[0].OwnedCount, Is.EqualTo(2));
        }

        [Test]
        public void Build_ResolvesDisplayNameFromCharacterId()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_02");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items[0].DisplayName, Is.EqualTo("仮木こり・二"));
        }

        [Test]
        public void Build_ResolvesRarityFromCharacterId()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_05");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items[0].Rarity, Is.EqualTo(CharacterRarity.SuperRare));
        }

        [Test]
        public void Build_WithUnknownCharacter_UsesFallback()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("unknown_character_id");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.Items.Count, Is.EqualTo(1));
            Assert.That(model.Items[0].CharacterId, Is.EqualTo("unknown_character_id"));
            Assert.That(model.Items[0].DisplayName, Is.EqualTo("不明なキャラクター"));
            Assert.That(model.Items[0].Rarity, Is.EqualTo(CharacterRarity.Common));
        }

        [Test]
        public void Build_ReturnsCorrectOwnedTypeCount()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");
            state.AcquireCharacter("character_woodcutter_02");
            state.AcquireCharacter("character_woodcutter_02");

            CharacterCollectionViewModel model = Build(state);

            Assert.That(model.OwnedTypeCount, Is.EqualTo(2));
        }

        [Test]
        public void Build_DoesNotModifyPlayerGameState()
        {
            var state = new PlayerGameState();
            state.AcquireCharacter("character_woodcutter_01");
            int typeCountBefore = state.OwnedCharacters.Count;
            int ownedCountBefore = state.OwnedCharacters[0].OwnedCount;

            Build(state);

            Assert.That(state.OwnedCharacters.Count, Is.EqualTo(typeCountBefore));
            Assert.That(state.OwnedCharacters[0].OwnedCount, Is.EqualTo(ownedCountBefore));
        }

        private CharacterCollectionViewModel Build(PlayerGameState state)
        {
            return _builder.Build(state, PrototypeCharacterCatalog.All);
        }
    }
}
