using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Config.Realisation
{
    [CreateAssetMenu(fileName = "RoundThemesConfig", menuName = "ProjectData/RoundThemesConfig")]
    public class RoundThemesConfig : ScriptableObject
    {
        public List<RoundThemeData> roundsThemeList = new();
    }

    [Serializable]
    public class RoundThemeData
    {
        public int id;
        public string theme;
    }
}