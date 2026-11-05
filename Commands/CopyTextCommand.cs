using System.Windows;
using System.Windows.Input;

namespace KGallery.Commands
{
    public class CopyTextCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return parameter != null;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                Clipboard.SetText(text);
            }
        }
    }
}
