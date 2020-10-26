using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LIS {
  public class JsonWR {

    string GetAppDir {
      get {
        return Directory.GetCurrentDirectory();
      }
    }

    string GetTcpJson {
      get {
        return Path.Combine(GetAppDir, "TcpFields.json");
      }
    }

    string GetSerialJson {
      get {
        return Path.Combine(GetAppDir, "SerialFields.json");
      }
    }


    public void WriteTcpJsonFile(TcpFields tcpFields) {
      DeleteExisingFiles();
      //open file stream
      using (StreamWriter file = File.CreateText(GetTcpJson)) {
        JsonSerializer serializer = new JsonSerializer();
        //serialize object directly into file stream
        serializer.Serialize(file, tcpFields);
      }
    }

    public void WriteSerialJsonFile(SerialFields serialF) {
      DeleteExisingFiles();
      //open file stream
      using (StreamWriter file = File.CreateText(GetSerialJson)) {
        JsonSerializer serializer = new JsonSerializer();
        //serialize object directly into file stream
        serializer.Serialize(file, serialF);
      }
    }

    public SerialFields ReadJsonFile() {
      SerialFields serialF = JsonConvert.DeserializeObject<SerialFields>(File.ReadAllText(GetSerialJson));
      return serialF;
    }

    public void DeleteExisingFiles() {
      if (File.Exists(GetTcpJson)) {
        File.Delete(GetTcpJson);
      }
      if (File.Exists(GetSerialJson)) {
        File.Delete(GetSerialJson);
      }
    }



  }
}
