using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodClicker.Application.Gacha;
using WoodClicker.Domain.Characters;
using WoodClicker.Domain.Gacha;

namespace WoodClicker.Presentation.Gacha
{
    public sealed class GachaView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _ownedMoneyText;
        [SerializeField] private TMP_Text _characterNameText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _acquisitionTypeText;
        [SerializeField] private TMP_Text _ownedCountText;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _drawButton;

        private GachaController _controller;

        public void Initialize(GachaController controller)
        {
            _controller = controller;
        }

        private void OnEnable()
        {
            _controller?.RefreshView();
        }

        public void OnDrawButtonClicked()
        {
            _controller?.DrawOnce();
        }

        public void RefreshStatus(double cost, double ownedMoney)
        {
            SetText(_costText, cost.ToString("0.##"));
            SetText(_ownedMoneyText, ownedMoney.ToString("0.##"));
        }

        public void ShowResult(
            string characterName,
            CharacterRarity rarity,
            bool isNew,
            int ownedCount)
        {
            SetText(_characterNameText, characterName);
            SetText(_rarityText, rarity.ToString());
            SetText(_acquisitionTypeText, isNew ? "新規入手" : "重複");
            SetText(_ownedCountText, ownedCount.ToString());
            SetText(_errorText, string.Empty);
        }

        public void ShowFailure(GachaFailureReason failureReason)
        {
            if (failureReason == GachaFailureReason.InsufficientMoney)
            {
                SetText(_errorText, "所持金が不足しています");
            }
        }

        public void SetDrawButtonInteractable(bool interactable)
        {
            if (_drawButton != null)
            {
                _drawButton.interactable = interactable;
            }
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
