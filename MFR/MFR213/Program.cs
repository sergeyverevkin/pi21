using System.IO.Pipes;
using System.Reflection.Metadata;
using System.Text;

namespace ConsoleApp1
{

  public class Program
  {
    public static void Main()
    {
      //// hard work demo
      //CHardWorkProcessor pProc = new CHardWorkProcessor();
      //pProc.GetWhole();
      //pProc.StepThrough();

      // map - filter - reduce demo
      h_StepMap();
      h_StepFilter();
      h_StepReduce();


    }

    #region demo methods

    /// <summary>
    /// 1 -> (acc++)
    /// </summary>
    private static void h_StepReduce()
    {
      using (Stream pIn = File.OpenRead("1.txt")) {
        CMFRProcessor pProc = new CMFRProcessor();
        int iCount = pProc.Reduce(pIn, pProc.FnReduceDictionary);
        Console.WriteLine($"{iCount}");
      }
    }

    /// <summary>
    /// 1 -> (0..1)
    /// </summary>
    private static void h_StepFilter()
    {
      using (Stream pIn = File.OpenRead("1.txt")) {
        using (Stream poUT = File.OpenWrite("2.txt")) {
          CMFRProcessor pProc = new CMFRProcessor();
          pProc.Filter(pIn, poUT, pProc.FnFilter);
          Console.WriteLine($"Done");
        }
      }
    }

    /// <summary>
    /// 1 -> 1
    /// </summary>
    private static void h_StepMap()
    {
      using (Stream pIn = File.OpenRead("1.txt")) {
        using (Stream poUT = File.OpenWrite("2.txt")) {
          CMFRProcessor pProc = new CMFRProcessor();
          pProc.Map(pIn, poUT, pProc.FnMap);
          Console.WriteLine($"Done");
        }
      }
    }

    #endregion


  }

}