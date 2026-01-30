using Fusion;
using System.Collections.Generic;
using _Project.Config.Realisation;
using Dreamers.UI.UIService.Interfaces;
using UnityEngine;

namespace _Project.ThemeSystem.Realisation
{
    public class ThemeManager : NetworkBehaviour
    {
        // Локальный список использованных тем (индексы)
        private HashSet<int> _usedThemeIndices = new HashSet<int>();

        // Текущая активная тема (синхронизируется по сети)
        [Networked] public int CurrentThemeId { get; private set; }
        [Networked] public NetworkString<_64> CurrentThemeName { get; private set; }

        private RoundThemesConfig _themesConfig;
        private ChangeDetector _changes;
        private UiGameScreen _uiGameScreen;
        private UiResultScreen _uiResultScreen;
     
        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public void Init(RoundThemesConfig roundThemesConfig, IUIService uiService)
        {
            _themesConfig = roundThemesConfig;
            _uiGameScreen = uiService.Get<UiGameScreen>();
            _uiResultScreen = uiService.Get<UiResultScreen>();
            
            Debug.Log($"{_uiGameScreen.gameObject} UI GAME SCREEN");
            
            if (Object.HasStateAuthority)
            {
                // Инициализация: сброс использованных тем в начале игры
              ResetThemes();
            }
        }

        /// <summary>
        /// Выдать новую тему для раунда. Вызывает ТОЛЬКО хост.
        /// Возвращает выбранную тему или null, если темы закончились (хотя автоматически сбросится).
        /// </summary>
        public RoundThemeData DrawTheme()
        {
            if (!Object.HasStateAuthority)
            {
                return null;
            }

            // Найти доступные темы (которые ещё не использовались)
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < _themesConfig.roundsThemeList.Count; i++)
            {
                if (!_usedThemeIndices.Contains(i))
                {
                    availableIndices.Add(i);
                }
            }

            // Если все темы использованы — сброс
            if (availableIndices.Count == 0)
            {
                Debug.Log("[ThemeManager] Все темы использованы, сброс пула");
                _usedThemeIndices.Clear();

                // Заново заполняем доступные
                for (int i = 0; i < _themesConfig.roundsThemeList.Count; i++)
                {
                    availableIndices.Add(i);
                }
            }

            // Выбрать случайную тему
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            RoundThemeData selectedTheme = _themesConfig.roundsThemeList[randomIndex];

            // Пометить тему как использованную
            _usedThemeIndices.Add(randomIndex);

            // Обновить networked свойства (синхронизируется всем клиентам автоматически)
            CurrentThemeId = selectedTheme.id;
            CurrentThemeName = selectedTheme.theme;

            Debug.Log($"[ThemeManager] Выбрана тема: {selectedTheme.theme} (ID: {selectedTheme.id})");

            return selectedTheme;
        }

        /// <summary>
        /// Сброс всех использованных тем (например, при рестарте игры).
        /// </summary>
        public void ResetThemes()
        {
            if (!Object.HasStateAuthority) return;

            _usedThemeIndices.Clear();
            CurrentThemeId = 0;
            CurrentThemeName = "";
        }

        public override void Render()
        {
            // Клиенты отслеживают изменение темы и обновляют UI
            foreach (var change in _changes.DetectChanges(this))
            {
                if (change == nameof(CurrentThemeName))
                {
                    OnThemeChanged();
                }
            }
        }

        private void OnThemeChanged()
        {
            // Уведомить UI о новой теме
            Debug.Log($"[ThemeManager Client] Новая тема: {CurrentThemeName}");

            _uiGameScreen.UpdateUI($"{CurrentThemeName}");
            _uiResultScreen.OnChangeTheme($"{CurrentThemeName}");
        }
    }
}