using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LIS;

namespace LISFramework.ViewModel {
  public class LisSettingVM : ViewModelBase {


    private TcpFields _tcpFields;
    private SerialFields _serialFields;
    

    public LisSettingVM() {
      _tcpFields = new TcpFields();
      _serialFields = new SerialFields();
      ConnectionTypeIEnumerable = Enum.GetNames(typeof(SendReciver.ConnectionType));
      TcpConnModeIEnumerable = Enum.GetNames(typeof(SendReciver.TcpConnectionMode));
    }

    public TcpFields TcpF {
      get {
        return _tcpFields;
      }
      set {
        _tcpFields = value;
        NotifyPropertyChanged("TcpF");

      }
    }

    public SerialFields SerialF {
      get {
        return _serialFields;
      }
      set {
        _serialFields = value;
        NotifyPropertyChanged("SerialF");

      }
    }

    public List<string> BaudRateList {
      get {
        return _serialFields.BaudRateList;
      }
      set {
        _serialFields.BaudRateList = value;
        NotifyPropertyChanged("BaudRateList");
      }
    }



    public List<string> DataBitList {
      get {
        return _serialFields.DataBitList;
      }
      set {
        _serialFields.DataBitList = value;
        NotifyPropertyChanged("DataBitList");
      }
    }

    public List<string> FlowControlList {
      get {
        return _serialFields.FlowControlList;
      }
      set {
        _serialFields.FlowControlList = value;
        NotifyPropertyChanged("FlowControlList");
      }
    }

    public List<string> ParityList {
      get {
        return _serialFields.ParityList;
      }
      set {
        _serialFields.ParityList = value;
        NotifyPropertyChanged("ParityList");
      }
    }
    public List<string> StopBitList {
      get {
        return _serialFields.StopBitList;
      }
      set {
        _serialFields.StopBitList = value;
        NotifyPropertyChanged("StopBitList");
      }
    }

    private string _connType;
    public string ConnectionType {
      get {
        return _connType;
      }
      set {
        _connType = value;
        NotifyPropertyChanged("ConnectionType");
      }
    }

    private string _tcpConnMode;
    public string TcpConnMode {
      get {
        return _tcpConnMode;
      }
      set {
        _tcpConnMode = value;
        NotifyPropertyChanged("TcpConnMode");
      }
    }



    public IEnumerable<string> ConnectionTypeIEnumerable { get; set; }

    public IEnumerable<string> TcpConnModeIEnumerable { get; set; }


    ICommand _SaveCommand;

    public ICommand SaveCommand {
      get {
        if (_SaveCommand == null) {
          _SaveCommand = new RelayCommand(param => this.SaveCommand_Execute(), null);
        }
        return _SaveCommand;
      }
    }

    private void SaveCommand_Execute() {
      JsonWR jsonWR = new JsonWR();
      if (_connType == SendReciver.ConnectionType.Serial.ToString()) {
        jsonWR.WriteSerialJsonFile(_serialFields);
      } else if (_connType == SendReciver.ConnectionType.Tcp.ToString()) {
        jsonWR.WriteTcpJsonFile(_tcpFields);
      }
      ///Save In Db Or SomeWhere Else
    }


  }
}
