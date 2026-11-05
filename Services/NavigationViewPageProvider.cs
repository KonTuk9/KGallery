using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Abstractions;

namespace KGallery.Services
{
    public class NavigationViewPageProvider : INavigationViewPageProvider
    {
        public object GetPage(Type pageType)
        {
            return App.ServiceProvider.GetRequiredService(pageType);
        }
    }
}
