using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LIS {
  public class SerialLis {


    public delegate void EventHandlerSerialMessage(string message);
    public EventHandlerSerialMessage UpdateSerialMessage;

    public delegate void EventHandlerSerialStatus(string status, bool isConnected);
    public EventHandlerSerialStatus UpdateSerialStatus;


    SerialPort Serial { get; set; }
    string BaudRate { get; set; }
    string SerialPort { get; set; }
    string SerialParity { get; set; }
    string SerialDataBit { get; set; }
    string SerialStopBit { get; set; }
    string SerialFlowControl { get; set; }

    public SerialLis(CommunicationFields serialFields) {
      BaudRate = serialFields.BaudRate; 
      SerialPort = serialFields.Port; 
      SerialParity = serialFields.Parity;
      SerialDataBit = serialFields.DataBit; 
      SerialStopBit = serialFields.StopBit; 
      SerialFlowControl = serialFields.FlowControl;
    }

    public bool IsConnected {
      get {
        return (Serial != null && Serial.IsOpen);
      }
    }

    public bool StartSerialPort() {
      bool isSerialStarted = false;
      try {
        Serial = new SerialPort();
        Serial.PinChanged += Serial_PinChanged;
        Serial.ErrorReceived += Serial_ErrorReceived;
        Serial.DataReceived += Serial_DataReceived;
        Serial.BaudRate = int.Parse(BaudRate);
        Serial.PortName = "COM" + SerialPort;
        Enum.TryParse(SerialParity, out Parity p);
        Serial.Parity = p;
        Serial.DataBits = int.Parse(SerialDataBit);
        Enum.TryParse(SerialStopBit, out StopBits sb);
        Serial.StopBits = sb;
        Serial.DtrEnable = Serial.RtsEnable = true;
        Serial.Encoding = Encoding.GetEncoding("ISO-8859-1");
        Serial.Open();
        if (Serial.DsrHolding) {
          UpdateSerialStatus("connected", true);
          isSerialStarted = true;
        } else if (SerialFlowControl == "0") {
          UpdateSerialStatus("connected", true);
          isSerialStarted = true;
        } else {
          UpdateSerialStatus("offline", false);
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
      return isSerialStarted;
    }

    public void StopSerialPort() {
      if(Serial != null) {
        Serial.Close();
        Serial.Dispose();
      }
    }

    private Parity GetParity() {
      if(SerialParity.ToString().ToUpper() == "N") {
        return Parity.None;
      } else if(SerialParity.ToString().ToUpper() == "O") {
        return Parity.Odd;
      } else if(SerialParity.ToString().ToUpper() == "E") {
        return Parity.Even;
      } else if(SerialParity.ToString().ToUpper() == "M") {
        return Parity.Mark;
      } else if(SerialParity.ToString().ToUpper() == "S") {
        return Parity.Space;
      } else {
        return Parity.Even;
      }
    }

    public void SendMessage(string message) {
      if(Serial != null) {
        Serial.Write(message);
      }
    }

    private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e) {
      UpdateSerialMessage(Serial.ReadExisting());
    }

    private void Serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
      string error = "";
      switch(e.EventType) {
        case SerialError.TXFull:
          error = "Output buffer was full";
          break;
        case SerialError.RXOver:
          error = "Input buffer overflow has occurred";
          break;
        case SerialError.Overrun:
          error = "Data Lost";
          break;
        case SerialError.RXParity:
          error = "Hardware detected a parity error";
          break;
        case SerialError.Frame:
          error = "Hardware detected a framing error";
          break;
        default:
          break;
      }
      if(error != "") {
        UpdateSerialStatus("Serial_ErrorReceived" + 2555 + error, false);
      }
    }

    private void Serial_PinChanged(object sender, SerialPinChangedEventArgs e) {
      if (e.EventType == SerialPinChange.DsrChanged) {
        Thread.Sleep(500);
        if (!Serial.IsOpen) {
          return;
        }
        if (SerialFlowControl != "0") {
          
          UpdateSerialStatus("[COMM]Serial connection closed! " + "COM" + SerialPort, false);
        } else {
          UpdateSerialStatus("[COMM]Serial connection started" + "COM" + SerialPort, true);
        }
      }
    }
  }
}
