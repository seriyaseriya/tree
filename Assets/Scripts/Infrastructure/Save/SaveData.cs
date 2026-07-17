using System;

namespace WoodClicker.Infrastructure.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public double ownedLogs;
        public double ownedMoney;
        public double totalLogsEarned;
        public double totalMoneyEarned;
        public long totalManualChops;
        public OwnedCharacterSaveData[] ownedCharacters =
            Array.Empty<OwnedCharacterSaveData>();
        public double treeMaxHealth;
        public double treeCurrentHealth;
    }

    [Serializable]
    public sealed class OwnedCharacterSaveData
    {
        public string characterId;
        public int ownedCount;
        public long firstObtainedOrder;
    }
}
