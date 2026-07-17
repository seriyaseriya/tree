using System;
using UnityEngine;
using WoodClicker.Domain.Gacha;
using WoodClicker.Infrastructure.Gacha;
using WoodClicker.Presentation.Gacha;
using WoodClicker.Presentation.MainScreen;
using WoodClicker.State;

namespace WoodClicker.Application.Gacha
{
    public sealed class GachaController : MonoBehaviour
    {
        [SerializeField] private GachaView _view;

        private readonly GachaService _service = new GachaService();
        private readonly IRandomProvider _randomProvider =
            new SystemRandomProvider();

        private GachaDefinition _definition;
        private PlayerGameState _gameState;
        private MainScreenView _mainScreenView;
        private bool _isDrawing;
        private Action _saveGameState;

        public void Initialize(
            PlayerGameState gameState,
            MainScreenView mainScreenView,
            Action saveGameState = null)
        {
            _gameState = gameState;
            _mainScreenView = mainScreenView;
            _saveGameState = saveGameState;
            _definition = PrototypeGachaFactory.Create();
            if (_view == null)
            {
                Debug.LogError("GachaView is not assigned.", this);
                enabled = false;
                return;
            }

            _view.Initialize(this);
            RefreshView();
        }

        public void DrawOnce()
        {
            if (_isDrawing || _gameState == null)
            {
                return;
            }

            _isDrawing = true;
            _view.SetDrawButtonInteractable(false);
            try
            {
                GachaResult result = _service.TryDraw(
                    _definition,
                    _gameState.OwnedMoney,
                    _gameState.GetOwnedCharacterIds(),
                    _randomProvider);
                if (!result.Succeeded)
                {
                    _view.ShowFailure(result.FailureReason);
                    return;
                }

                if (!_gameState.TrySpendMoney(result.Cost))
                {
                    _view.ShowFailure(GachaFailureReason.InsufficientMoney);
                    return;
                }

                int ownedCount = _gameState.AcquireCharacter(result.CharacterId);
                _view.ShowResult(
                    result.CharacterName,
                    result.Rarity,
                    result.IsNew,
                    ownedCount);
                RefreshView();
                _mainScreenView.RefreshOwnedMoney(_gameState.OwnedMoney);
                _saveGameState?.Invoke();
            }
            finally
            {
                _isDrawing = false;
                _view.SetDrawButtonInteractable(true);
            }
        }

        public void RefreshView()
        {
            if (_view == null || _gameState == null || _definition == null)
            {
                return;
            }

            _view.RefreshStatus(_definition.Cost, _gameState.OwnedMoney);
        }
    }
}
