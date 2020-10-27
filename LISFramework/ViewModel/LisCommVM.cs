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
      _lisPatFields = new LisPatientFields();
      serialMsgStatus = new SerialMessageStatus(); tcpMsgStatus = new TcpMesageStatus();
      sendR = new SendReciver(commFields, serialMsgStatus, tcpMsgStatus, _lisPatFields);
      sendR.InitializeConnection();
    }

    #region Private Variables
    
    ICommand _SendOrder;
    SendReciver sendR { get; set; }
    CommunicationFields commFields { get; set; }
    SerialMessageStatus serialMsgStatus { get; set; }
    TcpMesageStatus tcpMsgStatus { get; set; }
    LisPatientFields _lisPatFields { get; set; }

    #endregion Private Variables

    public ICommand SendOrder {
      get {
        if (_SendOrder == null) {
          _SendOrder = new RelayCommand(param => this.SendOrder_Execute(), null);
        }
        return _SendOrder;
      }
    }

    #region Serial Msg and Status Collection

    public ObservableCollection<string> SerialMsgColl {
      get {
        return serialMsgStatus.MessageCollection;
      }
      set {
        serialMsgStatus.MessageCollection = value;
        NotifyPropertyChanged("SerialMsgColl");
      }
    }


    public ObservableCollection<string> SerialStatusColl {
      get {
        return serialMsgStatus.StatusCollection;
      }
      set {
        serialMsgStatus.StatusCollection = value;
        NotifyPropertyChanged("SerialStatusColl");
      }
    }

    #endregion Serial Msg and Status Collection

    public LisPatientFields LisPatFields {
      get {
        return _lisPatFields;
      }
      set {
        _lisPatFields = value;
        NotifyPropertyChanged("LisPatFields");
      }
    }



    #region Tcp Msg and Status Collection

    public ObservableCollection<string> TcpMsgColl {
      get {
        return tcpMsgStatus.MessageCollection;
      }
      set {
        tcpMsgStatus.MessageCollection = value;
        NotifyPropertyChanged("TcpMsgColl");
      }
    }


    public ObservableCollection<string> TcpStatusColl {
      get {
        return tcpMsgStatus.StatusCollection;
      }
      set {
        tcpMsgStatus.StatusCollection = value;
        NotifyPropertyChanged("TcpStatusColl");
      }
    }

    #endregion Tcp Msg and Status Collection


    private void SendOrder_Execute() {
      sendR.SendOrderToMachine();
    }
  }

  


}
