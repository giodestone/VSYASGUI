using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.Base
{
    /// <summary>
    /// Provides a wrapper for a basic command which can be executed.
    /// </summary>
    public sealed class Command : ICommand
    {
        private readonly Action<object> _action;

        public Command(Action<object> action) => _action = action;

        /// <summary>
        /// Execute the current command.
        /// </summary>
        public void Execute(object parameter) => _action(parameter);

        /// <summary>
        /// Whether it is possible to execture the command. Always true for this command.
        /// </summary>
        public bool CanExecute(object parameter) => true;

        /// <summary>
        /// For when the execute function changes (should not change)
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
