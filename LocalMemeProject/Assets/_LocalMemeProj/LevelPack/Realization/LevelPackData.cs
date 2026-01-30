using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultLevelPack", menuName = "Configs/LevelPack")]
public class LevelPackData : ScriptableObject
{
    public string LevelPackName => _levelPackName;
    public string LevelPackDescription => _levelPackDescription;
    public List<PackThemesData> RoundThemes => _roundThemes;

    public List<PackImagesData> Images => _images;
    
    [SerializeField] private string _levelPackName;
    [SerializeField] private string _levelPackDescription;
    [SerializeField] private List<PackThemesData> _roundThemes = new();
    [SerializeField] private List<PackImagesData> _images = new();

    public LevelPackData(List<PackImagesData> sprites, List<PackThemesData> textList, string packName, string packDescription)
    {
        _images = new(sprites);
        _roundThemes = new(textList);
        _levelPackName = packName;
        _levelPackDescription = packDescription;
    }

    [Serializable]
    public class PackImagesData
    { 
        [HideInInspector] public string id;
        public Sprite sprite;
    }
    
    [Serializable]
    public class PackThemesData
    {
        public int id;
        public string theme;
    }
}
