using WoodClicker.State;

namespace WoodClicker.Infrastructure.Save
{
    public sealed class LoadedGameState
    {
        public PlayerGameState Player { get; }
        public TreeState Tree { get; }

        public LoadedGameState(PlayerGameState player, TreeState tree)
        {
            Player = player;
            Tree = tree;
        }
    }
}
