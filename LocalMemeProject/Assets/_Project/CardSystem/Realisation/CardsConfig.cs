using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardsConfig", menuName = "ProjectData/CardsConfig")]
public class CardsConfig : ScriptableObject
{
    [SerializeField] private List<Sprite> _icons = new();
    
    
    [HideInInspector] public List<CardData> cardDataList = new();

    public void Init()
    {
        cardDataList.Clear();
        
        foreach (var icon in _icons)
        {
            cardDataList.Add(new CardData()
            {
                picture = icon,
                uid = icon.name
            });
        }
    }
}

[Serializable]
public class CardData
{
    public string uid;
    public Sprite picture;
}