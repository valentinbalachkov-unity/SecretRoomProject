using Zenject;
using IUIRoot = Dreamers.UI.UIService.Interfaces.IUIRoot;
using IUIService = Dreamers.UI.UIService.Interfaces.IUIService;
using UIRoot = Dreamers.UI.UIService.Realization.UIRoot;

namespace Dreamers.UI.UIService.Installer 
{
    public class UIFrameworkInstaller : Installer<UIFrameworkInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IUIRoot>()
                .To<UIRoot>()
                .FromComponentInNewPrefabResource(nameof(UIRoot))
                .AsSingle();

            Container
                .Bind<IUIService>()
                .To<Realization.UIService>()
                .AsSingle();
        }
    }
}
