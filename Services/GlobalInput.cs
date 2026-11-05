using System.Windows;
using System.Windows.Input;

namespace KGallery.Services
{
    public class GlobalInput
    {
        private FrameworkElement _inputHandler;

        public void SetInputHandler(FrameworkElement inputHandler)
        {
            _inputHandler = inputHandler;
        }

        public void Subscribe(KeyEventHandler callback)
        {
            _inputHandler.KeyDown += callback;
        }

        public void Unsubscribe(KeyEventHandler callback)
        {
            _inputHandler.KeyDown -= callback;
        }
    }
}
