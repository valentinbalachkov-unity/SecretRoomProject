using _LocalMemeProj.UI.UIGameScreen;
using _LocalMemeProj.UI.UILobby;
using _LocalMemeProj.UI.UIScoreScreen.Realisation;
using Dreamers.Core.Controllers.Scripts.Interfaces;
using Zenject;

namespace Dreamers.UI.Installers
{
    public class UIWindowControllersInstaller : Installer<UIWindowControllersInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IWindowController<UILobby>>()
                .To<UILobbyController>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<IWindowController<UiGameScreen>>()
                .To<GameScreenController>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<IWindowController<UiResultScreen>>()
                .To<UIResultController>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<IWindowController<UILoadingScreen>>()
                .To<UILoadingController>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<IWindowController<UIMainMenu>>()
                .To<MainMenuController>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<IWindowController<UIScoreView>>()
                .To<UIScoreController>()
                .AsSingle()
                .NonLazy();
        }
    }
}