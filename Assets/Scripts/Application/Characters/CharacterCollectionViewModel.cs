using System.Collections.Generic;

namespace WoodClicker.Application.Characters
{
    public sealed class CharacterCollectionViewModel
    {
        private readonly CharacterCollectionItemViewModel[] _items;

        public IReadOnlyList<CharacterCollectionItemViewModel> Items => _items;
        public int OwnedTypeCount => _items.Length;
        public bool IsEmpty => _items.Length == 0;

        public CharacterCollectionViewModel(
            CharacterCollectionItemViewModel[] items)
        {
            _items = items ?? System.Array.Empty<CharacterCollectionItemViewModel>();
        }
    }
}
