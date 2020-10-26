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

    string GetCommJsonFile {
      get {
        return Path.Combine(GetAppDir, "Communication.json");
      }
    }

    public void WriteSerialJsonFile(CommunicationFields commFields) {
      if (File.Exists(GetCommJsonFile)) {
        File.Delete(GetCommJsonFile);
      }
      //open file stream
      using (StreamWriter file = File.CreateText(GetCommJsonFile)) {
        JsonSerializer serializer = new JsonSerializer();
        //serialize object directly into file stream
        serializer.Serialize(file, commFields);
      }
    }

    public CommunicationFields ReadJsonFile() {
      CommunicationFields serialF = JsonConvert.DeserializeObject<CommunicationFields>(File.ReadAllText(GetCommJsonFile));
      return serialF;
    }
  }
}
