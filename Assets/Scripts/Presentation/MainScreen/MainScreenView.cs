using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodClicker.Application;

namespace WoodClicker.Presentation.MainScreen
{
    public sealed class MainScreenView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _ownedLogsText;
        [SerializeField] private TMP_Text _ownedMoneyText;
        [SerializeField] private Image _cooldownGauge;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private TMP_Text _logsPerTapText;
        [SerializeField] private TMP_Text _logsPerSecondText;
        [SerializeField] private TMP_Text _equippedToolText;
        [SerializeField] private TMP_Text _toolStatusText;
        [SerializeField] private TMP_Text _sellOwnedLogsText;
        [SerializeField] private TMP_Text _baseSellPriceText;
        [SerializeField] private TMP_Text _finalSellAmountText;

        private ChoppingController _controller;

        public void Initialize(ChoppingController controller)
        {
            _controller = controller;
        }

        public void OnTreeButtonClicked()
        {
            if (_controller == null)
            {
                Debug.LogError("ChoppingController is not initialized.", this);
                return;
            }

            _controller.TryChop();
        }

        public void OnSellAllButtonClicked()
        {
            if (_controller == null)
            {
                Debug.LogError("ChoppingController is not initialized.", this);
                return;
            }

            _controller.SellAllLogs();
        }

        public void RefreshOwnedLogs(double ownedLogs)
        {
            if (_ownedLogsText != null)
            {
                _ownedLogsText.text = ownedLogs.ToString("0.##");
            }
        }

        public void RefreshCooldown(float remainingRatio)
        {
            if (_cooldownGauge != null)
            {
                _cooldownGauge.fillAmount = Mathf.Clamp01(remainingRatio);
            }

            if (_cooldownText != null)
            {
                _cooldownText.text = remainingRatio > 0f ? "クールタイム中" : "伐採可能";
            }
        }

        public void RefreshOwnedMoney(double ownedMoney)
        {
            if (_ownedMoneyText != null)
            {
                _ownedMoneyText.text = ownedMoney.ToString("0.##");
            }
        }

        public void RefreshChoppingInfo(
            double logsPerTap,
            double logsPerSecond,
            string equippedTool,
            string toolStatus)
        {
            SetText(_logsPerTapText, logsPerTap.ToString("0.##"));
            SetText(_logsPerSecondText, logsPerSecond.ToString("0.##"));
            SetText(_equippedToolText, equippedTool);
            SetText(_toolStatusText, toolStatus);
        }

        public void RefreshSellScreen(
            double ownedLogs,
            double baseSellPrice,
            double finalSellAmount)
        {
            SetText(_sellOwnedLogsText, ownedLogs.ToString("0.##"));
            SetText(_baseSellPriceText, baseSellPrice.ToString("0.##"));
            SetText(_finalSellAmountText, finalSellAmount.ToString("0.##"));
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
