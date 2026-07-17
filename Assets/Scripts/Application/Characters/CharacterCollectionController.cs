using UnityEngine;
using WoodClicker.Domain.Characters;
using WoodClicker.Presentation.Characters;
using WoodClicker.State;

namespace WoodClicker.Application.Characters
{
    public sealed class CharacterCollectionController : MonoBehaviour
    {
        [SerializeField] private CharacterCollectionView _view;

        private readonly CharacterCollectionBuilder _builder =
            new CharacterCollectionBuilder();
        private PlayerGameState _gameState;

        public void Initialize(PlayerGameState gameState)
        {
            _gameState = gameState;
            if (_view == null)
            {
                Debug.LogError("CharacterCollectionView is not assigned.", this);
                enabled = false;
                return;
            }

            _view.Initialize(this);
            RefreshView();
        }

        public void RefreshView()
        {
            if (_gameState == null || _view == null)
            {
                return;
            }

            CharacterCollectionViewModel model = _builder.Build(
                _gameState,
                PrototypeCharacterCatalog.All);
            _view.Render(model);
        }
    }
}
