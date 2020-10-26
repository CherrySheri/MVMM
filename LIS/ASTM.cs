using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LIS {
  public abstract class ASTM {

    protected string HeaderR {
      get {
        return "H";
      }
    }

    protected string PatientR {
      get {
        return "P";
      }
    }

    protected string OrederR {
      get {
        return "O";
      }
    }

    protected string ResultR {
      get {
        return "R";
      }
    }

    protected string CommentR {
      get {
        return "C";
      }
    }

    protected string RequestR {
      get {
        return "Q";
      }
    }

    protected string TerminatorR {
      get {
        return "L";
      }
    }


    protected string Delimeter {
      get {
        return @"\^&";
      }
    }

    protected string Sepreator {
      get {
        return "|";
      }
    }


    public abstract string CreateHeaderRecord();
    public abstract string CreatePatientRecord();
    public abstract string CreateOrderRecord();
    public abstract string CreateResultRecord();
    public abstract string CreateCommentRecord();
    public abstract string CreateRequestRecord();
    public abstract string CreateTerminatorRecord();
    public abstract string CalculateChcksum(string frame);


    public string PatFName {get;set;}
    public string PatMName { get; set; }
    public string PatLName { get; set; }
    public string DOB { get; set; }
    public string Gender { get; set; }
    public string TestName { get; set; }
    public string BgnTestDateTime { get; set; }
    public string EndTestDateTime { get; set; }

  }

  public class SpectrumAnalyzer : ASTM {

    public override string CalculateChcksum(string frame) {
      string checkSum = "00";
      int byteVal = 0;
      int sumOfChars = 0;
      bool complete = false;
      for (int fi = 0; fi < frame.Length; fi++) {
        byteVal = Convert.ToInt32(frame[fi]);
        switch (byteVal) {
          case 2:
            sumOfChars = 0;
            break;
          case 3:
            sumOfChars += byteVal;
            complete = true;
            break;
          default:
            sumOfChars += byteVal;
            break;
        }
        if (complete)
          break;
      }
      if (sumOfChars > 0) {
        //hex value mod 256 is checksum, return as hex value in upper case
        checkSum = Convert.ToString(sumOfChars % 256, 16).ToUpper();
      }

      //if checksum is only 1 char then prepend a 0
      return (string)(checkSum.Length == 1 ? "0" + checkSum : checkSum);
    }

    public override string CreateCommentRecord() {
      return "";
    }

    public override string CreateHeaderRecord() {
      string header = "1" + HeaderR + Sepreator + Delimeter + Sepreator;
      return header;
    }

    public override string CreateOrderRecord() {
      return "";
    }

    public override string CreatePatientRecord() {
      return "";
    }

    public override string CreateRequestRecord() {
      string reqInfo = "2" + RequestR + Sepreator + "1" + Sepreator + "ALL" + Sepreator + Sepreator
                      + "ALL" + Sepreator + Sepreator + DateTime.Now.ToString("yyyyMMdd") 
                      + "000000" 
                      + Sepreator + DateTime.Now.ToString("yyyyMMdd") + "000000";
      return reqInfo;
    }

    public override string CreateResultRecord() {
      return "";
    }

    public override string CreateTerminatorRecord() {
      string terminator = "3" + TerminatorR +Sepreator + "1" + Sepreator + "c";
      return terminator;
    }


    public List<string> GetOrders() {
      string header = CreateHeaderRecord();
      string request = CreateRequestRecord();
      string terminator = CreateTerminatorRecord();

      List<string> orderList = new List<string>();
      orderList.Add(header);
      orderList.Add(request);
      orderList.Add(terminator);
      return orderList;
    }


  }

}
