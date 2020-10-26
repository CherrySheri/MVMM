using LIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LISFramework.ViewModel {
  public class LisCommVM : ViewModelBase {

    ICommand _SendOrder;

    public ICommand SendOrder {
      get {
        if (_SendOrder == null) {
          _SendOrder = new RelayCommand(param => this.SendOrder_Execute(), null);
        }
        return _SendOrder;
      }
    }

    private void SendOrder_Execute() {
      //Load Serial Json File
      JsonWR jsonR = new JsonWR();
      SerialFields serialF = jsonR.ReadJsonFile();
      SendReciver sendR = new SendReciver(SendReciver.ConnectionType.Serial, serialF);
      sendR.SendOrderToMachine();
    }

  }
}
