using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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

    protected string AppendOperator {
      get {
        return "^";
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


   

  }

  public class SpectrumAnalyzer : ASTM {

    LisPatientFields _lisPatFields { get; set; }

    public SpectrumAnalyzer(LisPatientFields lisPatFields) {
      _lisPatFields = lisPatFields;
    }

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
      string header = HeaderR + Sepreator + Delimeter + Sepreator;
      return header;
    }

    public override string CreateOrderRecord() {
      string testName = _lisPatFields.TestName;
      string order = OrederR + "1" + Sepreator + "12345" + Sepreator + Sepreator +
                    AppendOperator + AppendOperator + AppendOperator + testName + Sepreator + Sepreator + Sepreator
                    + DateTime.Now.ToString("yyyyMMdd") + Sepreator + Sepreator + Sepreator + Sepreator + "A" +
                    Sepreator + Sepreator + Sepreator + Sepreator + "Raro Color" +
                    Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + Sepreator +
                    Sepreator + Sepreator + "Q";

      return order;
    }

    public override string CreatePatientRecord() {
      string patId = (_lisPatFields.PatientId == null || _lisPatFields.PatientId == "") ? Sepreator : _lisPatFields.PatientId;
      string patientName = _lisPatFields.PatFName + " " + _lisPatFields.PatMName + " " + _lisPatFields.PatLName;
      string patDob = Sepreator;
      if (_lisPatFields.DOB != DateTime.MinValue) {
        patDob = ((DateTime)_lisPatFields.DOB).ToString("yyyyMMdd");
      }
      string patGender = (_lisPatFields.Gender == null || _lisPatFields.Gender == "") ? Sepreator : _lisPatFields.Gender;
      string patInfo = PatientR + "1" + Sepreator + patId + Sepreator + Sepreator + Sepreator
                     + patientName + Sepreator + Sepreator + patDob + Sepreator + patGender
                     + Sepreator + Sepreator + Sepreator + Sepreator + "Cureta" + Sepreator
                     + Sepreator + Sepreator + Sepreator + Sepreator + "Nada" +
                      Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + Sepreator + " Piso 3^Cama 1";
      return patInfo;
    }

    public override string CreateRequestRecord() {

      string bgnDate = DateTime.Now.ToString("yyyyMMdd") + "000000";
      if (_lisPatFields.BgnTestDateTime != DateTime.MinValue) {
        bgnDate = _lisPatFields.BgnTestDateTime.ToString("yyyyMMdd") + "000000";
      } 

      string endDate = DateTime.Now.ToString("yyyyMMdd") + "000000";
      if (_lisPatFields.EndTestDateTime != DateTime.MinValue) {
        endDate = _lisPatFields.EndTestDateTime.ToString("yyyyMMdd") + "000000";
      }


      string sampleId = string.IsNullOrEmpty(_lisPatFields.SampleId) ? "ALL" : "^" + _lisPatFields.SampleId;
      string methodId = string.IsNullOrEmpty(_lisPatFields.MethoId) ? "ALL" : "^^^" + _lisPatFields.MethoId;
      

      string reqInfo = RequestR + Sepreator + "1" + Sepreator + sampleId + Sepreator + Sepreator
                      + methodId + Sepreator + Sepreator + bgnDate + Sepreator + endDate;
      return reqInfo;
    }

    public override string CreateResultRecord() {
      return "";
    }

    public override string CreateTerminatorRecord() {
      string terminator = TerminatorR +Sepreator + "1" + Sepreator + "c";
      return terminator;
    }


    public List<string> GetRequestAnalysis() {
      string header = CreateHeaderRecord();
      string request = CreateRequestRecord();
      string terminator = CreateTerminatorRecord();

      List<string> orderList = new List<string> {
        header,
        request,
        terminator
      };
      return orderList;
    }

    public List<string> GetOrderRecord() {
      string header = CreateHeaderRecord();
      string patient = CreatePatientRecord();
      string order = CreateOrderRecord();
      string terminator = CreateTerminatorRecord();

      List<string> orderList = new List<string>() {
        header, patient, order, terminator
      };

      return orderList;
    }




  }

}
