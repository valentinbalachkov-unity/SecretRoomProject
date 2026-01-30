using TMPro;
using UnityEngine;

public class TextImputController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;

    public void SetState(bool isActive)
    {
        _inputField.interactable = isActive;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public string GetText()
    {
        return _inputField.text;
    }
    
}
