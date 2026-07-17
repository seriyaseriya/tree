using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WoodClicker.Application.Characters;

namespace WoodClicker.Presentation.Characters
{
    public sealed class CharacterCollectionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _ownedTypeCountText;
        [SerializeField] private TMP_Text _emptyStateText;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TMP_FontAsset _fontAsset;

        private CharacterCollectionController _controller;

        public void Initialize(CharacterCollectionController controller)
        {
            _controller = controller;
        }

        private void OnEnable()
        {
            _controller?.RefreshView();
        }

        public void Render(CharacterCollectionViewModel model)
        {
            if (model == null)
            {
                return;
            }

            ClearItems();
            if (_ownedTypeCountText != null)
            {
                _ownedTypeCountText.text = $"所持種類数：{model.OwnedTypeCount}";
            }

            if (_emptyStateText != null)
            {
                _emptyStateText.gameObject.SetActive(model.IsEmpty);
            }

            if (_contentRoot == null)
            {
                return;
            }

            foreach (CharacterCollectionItemViewModel item in model.Items)
            {
                CreateItem(item);
            }
        }

        private void ClearItems()
        {
            if (_contentRoot == null)
            {
                return;
            }

            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_contentRoot.GetChild(i).gameObject);
            }
        }

        private void CreateItem(CharacterCollectionItemViewModel item)
        {
            var itemObject = new GameObject(
                $"CharacterItem_{item.CharacterId}",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement));
            RectTransform itemTransform = itemObject.GetComponent<RectTransform>();
            itemTransform.SetParent(_contentRoot, false);
            itemObject.GetComponent<Image>().color =
                new Color(0.18f, 0.11f, 0.06f, 0.88f);
            itemObject.GetComponent<LayoutElement>().preferredHeight = 210f;

            var textObject = new GameObject(
                "Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.SetParent(itemTransform, false);
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = new Vector2(28f, 16f);
            textTransform.offsetMax = new Vector2(-28f, -16f);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text =
                $"{item.DisplayName}\n" +
                $"レアリティ：{item.Rarity}\n" +
                $"所持数：{item.OwnedCount}　入手順：{item.AcquisitionOrder}";
            text.fontSize = 34f;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = Color.white;
            text.raycastTarget = false;
            if (_fontAsset != null)
            {
                text.font = _fontAsset;
            }
        }
    }
}
