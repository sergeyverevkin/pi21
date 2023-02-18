using System.Text;

namespace ConsoleApp1
{

  #region CRecordCollection

  public class CRecordCollection
  {
    private List<CRecord> RecordList;

    public void Serialize(Stream pOutStream)
    {
      foreach (CRecord pRecord in RecordList) {
        byte[] btRecord = pRecord.GetBytes();
        pOutStream.Write(btRecord);
      }
    }

    public void Deserialize(Stream pInStream)
    {
      pInStream.Seek(0, SeekOrigin.Begin);
      while (pInStream.Position < pInStream.Length) {
        byte[] btHeader = new byte[1];
        pInStream.Read(btHeader, 0, 1);
        byte[] btContent = new byte[btHeader[0]];
        pInStream.Read(btContent, 0, btContent.Length);
        string sName = Encoding.UTF8.GetChars(btContent).ToString();
        RecordList.Add(new CRecord() { Name = sName });
      }
    }

    public CRecord Deserialize2(Stream pInStream, int iRecordId)
    {
      pInStream.Seek(0, SeekOrigin.Begin);
      int ii = 0;
      while (pInStream.Position < pInStream.Length) {
        byte[] btHeader = new byte[1];
        pInStream.Read(btHeader, 0, 1);
        byte[] btContent = new byte[btHeader[0]];
        pInStream.Read(btContent, 0, btContent.Length);
        string sName = Encoding.UTF8.GetChars(btContent).ToString();
        if (ii == iRecordId) return new CRecord() { Name = sName };
        ii++;
      }

      return null;
    }

    public int DeserializeCount(Stream pInStream)
    {
      pInStream.Seek(0, SeekOrigin.Begin);
      int ii = 0;
      while (pInStream.Position < pInStream.Length) {
        byte[] btHeader = new byte[1];
        pInStream.Read(btHeader, 0, 1);
        byte[] btContent = new byte[btHeader[0]];
        pInStream.Read(btContent, 0, btContent.Length);
        ii++;
      }

      return ii;
    }
  }

  #endregion

  #region Record

  public class CRecord
  {
    public string Name;

    public byte[] GetBytes()
    {
      byte[] btInnerContent = Encoding.UTF8.GetBytes(Name);
      byte[] btHeader = new byte[] { (byte)btInnerContent.Length };
      byte[] btOut = new byte[btInnerContent.Length + btHeader.Length];
      for (int ii = 0; ii < btHeader.Length; ii++) {
        btOut[ii] = btHeader[ii];
      }

      int iShift = btHeader.Length;
      for (int ii = 0; ii < btInnerContent.Length; ii++) {
        btOut[ii + iShift] = btInnerContent[ii];
      }

      return btOut;
    }



  }



  #endregion

  #region CHardWorkProcessor

  public class CHardWorkProcessor
  {
    #region public methods

    public void StepThrough()
    {
      using (Stream pIn = File.OpenRead("1.txt")) {
        CRecordCollection pR = new CRecordCollection();
        int iCount = pR.DeserializeCount(pIn);
        for (int ii = 0; ii < iCount; ii++) {
          CRecord pRecord = pR.Deserialize2(pIn, ii);
          Console.WriteLine(pRecord.Name);
        }
      }
    }

    public void GetWhole()
    {
      using (Stream pIn = File.OpenRead("1.txt")) {
        CRecordCollection pR = new CRecordCollection();
        pR.Deserialize(pIn);
      }
    }

    #endregion
  }

  #endregion
}