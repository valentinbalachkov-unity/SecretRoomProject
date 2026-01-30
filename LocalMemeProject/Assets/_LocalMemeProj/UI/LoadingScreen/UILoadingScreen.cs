using Dreamers.UI.UIService.Realization;
using TMPro;
using UnityEngine;

public class UILoadingScreen : UIBaseWindow
{
    [SerializeField] private TMP_Text _loadingTextData;

    public void UpdateText(string text)
    {
        _loadingTextData.text = $"Loading: {text}";
    }
}
