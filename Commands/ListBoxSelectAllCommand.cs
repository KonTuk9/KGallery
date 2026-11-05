using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Commands
{
    public class ListBoxSelectAllCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return parameter is ListBox listBox && (listBox.SelectionMode == SelectionMode.Extended || listBox.SelectionMode == SelectionMode.Multiple);
        }

        public void Execute(object parameter)
        {
            if (parameter is ListBox listBox)
            {
                listBox.SelectAll();
            }
        }
    }
}
