using System;
using _Project.LobbySystem.Realisation;
using Dreamers.UI.UIService.Interfaces;
using UniRx;

namespace Dreamers.App
{
    public class ApplicationStartup : IDisposable
    {
        private CompositeDisposable _disposable = new();
        public ApplicationStartup(IUIService uiService, FusionLobbySystem fusionLobbySystem)
        {
            fusionLobbySystem.Init(uiService);
            uiService.Show<UIMainMenu>();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}