using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;


namespace LIS {
  public class TcpClientLis {

    public delegate void EventHandlerTcpMessage(string message);
    public EventHandlerTcpMessage UpdateTcpMessage;

    public delegate void EventHandlerTcpStatus(string status, bool isConnected);
    public EventHandlerTcpStatus UpdateTcpStatus;


    BackgroundWorker bgWorker;
    private byte[] ClientMessageArr { get; set; }
    TcpClient Client { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }


    public TcpClientLis(CommunicationFields commFields) {
      IpAddress = commFields.TcpIpAddress; 
      Port = commFields.TcpPort;
    }

    public void StartClient() {
      bgWorker = new BackgroundWorker();
      bgWorker.DoWork += BgWorker_DoWork;
      bgWorker.ProgressChanged += BgWorker_ProgressChanged;
      bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
      if(!bgWorker.IsBusy) {
        bgWorker.RunWorkerAsync();
      }
    }

    private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      if(e.Result.ToString().ToLower() == "offline") {
        UpdateTcpStatus("server_offline", false);
      } else {
        Client.GetStream().BeginRead(ClientMessageArr, 0, 1024, new AsyncCallback(DoListen), null);
      }
    }

    private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      UpdateTcpStatus("connected", true);
    }

    private void BgWorker_DoWork(object sender, DoWorkEventArgs e) {
      try {
        Client = new TcpClient();
        bgWorker.ReportProgress(100);
      } catch(Exception ex) {
        e.Result = "offline";
        Console.WriteLine(ex.Message);
      }
    }

    private void DoListen(IAsyncResult asyncResult) {
      try {
        int bytesRecv = Client.GetStream().EndRead(asyncResult);
        if(bytesRecv < 1) {
          MarkAsDisconnected();
        } else {
          string message = Encoding.Default.GetString(ClientMessageArr, 0, bytesRecv);
          UpdateTcpMessage(message);
          Client.GetStream().BeginRead(ClientMessageArr, 0, 1024, new AsyncCallback(this.DoListen), null);
        }
      } catch(ObjectDisposedException ex) {
        Console.WriteLine(ex.Message);
        MarkAsDisconnected();
      } catch(Exception ex2) {
        Console.WriteLine(ex2.Message);
        MarkAsDisconnected();
      }
    }


    private void MarkAsDisconnected() {
      UpdateTcpStatus("disconnect_from_server", false);
    }

    public void StopClient() {
      Client?.Close();
    }


    public void Send(string psMessage) {
      StreamWriter streamWriter;
      streamWriter = new StreamWriter(Client.GetStream(), Encoding.GetEncoding("ISO-8859-1"));
      streamWriter.Write(psMessage);
      streamWriter.Flush();
    }


  }
}
