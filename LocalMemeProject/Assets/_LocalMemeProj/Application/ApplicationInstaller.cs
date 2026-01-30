using _Project.Audio.Realisation;
using _Project.GameSystem.Realisation;
using _Project.LobbySystem.Realisation;
using Dreamers.MainCamera;
using Dreamers.UI.Installers;
using Dreamers.UI.UIService.Installer;
using Zenject;

namespace Dreamers.App
{
    public class ApplicationInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<CameraView>()
                .FromComponentInNewPrefabResource(nameof(CameraView))
                .AsSingle().NonLazy();
            
            Container
                .Bind<FileOpener>()
                .FromComponentInNewPrefabResource(nameof(FileOpener))
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<FirebaseManager>()
                .FromComponentInNewPrefabResource(nameof(FirebaseManager))
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<AudioManager>()
                .FromComponentInNewPrefabResource(nameof(AudioManager))
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<FusionLobbySystem>()
                .FromComponentInNewPrefabResource(nameof(FusionLobbySystem))
                .AsSingle()
                .NonLazy();
            
            UIFrameworkInstaller.Install(Container);
            
            Container
                .Bind<GameStateUIPresenter>()
                .FromComponentInNewPrefabResource(nameof(GameStateUIPresenter))
                .AsSingle()
                .NonLazy();
            

            UIWindowControllersInstaller.Install(Container);

            Container
                .Bind<ApplicationStartup>()
                .AsSingle()
                .NonLazy();
           
        }
    }
}
