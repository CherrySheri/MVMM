using LIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LISFramework.ViewModel {
  public class LisCommVM : ViewModelBase {


    public LisCommVM() {
      JsonWR jsonR = new JsonWR();
      commFields = jsonR.ReadJsonFile();
      sendR = new SendReciver(commFields);
    }

    ICommand _SendOrder;
    SendReciver sendR { get; set; }
    CommunicationFields commFields { get; set; }

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
      sendR.SendOrderToMachine();
    }

  }
}
