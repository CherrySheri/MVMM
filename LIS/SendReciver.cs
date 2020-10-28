using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows.Data;
using System.Threading;
using System.Windows.Navigation;

namespace LIS {
  public class SendReciver {

    #region Private Fields

    private int _frameSendInSeconds { get; set; } = 12;
    private string _sendMsg { get; set; }
    private string _receiveMsg { get; set; }

    private ConnectionType _connType = ConnectionType.Serial;

    private TcpConnectionMode _tcpConnMode = TcpConnectionMode.C;

    private CommunicationFields _communicationFields { get; set; }

    private SerialMessageStatus _serialMsgStatus { get; set; }

    private TcpMesageStatus _tcpMsgStatus { get; set; }

    private System.Threading.Timer _threadTimer;

    private BackgroundWorker _receivedBgWorker;

    private TcpServerLis _tcpServerLis { get; set; }

    private TcpClientLis _tcpClientLis { get; set; }

    private SerialLis _serialLis { get; set; }

    private bool _isConnectedToMachine { get; set; }

    private string CurrentDateTime {
      get {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";
      }
    }

    private LisPatientFields _lisPatFields { get; set; }

    private ClsLog _clsLog { get; set; }



    #endregion Private Fields


    #region Constructor
    
    public SendReciver(CommunicationFields commFields, SerialMessageStatus serialMsgStatus, 
                       TcpMesageStatus messageStatus, LisPatientFields lisPatF) {
      if (commFields != null) {
        _connType = commFields.ConnType;
        if (_connType == ConnectionType.Tcp) {
          _tcpConnMode = _communicationFields.TcpMode;
        }
        _communicationFields = commFields;
        this._serialMsgStatus = serialMsgStatus;
        this._tcpMsgStatus   = messageStatus;
        _lisPatFields = lisPatF;
      }
      _clsLog = new ClsLog();
    }
    #endregion Constructor


    #region Init Component


    public void InitializeConnection() {
      StartTcpSerialConnection();
      StartDispatcherTimer();
      StartBackgroundWorker();
    }

    #region Timer Initialization

    private void StartDispatcherTimer() {
      _threadTimer = new System.Threading.Timer(new TimerCallback(SendMessageAfterParticularDelay),
        null, TimeSpan.FromSeconds(_frameSendInSeconds), TimeSpan.FromSeconds(_frameSendInSeconds));
    }


    private void SendMessageAfterParticularDelay(object state) {
      if (!_isConnectedToMachine || string.IsNullOrEmpty(_sendMsg)) return;
      if (_connType == ConnectionType.Serial) {
        SendSerialMsg();
        _serialLis.SendMessage(_sendMsg);
      } else if (_connType == ConnectionType.Tcp) {
        SendTcpCSMsg();
        if (_tcpConnMode == TcpConnectionMode.C) {
          _tcpClientLis.Send(_sendMsg);
        } else if (_tcpConnMode == TcpConnectionMode.S) {
          _tcpServerLis.Send(_sendMsg);
        }
      }
      if (_sendMsg == EOT) {
        _sendMsg = null;
      }
    }

    #endregion Timer Initialization


    #region BackgroundWorker Initialization
    private void StartBackgroundWorker() {
      if (_receivedBgWorker == null) {
        _receivedBgWorker = new BackgroundWorker();
        _receivedBgWorker.DoWork += ReceivedBgWorker_DoWork;
        _receivedBgWorker.RunWorkerCompleted += ReceivedBgWorker_RunWorkerCompleted;
        if (!_receivedBgWorker.IsBusy) {
          _receivedBgWorker.RunWorkerAsync();
        }
      }
    }

    private void ReceivedBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      
    }

    private void ReceivedBgWorker_DoWork(object sender, DoWorkEventArgs e) {
      for (; ; ) {
        if (!_isConnectedToMachine) continue;
        if (string.IsNullOrEmpty(_receiveMsg)) continue;
        if (_receiveMsg.Contains(ENQ)) {
          _sendMsg = ACK;
        } else if (_receiveMsg.Contains(STX)) {
          if (_receiveMsg.Contains(CR) || _receiveMsg.Contains(LF)) {
            _sendMsg = ACK;
          } else if (_receiveMsg.Contains(GS)) {
            _sendMsg = ACK;
          } else if (_receiveMsg.Contains(ETX) || _receiveMsg.Contains(ETB)) {
            _sendMsg = ACK;
          } else if (_receiveMsg.Contains("<!--:End:Chksum:1:")) {
            _sendMsg = ACK; _receiveMsg = null;
          }
        } else if (_receiveMsg.Contains(EOT)) {
          _sendMsg = null; _receiveMsg = null;
        }
        Thread.Sleep(TimeSpan.FromSeconds(_frameSendInSeconds));
      }
    }

    #endregion BackgroundWorker Initialization


    #region TcpSerial Connection Init

    public void StartTcpSerialConnection() {
      if (_communicationFields == null) return;
      if (_connType == ConnectionType.Serial) {
        _serialLis = new SerialLis(_communicationFields);
        _serialLis.UpdateSerialMessage += new SerialLis.EventHandlerSerialMessage(UpdateSerialMessage);
        _serialLis.UpdateSerialStatus += new SerialLis.EventHandlerSerialStatus(UpdateSerialStatus);
        _serialLis.StartSerialPort();
      } else if (_connType == ConnectionType.Tcp) {
        if (_tcpConnMode == TcpConnectionMode.C) {
          _tcpClientLis = new TcpClientLis(_communicationFields);
          _tcpClientLis.UpdateTcpMessage += new TcpClientLis.EventHandlerTcpMessage(UpdateTcpClientMessage);
          _tcpClientLis.UpdateTcpStatus += new TcpClientLis.EventHandlerTcpStatus(UpdateTcpClientStatus);
          _tcpClientLis.StartClient();
        } else if (_tcpConnMode == TcpConnectionMode.S) {
          _tcpServerLis = new TcpServerLis(_communicationFields);
          _tcpServerLis.UpdateTcpMessage += new TcpServerLis.EventHandlerTcpMessage(UpdateTcpServerMessage);
          _tcpServerLis.UpdateTcpStatus += new TcpServerLis.EventHandlerTcpStatus(UpdateTcpServerStatus);
          _tcpServerLis.StartServer();
        }
      }
    }

    #endregion TcpSerial Connection Init


    #endregion Init Component


    #region Tcp Server Msg And Status

    private void UpdateTcpServerMessage(string message) {
      _receiveMsg = message;
      lock(_tcpMsgStatus._messageLock) {
        string msg = CurrentDateTime + "R(" + message.Length + "){" + ConvertCtrlCharToStr(message) + "}";
        _clsLog.WriteTcpMessgeLog(msg);
        _tcpMsgStatus.MessageCollection.Add(msg);
      }
    }

    private void UpdateTcpServerStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
      lock (_tcpMsgStatus._statusLock) {
        _clsLog.WriteTcpStatusLog(status);
        _tcpMsgStatus.StatusCollection.Add(status);
      }
    }

    #endregion Tcp Server Msg And Status


    #region Tcp Client Msg And Status

    private void UpdateTcpClientMessage(string message) {
      _receiveMsg = message;
      lock (_tcpMsgStatus._messageLock) {
        string msg = CurrentDateTime + "R(" + message.Length + "){" + ConvertCtrlCharToStr(message) + "}";
        _clsLog.WriteTcpMessgeLog(msg);
        _tcpMsgStatus.MessageCollection.Add(msg);
      }
    }

    private void UpdateTcpClientStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
      lock (_tcpMsgStatus._statusLock) {
        _clsLog.WriteTcpStatusLog(status);
        _tcpMsgStatus.StatusCollection.Add(status);
      }
    }

    private void SendTcpCSMsg() {
      lock (_tcpMsgStatus._messageLock) {
        string message = CurrentDateTime + "S(" + _sendMsg.Length + "){" + ConvertCtrlCharToStr(_sendMsg) + "}";
        _clsLog.WriteTcpMessgeLog(message);
        _tcpMsgStatus.MessageCollection.Add(message);
      }
    }

    #endregion Tcp Client Msg And Status


    #region Serial Msg And Status
    
    private void UpdateSerialMessage(string message) {
      _receiveMsg = message;
      lock (_serialMsgStatus._messgeLock) {
        string msg = CurrentDateTime + "R(" + message.Length + "){" + ConvertCtrlCharToStr(message) + "}";
        _clsLog.WriteSerialMsgLog(msg);
        _serialMsgStatus.MessageCollection.Add(ConvertCtrlCharToStr(msg));
      }
    }

    private void SendSerialMsg() {
      lock (_serialMsgStatus._messgeLock) {
        string message = CurrentDateTime + "S(" + _sendMsg.Length + "){" + ConvertCtrlCharToStr(_sendMsg) + "}";
        _clsLog.WriteSerialMsgLog(message);
        _serialMsgStatus.MessageCollection.Add(message);
      }
    }


    private void UpdateSerialStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
      lock (_serialMsgStatus._statusLock) {
        _clsLog.WriteSerialMsgLog(status);
        _serialMsgStatus.StatusCollection.Add(status);
      }
    }

    #endregion Serial Msg And Status


    #region SendOrder To Machine

    public void SendOrderToMachine() {
      StartBackgroundWorker();
      BackgroundWorker SenMsgBgWork = new BackgroundWorker();
      SenMsgBgWork.DoWork += SendMsgBgWork_DoWork;
      SenMsgBgWork.RunWorkerCompleted += SendMsgRunWork_Completed;
      if (!SenMsgBgWork.IsBusy) {
        SenMsgBgWork.RunWorkerAsync();
      }
    }

    private void SendMsgRunWork_Completed(object sender, RunWorkerCompletedEventArgs e) {
      
    }

    private void SendMsgBgWork_DoWork(object sender, DoWorkEventArgs e) {
      int ri = 0;
      List<string> recordArray = GetOrderList(); //GetOrderFromSpectrum();
      _sendMsg = ENQ;
      while (ri < recordArray.Count) {
        if (!_isConnectedToMachine) continue;
        if (_receiveMsg == ACK) {
          _sendMsg = recordArray[ri];
          _receiveMsg = "";
          ri++;
        } else if (_receiveMsg == NAK) {
          _sendMsg = null; _receiveMsg = null;
          break;
        }
        Thread.Sleep(TimeSpan.FromSeconds(12));
      }
      if (_sendMsg.Contains("L")) {
        _sendMsg = EOT; _receiveMsg = "";
      }
    }

    private List<string> GetOrderFromSpectrum() {
      SpectrumAnalyzer spectrum = new SpectrumAnalyzer(_lisPatFields);
      List<string> orderList = new List<string>();
      if (_lisPatFields.OrderType.ToLower() == "o") {
        orderList = spectrum.GetOrderRecord();
      } else if (_lisPatFields.OrderType.ToLower() == "q") {
        orderList = spectrum.GetRequestAnalysis();
      }
      int recordLevel = 1;
      for (int oi = 0; oi < orderList.Count; oi++) {
        if (recordLevel == 0) { recordLevel = 1; }
        string order = STX + recordLevel + orderList[oi] + CR + ETX + CR + LF;
        string checksum = spectrum.CalculateChcksum(order);
        order = STX + recordLevel + orderList[oi] + CR + ETX + checksum + CR + LF;
        orderList[oi] = order; recordLevel++;
        if (recordLevel == 8) {
          recordLevel = 0;
        }
      }
      return orderList;
    }


    private List<string> GetOrderList() {
      List<string> orderList = new List<string>();
      if (_communicationFields.PathoMachine == PathoMachine.SpectrumAnalyzer) {
        orderList = GetOrderFromSpectrum();
      }
      return orderList;
    }
    



    #endregion SendOrder To Machine


    #region Communication Protocol


    private string ConvertCtrlCharToStr(string ctrlChar) {
      ctrlChar = ctrlChar.Replace(SOH, "<SOH>");
      ctrlChar = ctrlChar.Replace(STX, "<STX>");
      ctrlChar = ctrlChar.Replace(ETX, "<ETX>");
      ctrlChar = ctrlChar.Replace(EOT, "<EOT>");
      ctrlChar = ctrlChar.Replace(ENQ, "<ENQ>");
      ctrlChar = ctrlChar.Replace(ACK, "<ACK>");
      ctrlChar = ctrlChar.Replace(LF, "<LF>");
      ctrlChar = ctrlChar.Replace(CR, "<CR>");
      ctrlChar = ctrlChar.Replace(DLE, "<DLE>");
      ctrlChar = ctrlChar.Replace(NAK, "<NAK>");
      //frame = frame.Replace(SYN, "<SYN>");
      ctrlChar = ctrlChar.Replace(ETB, "<ETB>");
      //frame = frame.Replace(CAN, "<CAN>");
      //frame = frame.Replace(SB, "<SB>");
      //frame = frame.Replace(EB, "<EB>");
      //frame = frame.Replace(DEL, "<DEL>");
      //frame = frame.Replace(DC1, "<DC1>");
      //frame = frame.Replace(DC2, "<DC2>");
      //frame = frame.Replace(DC3, "<DC3>");
      //frame = frame.Replace(DC4, "<DC4>");
      ctrlChar = ctrlChar.Replace("\t", "<TAB>");
      ctrlChar = ctrlChar.Replace(FS, "<FS>");
      ctrlChar = ctrlChar.Replace(GS, "<GS>");
      string controlChar = ctrlChar.Replace(RS, "<RS>");
      return controlChar;
    }

    readonly string SOH = char.ConvertFromUtf32(1);
    readonly string STX = char.ConvertFromUtf32(2);
    readonly string ETX = char.ConvertFromUtf32(3);
    readonly string EOT = char.ConvertFromUtf32(4);
    readonly string ENQ = char.ConvertFromUtf32(5);
    readonly string ACK = char.ConvertFromUtf32(6);
    readonly string LF = char.ConvertFromUtf32(10);
    readonly string CR = char.ConvertFromUtf32(13);
    readonly string DLE = char.ConvertFromUtf32(16);
    readonly string NAK = char.ConvertFromUtf32(21);
    readonly string ETB = char.ConvertFromUtf32(23);
    readonly string SUB = char.ConvertFromUtf32(26);
    readonly string ESC = char.ConvertFromUtf32(27);
    readonly string FS = char.ConvertFromUtf32(28);
    readonly string GS = char.ConvertFromUtf32(29);
    readonly string RS = char.ConvertFromUtf32(30);

    #endregion Communication Protocol


    public enum ConnectionType {
      Serial,
      Tcp
    }

    public enum TcpConnectionMode {
      C,
      S
    }

    public enum PathoMachine {
      SpectrumAnalyzer
    }

    public enum OrderType {
      Q,
      O
    }

    

  }

  [Serializable]
  public class CommunicationFields {
    public string BaudRate { get; set; }
    public string Port { get; set; }
    public string Parity { get; set; }
    public string DataBit { get; set; }
    public string StopBit { get; set; }
    public string FlowControl { get; set; }

    public List<string> BaudRateList = new List<string>() { "300", "600", "1200", "2400", "9600", "14400", "19200", "38400", "57600", "115200" };
    public List<string> DataBitList = new List<string>() { "4", "5", "6", "7", "8" };
    public List<string> FlowControlList = new List<string>() { "None", "X-ON/X-Off", "Hardware" };
    public List<string> ParityList = Enum.GetNames(typeof(System.IO.Ports.Parity)).ToList();
    public List<string> StopBitList = Enum.GetNames(typeof(System.IO.Ports.StopBits)).ToList();

    public string TcpIpAddress { get; set; }
    public int TcpPort { get; set; }

    public SendReciver.TcpConnectionMode TcpMode { get; set; }

    public SendReciver.ConnectionType ConnType { get; set; }

    public SendReciver.PathoMachine PathoMachine { get; set; }


  }

  public class SerialMessageStatus {
    public SerialMessageStatus() {
      
      MessageCollection = new ObservableCollection<string>();
      BindingOperations.EnableCollectionSynchronization(MessageCollection, _messgeLock);
      StatusCollection = new ObservableCollection<string>();
      BindingOperations.EnableCollectionSynchronization(StatusCollection, _statusLock);
    }

    public object _messgeLock = new object();
    public object _statusLock = new object();

    public ObservableCollection<string> MessageCollection { get; set; }
    public ObservableCollection<string> StatusCollection { get; set; }

  }

  public class TcpMesageStatus {

    public TcpMesageStatus() {
      MessageCollection = new ObservableCollection<string>();
      BindingOperations.EnableCollectionSynchronization(MessageCollection, _messageLock);
      StatusCollection = new ObservableCollection<string>();
      BindingOperations.EnableCollectionSynchronization(StatusCollection, _statusLock);
    }

    public object _messageLock = new object();
    public object _statusLock = new object();

    public ObservableCollection<string> MessageCollection { get; set; }
    public ObservableCollection<string> StatusCollection { get; set; }
  }


  public class LisPatientFields {

    public string PatientId { get; set; }

    public string PatFName { get; set; }

    public string PatMName { get; set; }

    public string PatLName { get; set; }

    public DateTime DOB { get; set; }

    public string Gender { get; set; }

    public string SampleId { get; set; }

    public string MethoId { get; set; }

    public string TestName { get; set; }

    public DateTime BgnTestDateTime { get; set; } = DateTime.Now;

    public DateTime EndTestDateTime { get; set; } = DateTime.Now;

    public string OrderType { get; set; }

    public enum GenderEnum {
      M,
      F,
      O
    }


  }
}
