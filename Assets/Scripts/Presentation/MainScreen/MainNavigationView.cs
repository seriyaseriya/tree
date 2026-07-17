using UnityEngine;
using UnityEngine.UI;

namespace WoodClicker.Presentation.MainScreen
{
    public sealed class MainNavigationView : MonoBehaviour
    {
        private static readonly Color SelectedColor =
            new Color(0.78f, 0.52f, 0.24f, 0.96f);
        private static readonly Color UnselectedColor =
            new Color(0.22f, 0.16f, 0.11f, 0.92f);

        [SerializeField] private GameObject _choppingScreen;
        [SerializeField] private GameObject _sellScreen;
        [SerializeField] private GameObject _gachaScreen;
        [SerializeField] private GameObject _characterScreen;
        [SerializeField] private GameObject _skillTreeScreen;
        [SerializeField] private GameObject _optionsScreen;
        [SerializeField] private Image[] _navigationButtonImages;

        private int _currentScreenIndex;

        private void Awake()
        {
            ShowNormalScreen(0);
        }

        public void ShowChoppingScreen() => ShowNormalScreen(0);
        public void ShowSellScreen() => ShowNormalScreen(1);
        public void ShowGachaScreen() => ShowNormalScreen(2);
        public void ShowCharacterScreen() => ShowNormalScreen(3);
        public void ShowSkillTreeScreen() => ShowNormalScreen(4);

        public void ShowOptionsScreen()
        {
            SetAllNormalScreensInactive();
            SetActive(_optionsScreen, true);
            RefreshNavigationColors();
        }

        public void CloseOptionsScreen()
        {
            ShowNormalScreen(_currentScreenIndex);
        }

        private void ShowNormalScreen(int screenIndex)
        {
            _currentScreenIndex = Mathf.Clamp(screenIndex, 0, 4);
            SetActive(_choppingScreen, _currentScreenIndex == 0);
            SetActive(_sellScreen, _currentScreenIndex == 1);
            SetActive(_gachaScreen, _currentScreenIndex == 2);
            SetActive(_characterScreen, _currentScreenIndex == 3);
            SetActive(_skillTreeScreen, _currentScreenIndex == 4);
            SetActive(_optionsScreen, false);
            RefreshNavigationColors();
        }

        private void SetAllNormalScreensInactive()
        {
            SetActive(_choppingScreen, false);
            SetActive(_sellScreen, false);
            SetActive(_gachaScreen, false);
            SetActive(_characterScreen, false);
            SetActive(_skillTreeScreen, false);
        }

        private void RefreshNavigationColors()
        {
            if (_navigationButtonImages == null)
            {
                return;
            }

            for (int i = 0; i < _navigationButtonImages.Length; i++)
            {
                if (_navigationButtonImages[i] != null)
                {
                    _navigationButtonImages[i].color =
                        i == _currentScreenIndex ? SelectedColor : UnselectedColor;
                }
            }
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
