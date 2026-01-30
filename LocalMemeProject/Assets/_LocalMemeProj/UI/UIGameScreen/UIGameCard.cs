using System;
using _Project.LobbySystem.Realisation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _LocalMemeProj.UI.UIGameScreen
{
    public class UIGameCard : MonoBehaviour
    {
        public CardData CurrentData => _currentData;
        public Action<UIGameCard> OnClick;
        public PlayerController playerController;

        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _rectTransformBackground;
        [SerializeField] private RectTransform _rectTransformPrefab;
        [SerializeField] private Button _onClickButton;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _userNameText;

        [SerializeField] private Vector2 _horizontalSiza;
        [SerializeField] private Vector2 _varticalSiza;

        [SerializeField] private Image _background;
        
        private CardData _currentData;


        private void Awake()
        {
            _onClickButton.onClick.AddListener(() =>
            {
                OnClick?.Invoke(this);
            });
        }

        private void OnDestroy()
        {
            OnClick = null;
            _onClickButton.onClick.RemoveAllListeners();
        }

        public void SelectCard(bool isSelect, string text)
        {
            _background.color = isSelect ? Color.gold : Color.black;
            _descriptionText.text = text;
        }
        
        public void SelectCard(bool isSelect)
        {
            _background.color = isSelect ? Color.gold : Color.black;
        }

        public void Init(CardData data)
        {
            _onClickButton.interactable = true;
            _rectTransform.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza : _varticalSiza;
            _rectTransformBackground.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            _rectTransformPrefab.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            
            _currentData = data;
            //_rectTransform.sizeDelta = new Vector2(data.sprite.texture.width, data.sprite.texture.height);
            _icon.sprite = data.picture;
        }
        

        public void SetResult(CardData data, string description)
        {
            _onClickButton.interactable = false;
            _currentData = data;
            
            //_rectTransform.sizeDelta = new Vector2(data.texture.width, data.texture.height);
            _rectTransform.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza : _varticalSiza;
            _rectTransformBackground.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            _rectTransformPrefab.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            
            _descriptionText.text = description;
            _icon.sprite = data.picture;
            _userNameText.text = "";
        }
        
        public void SetResult(CardData data, string description, PlayerController pController)
        {
            _currentData = data;
            playerController = pController;
            _onClickButton.interactable = true;
            
            //_rectTransform.sizeDelta = new Vector2(data.texture.width, data.texture.height);
            _rectTransform.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza : _varticalSiza;
            _rectTransformBackground.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            _rectTransformPrefab.sizeDelta = data.picture.texture.width > data.picture.texture.height ? _horizontalSiza + new Vector2(30,30) : _varticalSiza + new Vector2(30,30);
            
            _descriptionText.text = description;
            _icon.sprite = data.picture;
            _userNameText.text = playerController.PlayerName.Value;
            
        }
        
    }
}