using UnityEngine;
using WoodClicker.Domain.Chopping;
using WoodClicker.Domain.Selling;
using WoodClicker.Presentation.MainScreen;
using WoodClicker.State;

namespace WoodClicker.Application
{
    public sealed class ChoppingController : MonoBehaviour
    {
        private const double LogsPerChop = 1d;
        private const float CooldownSeconds = 1f;
        private const double MoneyPerLog = 1d;

        [SerializeField] private MainScreenView _mainScreenView;

        private PlayerGameState _gameState;
        private ChoppingService _choppingService;
        private SellingService _sellingService;
        private float _remainingCooldown;
        private float _activeCooldownDuration;

        public double OwnedLogs => _gameState?.OwnedLogs ?? 0d;
        public long TotalManualChops => _gameState?.TotalManualChops ?? 0L;
        public double OwnedMoney => _gameState?.OwnedMoney ?? 0d;
        public bool IsCooldownActive => _remainingCooldown > 0f;

        private void Awake()
        {
            _gameState = new PlayerGameState();
            _choppingService = new ChoppingService(
                new ChoppingConfig(LogsPerChop, CooldownSeconds));
            _sellingService = new SellingService(MoneyPerLog);

            if (_mainScreenView == null)
            {
                Debug.LogError("MainScreenView is not assigned.", this);
                enabled = false;
                return;
            }

            _mainScreenView.Initialize(this);
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
            ChoppingResult result = _choppingService.TryChop(IsCooldownActive);
            if (!result.Succeeded)
            {
                return result;
            }

            _gameState.ApplyManualChop(result.EarnedLogs);
            _activeCooldownDuration = result.CooldownSeconds;
            _remainingCooldown = result.CooldownSeconds;
            RefreshView();
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
            return result;
        }

        private void RefreshView()
        {
            _mainScreenView.RefreshOwnedLogs(_gameState.OwnedLogs);
            _mainScreenView.RefreshOwnedMoney(_gameState.OwnedMoney);
            _mainScreenView.RefreshCooldown(GetCooldownRemainingRatio());
        }
    }
}
