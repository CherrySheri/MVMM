using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIS {
  public class ClsLog {

    public ClsLog() {
      CreateLog();
    }

    private string CurrentDate {
      get {
        return DateTime.Now.ToString("yyyyMMdd");
      }
    }

    private string currentDir {
      get {
        return Directory.GetCurrentDirectory();
      }
    }

    private string lisLogFolder {
      get {
        return Path.Combine(currentDir, "lisLogFolder");
      }
    }

    private string serialMsgLog { get; set; }
    private string serialStatusLog { get; set; }

    private string tcpMsgLog { get; set; }
    private string tcpStatusLog { get; set; }

    private object lockObj = new object();

    private void CreateLog() {
      if (!Directory.Exists(lisLogFolder)) {
        Directory.CreateDirectory(lisLogFolder);
      }
      serialMsgLog   = Path.Combine(lisLogFolder, "SerialMsg_" + CurrentDate + ".txt");
      serialStatusLog = Path.Combine(lisLogFolder, "SerialStatus_" + CurrentDate + ".txt");
      tcpMsgLog = Path.Combine(lisLogFolder, "TcpMsg_" + CurrentDate + ".txt");
      tcpStatusLog = Path.Combine(lisLogFolder, "TcpStatus_" + CurrentDate + ".txt");
    }



    private void Write(string logPath, string message) {
      lock (lockObj) {
        File.AppendAllLines(logPath, new string[] { message + "\t\n" });
      }
    }

    #region SerialMsg and Status Log

    
    public void WriteSerialMsgLog(string message) {
      if (!File.Exists(serialMsgLog)) {
        FileStream fs = File.Create(serialMsgLog);
        fs.Close();
      }
      Write(serialMsgLog, message);
    }

    public void WriteSerialStatusLog(string message) {
      if (!File.Exists(serialStatusLog)) {
        FileStream fs = File.Create(serialStatusLog);
        fs.Close();
      }

      Write(serialStatusLog, message);
    }

    #endregion SerialMsg and Status Log


    #region Tcp Message and Status Log

    public void WriteTcpMessgeLog(string message) {
      if (!File.Exists(tcpMsgLog)) {
        FileStream fs = File.Create(tcpMsgLog);
        fs.Close();
      }
      Write(tcpMsgLog, message);
    }

    public void WriteTcpStatusLog(string message) {
      if (!File.Exists(tcpStatusLog)) {
        FileStream fs = File.Create(tcpStatusLog);
        fs.Close();
      }

      Write(tcpStatusLog, message);
    }

    #endregion Tcp Message and Status Log


  }
}
