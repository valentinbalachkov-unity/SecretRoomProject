using System.Collections;
using System.Linq;
using UnityEngine;

namespace _Project.Audio.Realisation
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private MusicConfig _config;
        [SerializeField] private AudioSource _audioSourceA; // Для кроссфейда нужны два источника
        [SerializeField] private AudioSource _audioSourceB;

        private AudioSource _activeSource;
        private MusicState _currentState = MusicState.None;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            // Singleton + DontDestroyOnLoad
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _activeSource = _audioSourceA;
        }

        /// <summary>
        /// Переключить музыку в зависимости от состояния
        /// </summary>
        public void PlayMusic(MusicState state)
        {
            if (_currentState == state) return; // Уже играет эта тема

            var track = _config.tracks.FirstOrDefault(t => t.state == state);
            if (track == null && state != MusicState.None)
            {
                Debug.LogWarning($"[AudioManager] Нет трека для состояния {state}");
                return;
            }

            _currentState = state;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(CrossfadeRoutine(track));
        }

        private IEnumerator CrossfadeRoutine(MusicTrack newTrack)
        {
            float duration = _config.fadeDuration;
            float timer = 0f;

            // Определяем, какой источник сейчас играет, а какой будет новым
            AudioSource outgoing = _activeSource;
            AudioSource incoming = (_activeSource == _audioSourceA) ? _audioSourceB : _audioSourceA;

            // Настраиваем новый источник
            if (newTrack != null)
            {
                incoming.clip = newTrack.clip;
                incoming.loop = newTrack.loop;
                incoming.volume = 0f; // Начинаем с 0
                incoming.Play();
            }

            float startVol = outgoing.volume;
            float targetVol = (newTrack != null) ? newTrack.volume : 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                // Плавное затухание старого
                outgoing.volume = Mathf.Lerp(startVol, 0f, t);

                // Плавное появление нового
                if (newTrack != null)
                {
                    incoming.volume = Mathf.Lerp(0f, targetVol, t);
                }

                yield return null;
            }

            // Финализация
            outgoing.Stop();
            outgoing.volume = 0f;

            if (newTrack != null)
            {
                incoming.volume = targetVol;
                _activeSource = incoming; // Меняем активный источник
            }
            else
            {
                // Если перешли в состояние None (тишина)
                _activeSource = outgoing; // Не важно какой, оба молчат
            }
        }
    }
}