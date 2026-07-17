using WoodClicker.Domain.Characters;

namespace WoodClicker.Domain.Gacha
{
    public readonly struct GachaResult
    {
        public string CharacterId { get; }
        public string CharacterName { get; }
        public CharacterRarity Rarity { get; }
        public bool IsNew { get; }
        public double Cost { get; }
        public GachaFailureReason FailureReason { get; }
        public bool Succeeded => FailureReason == GachaFailureReason.None;

        private GachaResult(
            string characterId,
            string characterName,
            CharacterRarity rarity,
            bool isNew,
            double cost,
            GachaFailureReason failureReason)
        {
            CharacterId = characterId;
            CharacterName = characterName;
            Rarity = rarity;
            IsNew = isNew;
            Cost = cost;
            FailureReason = failureReason;
        }

        public static GachaResult Success(
            CharacterDefinition character,
            bool isNew,
            double cost)
        {
            return new GachaResult(
                character.CharacterId,
                character.DisplayName,
                character.Rarity,
                isNew,
                cost,
                GachaFailureReason.None);
        }

        public static GachaResult Failure(GachaFailureReason failureReason)
        {
            return new GachaResult(
                string.Empty,
                string.Empty,
                CharacterRarity.Common,
                false,
                0d,
                failureReason);
        }
    }
}
