using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodClicker.Application;

namespace WoodClicker.Presentation.MainScreen
{
    public sealed class MainScreenView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _ownedLogsText;
        [SerializeField] private Image _cooldownGauge;

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
        }
    }
}
