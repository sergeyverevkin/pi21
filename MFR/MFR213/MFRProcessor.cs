
namespace ConsoleApp1
{
  public class CMFRProcessor
  {
    #region Map-Filter-Reduce
    public void Filter(
      Stream pInStream,
      Stream pOutStream,
      Func<byte, bool> fnFilter)
    {
      pInStream.Seek(0, SeekOrigin.Begin);

      byte[] bt = new byte[1];
      while (pInStream.Position < pInStream.Length) {
        pInStream.Read(bt, 0, 1);
        bool bIsNeeded = fnFilter(bt[0]);
        if (bIsNeeded) {
          pOutStream.Write(bt, 0, 1);
        }
      }
    }


    public void Map(
      Stream pInStream,
      Stream pOutStream,

      Func<byte, byte> fnMap)
    {

      pInStream.Seek(0, SeekOrigin.Begin);

      byte[] bt = new byte[1];
      while (pInStream.Position < pInStream.Length) {
        pInStream.Read(bt, 0, 1);
        byte[] btOut = new[] { fnMap(bt[0]) };
        pOutStream.Write(btOut, 0, 1);
      }
    }

    public int Reduce(
      Stream pStream,
      Func<int, byte, int> fnReduce)
    {
      pStream.Seek(0, SeekOrigin.Begin);

      byte[] bt = new byte[1];
      int ii = 0;
      while (pStream.Position < pStream.Length) {
        pStream.Read(bt, 0, 1);
        ii = fnReduce(ii, bt[0]);
        if (ii < 0) return -ii;
      }

      return ii;
    }


    #endregion

    #region delegates "implementation"

    private Dictionary<char, int> ar =
      new Dictionary<char, int>() {
        { '0', 0 },
        { '1', 0 },
        { '2', 0 },
        { '3', 0 },
      };


    public int FnReduceDictionary(int iCounter, byte btValue)
    {
      char ch = (char)btValue;
      if (ar.ContainsKey(ch)) {
        ar[ch]++;
        if (ar[ch] > 3) {
          return -btValue;
        }
      }
      return iCounter;
    }


    public int FnReduce(int iCounter, byte btValue)
    {
      if (btValue == '0') {
        iCounter++;
      }

      if (iCounter >= 3) {
        return -iCounter;
      }
      return iCounter;
    }

    public bool FnFilter(byte arg)
    {
      return arg == '0';
    }

    public byte FnMap(byte arg)
    {
      return arg;
    }
    

    #endregion
  }
}