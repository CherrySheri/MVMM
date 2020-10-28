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


    
    private CommunicationFields _commFields;
    private JsonWR _jsonWR;


    public LisSettingVM() {
      _jsonWR = new JsonWR();
      _commFields =_jsonWR.ReadJsonFile();
      if (_commFields == null) {
        _commFields = new CommunicationFields();
      }
      ConnectionTypeIEnumerable = Enum.GetNames(typeof(SendReciver.ConnectionType));
      TcpConnModeIEnumerable = Enum.GetNames(typeof(SendReciver.TcpConnectionMode));
      PathoMachineIEnumberable = Enum.GetNames(typeof(SendReciver.PathoMachine));
    }

    public CommunicationFields CommFields {
      get {
        return _commFields;
      }
      set {
        _commFields = value;
        NotifyPropertyChanged("CommFields");

      }
    }

    public List<string> BaudRateList {
      get {
        return _commFields.BaudRateList;
      }
      set {
        _commFields.BaudRateList = value;
        NotifyPropertyChanged("BaudRateList");
      }
    }



    public List<string> DataBitList {
      get {
        return _commFields.DataBitList;
      }
      set {
        _commFields.DataBitList = value;
        NotifyPropertyChanged("DataBitList");
      }
    }

    public List<string> FlowControlList {
      get {
        return _commFields.FlowControlList;
      }
      set {
        _commFields.FlowControlList = value;
        NotifyPropertyChanged("FlowControlList");
      }
    }

    public List<string> ParityList {
      get {
        return _commFields.ParityList;
      }
      set {
        _commFields.ParityList = value;
        NotifyPropertyChanged("ParityList");
      }
    }
    public List<string> StopBitList {
      get {
        return _commFields.StopBitList;
      }
      set {
        _commFields.StopBitList = value;
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







    public IEnumerable<string> ConnectionTypeIEnumerable { get; set; }

    public IEnumerable<string> TcpConnModeIEnumerable { get; set; }

    public IEnumerable<string> PathoMachineIEnumberable { get; set; }



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
      _jsonWR.WriteSerialJsonFile(_commFields);
    }


  }
}
