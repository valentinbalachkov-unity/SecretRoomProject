using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicConfig", menuName = "Audio/MusicConfig")]
public class MusicConfig : ScriptableObject
{
    public List<MusicTrack> tracks = new();
    public float fadeDuration = 1.0f; // Время плавного перехода
}

[System.Serializable]
public class MusicTrack
{
    public MusicState state;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
}

public enum MusicState
{
    MainMenu,
    Lobby,
    GameRound, // Фаза игры
    Voting,    // Фаза голосования
    Results,   // Финальные результаты
    None       // Тишина
}