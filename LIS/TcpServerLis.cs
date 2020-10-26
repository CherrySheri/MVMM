using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace LIS {
  public class TcpServerLis {

    public delegate void EventHandlerTcpMessage(string message);
    public EventHandlerTcpMessage UpdateTcpMessage;

    public delegate void EventHandlerTcpStatus(string status, bool isConnected);
    public EventHandlerTcpStatus UpdateTcpStatus;

    TcpListener TcpListner { get; set; }

    TcpClient AcceptedClient { get; set; }

    BackgroundWorker bgWorker;

    byte[] ClientMsgArray { get; set; }

    Socket ClientSocket { get; set; }

    string IpAddress { get; set; }

    int Port { get; set; }

    public string LocalIp { get; private set; }

    public string RemoteIp { get; private set; }

    public TcpServerLis(CommunicationFields commFields) {
      IpAddress = commFields.TcpIpAddress;
      Port = commFields.TcpPort;
    }

    public bool IsConnected {
      get {
        return (TcpListner != null && AcceptedClient != null && AcceptedClient.Connected);
      }
    }

    public void StartServer() {
      bgWorker = new BackgroundWorker();
      bgWorker.DoWork += BgWorker_DoWork;
      bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
      if(!bgWorker.IsBusy) {
        bgWorker.RunWorkerAsync();
      }
    }

    private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      Console.WriteLine("Background Worker Completed");
    }

    private void BgWorker_DoWork(object sender, DoWorkEventArgs e) {
      Listen();
    }


    public void StopServer() {
      ClientSocket?.Close();
      AcceptedClient?.Close();
    }


    private void Listen() {
      try {
        IPHostEntry iPHost = Dns.GetHostEntry(IpAddress);
        TcpListner = new TcpListener(iPHost.AddressList[0], Port);
        TcpListner.Start();
        UpdateTcpStatus("listener_start", true);
        for(; ; ) {
          if(AcceptedClient == null) {
            UpdateTcpStatus("Waiting for incoming Connection......", false);
          }
          AcceptedClient = TcpListner.AcceptTcpClient();
          RemoteIp = AcceptedClient.Client.RemoteEndPoint.ToString();
          UpdateTcpStatus("connected", true);
          ClientMsgArray = new byte[1024];
          AcceptedClient.GetStream().BeginRead(ClientMsgArray, 0, 1024, new AsyncCallback(DoReceive), null);
        }
      } catch(Exception ex) {
        Console.WriteLine(ex.Message);
        return;
      }
    }

    private void DoReceive(IAsyncResult asyncResult) {
      try {
        NetworkStream netStream = AcceptedClient.GetStream();
        int noBytesRecv;
        lock(netStream) {
          noBytesRecv = AcceptedClient.GetStream().EndRead(asyncResult);
        }
        if(noBytesRecv < 1) {
          UpdateTcpStatus("Disconnected from Server....", false);
        } else {
          string message = Encoding.Default.GetString(ClientMsgArray, 0, noBytesRecv);
          UpdateTcpMessage(message);
          NetworkStream netStream2 = AcceptedClient.GetStream();
          lock(netStream2) {
            AcceptedClient.GetStream().BeginRead(ClientMsgArray, 0, 1024, new AsyncCallback(DoReceive), null);
          }
        }
      } catch(Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    public void Send(string msgData) {
      if(AcceptedClient == null) {
        byte[] array = Encoding.ASCII.GetBytes(msgData);//new byte[data.Length];
        ClientSocket.BeginSend(array, 0, array.Length, SocketFlags.None, null, null);
        return;
      }
      NetworkStream netStram = AcceptedClient.GetStream();
      lock(netStram) {
        StreamWriter streamWriter = new StreamWriter(AcceptedClient.GetStream(), Encoding.GetEncoding("ISO-8859-1"));
        streamWriter.Write(msgData);
        streamWriter.Flush();
      }
    }
  }
}
