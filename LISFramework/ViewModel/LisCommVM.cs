using LIS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace LISFramework.ViewModel {
  public class LisCommVM : ViewModelBase {


    public LisCommVM() {
      JsonWR jsonR = new JsonWR();
      commFields = jsonR.ReadJsonFile();
      SerialMsg = new SerialMessageStatus(); TcpMessageStatus = new TcpMesageStatus();
      sendR = new SendReciver(commFields, SerialMsg, TcpMessageStatus);
      sendR.InitializeConnection();
    }


    ICommand _SendOrder;
    SendReciver sendR { get; set; }
    CommunicationFields commFields { get; set; }
    SerialMessageStatus SerialMsg { get; set; }
    TcpMesageStatus TcpMessageStatus { get; set; }

    public ICommand SendOrder {
      get {
        if (_SendOrder == null) {
          _SendOrder = new RelayCommand(param => this.SendOrder_Execute(), null);
        }
        return _SendOrder;
      }
    }

    public ObservableCollection<string> SerialMessageList {
      get {
        return SerialMsg.MessageCollection;
      }
      set {
        SerialMsg.MessageCollection = value;
        NotifyPropertyChanged("MessageList");
      }
    }



    public ObservableCollection<string> SerialStatusCollection {
      get {
        return SerialMsg.StatusCollection;
      }
      set {
        SerialMsg.StatusCollection = value;
        NotifyPropertyChanged("StatusList");
      }
    }


    private void SendOrder_Execute() {
      //Load Serial Json File
      sendR.SendOrderToMachine();
    }

  }
}
