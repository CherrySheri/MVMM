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
      using (StreamWriter file = File.CreateText(GetCommJsonFile)) {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(file, commFields);
      }
    }

    public CommunicationFields ReadJsonFile() {
      CommunicationFields communicationField = null;
      if (File.Exists(GetCommJsonFile)) {
        communicationField = JsonConvert.DeserializeObject<CommunicationFields>(File.ReadAllText(GetCommJsonFile));
      }
      return communicationField;

    }
  }
}
