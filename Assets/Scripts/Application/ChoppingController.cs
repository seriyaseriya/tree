using UnityEngine;
using WoodClicker.Application.Gacha;
using WoodClicker.Application.Characters;
using WoodClicker.Application.Save;
using WoodClicker.Domain.Chopping;
using WoodClicker.Domain.Selling;
using WoodClicker.Presentation.MainScreen;
using WoodClicker.State;
using WoodClicker.Infrastructure.Save;

namespace WoodClicker.Application
{
    public sealed class ChoppingController : MonoBehaviour
    {
        private const double LogsPerChop = 1d;
        private const float CooldownSeconds = 1f;
        private const double MoneyPerLog = 1d;
        private const double InitialTreeMaxHealth = 100d;

        [SerializeField] private MainScreenView _mainScreenView;
        [SerializeField] private GachaController _gachaController;
        [SerializeField] private CharacterCollectionController
            _characterCollectionController;
        [SerializeField] private SaveController _saveController;

        private PlayerGameState _gameState;
        private ChoppingService _choppingService;
        private SellingService _sellingService;
        private TreeState _treeState;
        private float _remainingCooldown;
        private float _activeCooldownDuration;

        public double OwnedLogs => _gameState?.OwnedLogs ?? 0d;
        public long TotalManualChops => _gameState?.TotalManualChops ?? 0L;
        public double OwnedMoney => _gameState?.OwnedMoney ?? 0d;
        public double CurrentTreeHealth =>
            _treeState?.CurrentHealth ?? InitialTreeMaxHealth;
        public bool IsTreeFelled => _treeState?.IsFelled ?? false;
        public bool IsCooldownActive => _remainingCooldown > 0f;

        private void Awake()
        {
            _saveController ??= GetComponent<SaveController>();
            _saveController ??= gameObject.AddComponent<SaveController>();
            LoadedGameState loadedState = _saveController.LoadOrCreate();
            _gameState = loadedState.Player;
            _choppingService = new ChoppingService(
                new ChoppingConfig(LogsPerChop, CooldownSeconds));
            _sellingService = new SellingService(MoneyPerLog);
            _treeState = loadedState.Tree;

            if (_mainScreenView == null)
            {
                Debug.LogError("MainScreenView is not assigned.", this);
                enabled = false;
                return;
            }

            _mainScreenView.Initialize(this);
            if (_gachaController != null)
            {
                _gachaController.Initialize(
                    _gameState,
                    _mainScreenView,
                    SaveGameState);
            }
            if (_characterCollectionController != null)
            {
                _characterCollectionController.Initialize(_gameState);
            }
            RefreshView();
        }

        private void Update()
        {
            if (!IsCooldownActive)
            {
                return;
            }

            _remainingCooldown = Mathf.Max(
                0f,
                _remainingCooldown - Time.unscaledDeltaTime);

            _mainScreenView.RefreshCooldown(GetCooldownRemainingRatio());
        }

        public ChoppingResult TryChop()
        {
            ChoppingResult result = _choppingService.TryChop(
                IsCooldownActive,
                _treeState.IsFelled);
            if (!result.Succeeded)
            {
                return result;
            }

            _gameState.ApplyManualChop(result.EarnedLogs);
            _treeState.ApplyDamage(result.TreeDamage);
            _activeCooldownDuration = result.CooldownSeconds;
            _remainingCooldown = result.CooldownSeconds;
            RefreshView();
            SaveGameState();
            return result;
        }

        public float GetCooldownRemainingRatio()
        {
            if (_activeCooldownDuration <= 0f || _remainingCooldown <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_remainingCooldown / _activeCooldownDuration);
        }

        public SellingResult SellAllLogs()
        {
            SellingResult result = _sellingService.SellAll(_gameState.OwnedLogs);
            if (!result.Succeeded)
            {
                return result;
            }

            _gameState.ApplySale(result.SoldLogs, result.EarnedMoney);
            RefreshView();
            SaveGameState();
            return result;
        }

        public SellingResult GetSellAllPreview()
        {
            return _sellingService.SellAll(_gameState.OwnedLogs);
        }

        private void RefreshView()
        {
            _mainScreenView.RefreshOwnedLogs(_gameState.OwnedLogs);
            _mainScreenView.RefreshOwnedMoney(_gameState.OwnedMoney);
            SellingResult sellPreview = GetSellAllPreview();
            _mainScreenView.RefreshSellScreen(
                _gameState.OwnedLogs,
                MoneyPerLog,
                sellPreview.EarnedMoney);
            _mainScreenView.RefreshChoppingInfo(LogsPerChop, 0d, "なし", "正常");
            _mainScreenView.RefreshCooldown(GetCooldownRemainingRatio());
        }

        private void SaveGameState()
        {
            _saveController?.Save(_gameState, _treeState);
        }
    }
}
