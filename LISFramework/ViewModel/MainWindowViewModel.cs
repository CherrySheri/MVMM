using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LISFramework.ViewModel {
  public class MainWindowViewModel : ViewModelBase {

    #region Setting ToolBar
    
    ICommand _SettingCommand;

    public ICommand SettingCommand {
      get {
        if (_SettingCommand == null) {
          _SettingCommand = new RelayCommand(param => this.Setting_Execute(), null);
        }
        return _SettingCommand;
      }
    }

    //private bool SettingClick_CanExecute(object param) {
    //  if (string.IsNullOrEmpty(this.Dis)) {

    //  }
    //}


    private void Setting_Execute() {
      LisSettings lisSettings = new LisSettings();
      lisSettings.Show();
    }

    #endregion Setting ToolBar
  }

}
