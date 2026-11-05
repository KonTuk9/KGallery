using KGallery.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace KGallery.Services
{
    public class OpenDialogFactory
    {
        private Dictionary<Type, object> l_Dialogs = new Dictionary<Type, object>();
        private ContentPresenter _contentPresenter;

        public void SetContentPresenter(ContentPresenter contentPresenter)
        {
            _contentPresenter = contentPresenter;
        }

        public void ShowDialog<TDialog, KArgument>(KArgument parameter)
        {
            if (!l_Dialogs.TryGetValue(typeof(TDialog), out object result))
            {
                result = App.ServiceProvider.GetRequiredService<TDialog>();
                l_Dialogs.Add(typeof(TDialog), result);
            }

            _contentPresenter.Content = result;
            ((IOpenDialog<KArgument>)result).Open(parameter);
        }
    }
}
