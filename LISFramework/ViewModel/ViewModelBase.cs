using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LISFramework.ViewModel {
  public class ViewModelBase : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyPropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class RelayCommand : ICommand {

    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute) : this(execute, null) {
      try {
        if (execute ==  null) {
          throw new NotImplementedException("Not implemented");
        }
      } catch (Exception) {
        throw;
      }
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
      try {
        if (execute == null) {
          _execute = null;
          throw new NotImplementedException("Not implemented");
        }
        _execute = execute; _canExecute = canExecute;
      } catch (Exception) {
      }
    }


    public event EventHandler CanExecuteChanged {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter) {
      return _canExecute == null ? true : _canExecute(parameter);
    }

    public void Execute(object parameter) {
      _execute(parameter);
    }
  }


}
