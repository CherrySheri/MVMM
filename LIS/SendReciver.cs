using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace LIS {
  public class SendReciver {


    public int FrmeRetryInSeconds { get; set; } = 12;
    private int _FrameSendIntervalInSeconds { get; set; } = 1;
    public CommunicationFields CommFields { get; set; }

    private System.Threading.Timer Timer;
    BackgroundWorker _ReceivedBgWorker;

    private string _SenderMsg { get; set; }
    private string _ReceivedMsg { get; set; }

    private ConnectionType _ConnectionType = ConnectionType.Serial;
    private TcpConnectionMode _TcpConnMode = TcpConnectionMode.C;

    private TcpServerLis _TcpServerLis { get; set; }
    private TcpClientLis _TcpClientLis { get; set; }
    private SerialLis _SerialLis { get; set; }

    private bool _isConnectedToMachine { get; set; } 


    public SendReciver(CommunicationFields commFields) {
      if (commFields != null) {
        _ConnectionType = commFields.ConnType;
        if (_ConnectionType == ConnectionType.Tcp) {
          _TcpConnMode = CommFields.TcpMode;
        }
        CommFields = commFields;
        StartConnection();
        StartDispatcherTimer();
        StartBackgroundWorker();
      }
    }

    #region DispatcherTimer Initialization
    private void StartDispatcherTimer() {
      Timer = new System.Threading.Timer(new TimerCallback(SendMessageAfterParticularDelay),
        null, TimeSpan.FromSeconds(_FrameSendIntervalInSeconds),
        TimeSpan.FromSeconds(_FrameSendIntervalInSeconds));
    }


    private void SendMessageAfterParticularDelay(object state) {
      if (!_isConnectedToMachine) return;
      if (_ConnectionType == ConnectionType.Serial) {
        _SerialLis.SendMessage(_SenderMsg);
      } else if (_ConnectionType == ConnectionType.Tcp) {
        if (_TcpConnMode == TcpConnectionMode.C) {
          _TcpClientLis.Send(_SenderMsg);
        } else if (_TcpConnMode == TcpConnectionMode.S) {
          _TcpServerLis.Send(_SenderMsg);
        }
      }
    }

    #endregion DispatcherTimer Initialization

    #region BackgroundWorker Initialization
    private void StartBackgroundWorker() {
      _ReceivedBgWorker = new BackgroundWorker();
      _ReceivedBgWorker.DoWork += ReceivedBgWorker_DoWork;
      _ReceivedBgWorker.RunWorkerCompleted += ReceivedBgWorker_RunWorkerCompleted;
      if (!_ReceivedBgWorker.IsBusy) {
        _ReceivedBgWorker.RunWorkerAsync();
      }
    }

    private void ReceivedBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      throw new Exception("Run Worker Completed");
    }

    private void ReceivedBgWorker_DoWork(object sender, DoWorkEventArgs e) {
      for (; ; ) {
        if (!_isConnectedToMachine) continue;
        if (string.IsNullOrEmpty(_ReceivedMsg)) continue;
        if (_ReceivedMsg.Contains(ENQ)) {
          _SenderMsg = ACK; //_SendDispatcherTimer.Start();
          SendMeesageToMachine();
        } else if (_ReceivedMsg.Contains(STX)) {
          if (_ReceivedMsg.Contains(CR) || _ReceivedMsg.Contains(LF)) {
            _SenderMsg = ACK; SendMeesageToMachine(); //_SendDispatcherTimer.Start();
          } else if (_ReceivedMsg.Contains(GS)) {
            _SenderMsg = ACK; SendMeesageToMachine();//_SendDispatcherTimer.Start();
          } else if (_ReceivedMsg.Contains(ETX) || _ReceivedMsg.Contains(ETB)) {
            _SenderMsg = ACK; SendMeesageToMachine();//_SendDispatcherTimer.Start();
          } else if (_ReceivedMsg.Contains("<!--:End:Chksum:1:")) {
            _SenderMsg = ACK; SendMeesageToMachine();//_SendDispatcherTimer.Start();
          } else if (_ReceivedMsg.Contains(EOT)) {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            break;
          }
        }
      }
    }

    #endregion BackgroundWorker Initialization

    public void StartConnection() {
      if (_ConnectionType == ConnectionType.Serial) {
        _SerialLis = new SerialLis(CommFields);
        _SerialLis.UpdateSerialMessage += new SerialLis.EventHandlerSerialMessage(UpdateSerialMessage);
        _SerialLis.UpdateSerialStatus += new SerialLis.EventHandlerSerialStatus(UpdateSerialStatus);
        _SerialLis.StartSerialPort();
      } else if (_ConnectionType == ConnectionType.Tcp) {
        if (_TcpConnMode == TcpConnectionMode.C) {
          _TcpClientLis = new TcpClientLis(CommFields);
          _TcpClientLis.UpdateTcpMessage += new TcpClientLis.EventHandlerTcpMessage(UpdateTcpClientMessage);
          _TcpClientLis.UpdateTcpStatus += new TcpClientLis.EventHandlerTcpStatus(UpdateTcpClientStatus);
          _TcpClientLis.StartClient();
        } else if (_TcpConnMode == TcpConnectionMode.S) {
          _TcpServerLis = new TcpServerLis(CommFields);
          _TcpServerLis.UpdateTcpMessage += new TcpServerLis.EventHandlerTcpMessage(UpdateTcpServerMessage);
          _TcpServerLis.UpdateTcpStatus += new TcpServerLis.EventHandlerTcpStatus(UpdateTcpServerStatus);
          _TcpServerLis.StartServer();
        }
      }
    }

    #region Tcp Server Msg And Status

    private void UpdateTcpServerMessage(string message) {
      
    }

    private void UpdateTcpServerStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
    }

    #endregion Tcp Server Msg And Status


    #region Tcp Client Msg And Status

    private void UpdateTcpClientMessage(string message) {
      
    }

    private void UpdateTcpClientStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
    }

    #endregion Tcp Client Msg And Status


    #region Serial Msg And Status

    private void UpdateSerialMessage(string message) {
      _ReceivedMsg = message;
    }

    private void UpdateSerialStatus(string status, bool isConnected) {
      _isConnectedToMachine = isConnected;
    }

    #endregion Serial Msg And Status

    private void SendMeesageToMachine() {
      //if (_ConnectionType == ConnectionType.Serial) {
      //  _SerialLis.SendMessage(_SenderMsg);
      //} else if (_ConnectionType == ConnectionType.Tcp) {
      //  if (_TcpConnMode == TcpConnectionMode.C) {
      //    _TcpClientLis.Send(_SenderMsg);
      //  } else if (_TcpConnMode == TcpConnectionMode.S) {
      //    _TcpServerLis.Send(_SenderMsg);
      //  }
      //}
    }

    public void SendOrderToMachine() {
      int ri = 0;
      List<string> recordArray = GetOrderFromSpectrum();
      _SenderMsg = ENQ; 
      while (ri < recordArray.Count) {
        if (!_isConnectedToMachine) continue;
        if (_ReceivedMsg == ACK) {
          _SenderMsg = recordArray[ri];
          _ReceivedMsg = "";
          ri++;
        } else if (_ReceivedMsg == NAK) {
          break;
        }
        Thread.Sleep(TimeSpan.FromSeconds(_FrameSendIntervalInSeconds));
      }
      Timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private List<string> GetOrderFromSpectrum() {
      SpectrumAnalyzer spectrum = new SpectrumAnalyzer();
      List<string> orderList = spectrum.GetOrders();
      for (int oi = 0; oi < orderList.Count; oi++) {
        string order = STX + orderList[oi] + CR + ETX + CR + LF;
        string checksum = spectrum.CalculateChcksum(order);
        order = STX + orderList[oi] + CR + ETX + checksum + CR + LF;
        orderList[oi] = order;
      }
      return orderList;
    }

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


  }
}
