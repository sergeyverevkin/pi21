using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace MFR212
{
  public class Event
  {
    public DateTime EventDate;
    public string Message;
    public int Type;
  }


  // 1. Данных много: они не умещаются в память
  // 2. Возможность распараллеливания обработки
  // 3. 
  public class EventManager
  {
    private readonly string m_sFilename;

    public EventManager(string sFilename)
    {
      m_sFilename = sFilename;
    }

    public void SaveEvents(List<Event> arEvents)
    {
      using (Stream pOutputStream =
             File.OpenWrite(m_sFilename)) {
        using (StreamWriter pSw =
               new StreamWriter(pOutputStream, Encoding.UTF8)) {
          foreach (Event pEvent in arEvents) {
            string sEvent = JsonConvert.SerializeObject(pEvent);
            pSw.Write((byte)sEvent.Length);
            pSw.Write(sEvent);
          }
        }
      }
    }

    public List<Event> LoadEvents()
    {
      List<Event> ar = new List<Event>();
      using (Stream pStream =
             File.OpenRead(m_sFilename)) {
        while (pStream.Length > pStream.Position) {
          byte[] btHeader = new byte[1];
          pStream.Read(btHeader, 0, 1);

          byte[] btContent = new byte[btHeader[0]];
          pStream.Read(btContent, 0, btContent.Length);
          string sContent = Encoding.UTF8.GetString(btContent);
          Event pEvent = JsonConvert.DeserializeObject<Event>(sContent);
          ar.Add(pEvent);
        }
      }

      return ar;
    }

    /// <summary>
    /// 1 -> 1 (конвертация)
    /// </summary>
    /// <param name="fnMap"></param>
    /// <param name="iTopCount"></param>
    /// <returns></returns>
    public List<string> Map(
      Func<Event, string> fnMap,
      int iTopCount)
    {
      List<string> ar = new List<string>();
      using (Stream pStream =
             File.OpenRead(m_sFilename)) {
        while (pStream.Length > pStream.Position) {
          byte[] btHeader = new byte[1];
          pStream.Read(btHeader, 0, 1);

          byte[] btContent = new byte[btHeader[0]];
          pStream.Read(btContent, 0, btContent.Length);
          string sContent = Encoding.UTF8.GetString(btContent);
          Event pEvent = JsonConvert.DeserializeObject<Event>(sContent);
          ar.Add(fnMap(pEvent));
          if (ar.Count >= iTopCount) break;
        }
      }

      return ar;


    }

    public string fnMap(Event arg)
    {
      return arg.Message;
    }

    public string fnMap2(Event arg)
    {
      return arg.EventDate.ToShortTimeString();
    }

    /// <summary>
    /// * -> мало (фильтрация)
    /// </summary>
    /// <param name="fnFilter"></param>
    /// <param name="iTopCount"></param>
    /// <returns></returns>
    public List<Event> Filter(
      Func<Event, bool> fnFilter,
      int iTopCount)
    {

      List<Event> ar = new List<Event>();
      using (Stream pStream =
             File.OpenRead(m_sFilename)) {
        while (pStream.Length > pStream.Position) {
          byte[] btHeader = new byte[1];
          pStream.Read(btHeader, 0, 1);

          byte[] btContent = new byte[btHeader[0]];
          pStream.Read(btContent, 0, btContent.Length);
          string sContent = Encoding.UTF8.GetString(btContent);
          Event pEvent = JsonConvert.DeserializeObject<Event>(sContent);
          if (fnFilter(pEvent)) {
            ar
              .Add(pEvent);
            if (ar.Count >= iTopCount) break;
          }
        }
      }

      return ar;
    }

    public bool fnFilter(Event arg)
    {
      return true;
    }

    public bool fnFilterToday(Event arg)
    {
      return (arg.EventDate >= DateTime.Today);
    }

    /// <summary>
    /// * -> 1 (свод)
    /// </summary>
    /// <param name="fnReduce"></param>
    /// <returns></returns>
    public int Reduce(
      Func<Event, int, int> fnReduce)
    {
      int iCounter = 0;
      using (Stream pStream =
             File.OpenRead(m_sFilename)) {
        while (pStream.Length > pStream.Position) {
          byte[] btHeader = new byte[1];
          pStream.Read(btHeader, 0, 1);

          byte[] btContent = new byte[btHeader[0]];
          pStream.Read(btContent, 0, btContent.Length);
          string sContent = Encoding.UTF8.GetString(btContent);
          Event pEvent = JsonConvert.DeserializeObject<Event>(sContent);

          iCounter = fnReduce(pEvent, iCounter);
        }
      }

      return iCounter;
    }


    /// <summary>
    /// * -> 1 (свод)
    /// </summary>
    /// <param name="fnReduce"></param>
    /// <returns></returns>
    public int ReduceWithBreak(
      Func<Event, int, int> fnReduce)
    {
      int iCounter = 0;
      using (Stream pStream =
             File.OpenRead(m_sFilename)) {
        while (pStream.Length > pStream.Position) {
          byte[] btHeader = new byte[1];
          pStream.Read(btHeader, 0, 1);

          byte[] btContent = new byte[btHeader[0]];
          pStream.Read(btContent, 0, btContent.Length);
          string sContent = Encoding.UTF8.GetString(btContent);
          Event pEvent = JsonConvert.DeserializeObject<Event>(sContent);

          iCounter = fnReduce(pEvent, iCounter);
          if (iCounter < 0) return -iCounter;
        }
      }

      return iCounter;
    }


    public int fnReduceErrorsToday(
      Event pObject,
      int iAccumulator)
    {
      if (pObject.Type == 2
          && pObject.EventDate > DateTime.Today
          && pObject.EventDate < DateTime.Today.AddDays(+1)) {
        return iAccumulator + 1;
      }

      return iAccumulator;
    }


    public int fnReduceErrorsTodayMax(
      Event pObject,
      int iAccumulator,
      int iMax)
    {
      if (pObject.Type == 2
          && pObject.EventDate > DateTime.Today
          && pObject.EventDate < DateTime.Today.AddDays(+1)) {

        if (iAccumulator >= iMax - 1) {
          return -(iAccumulator + 1);
        }

        return iAccumulator + 1;
      }

      return iAccumulator;
    }

    private Dictionary<int, int> arCount = new Dictionary<int, int>() {
      { 0, 0 },
      { 1, 0 },
      { 2, 0 },
    };


    public int fnReduceBreak(
      Event pObject,
      int iAccumulator)
    {
      if (arCount.ContainsKey(pObject.Type)) {
        arCount[pObject.Type]++;
        if (arCount[pObject.Type] > 3) {
          return -pObject.Type;
        }
      }

      return iAccumulator;
    }


    public int fnReduce(
        Event pObject,
        int iAccumulator,
        int iMax)
    {
      if (pObject.Type == 2
          && pObject.EventDate > DateTime.Today
          && pObject.EventDate < DateTime.Today.AddDays(+1)) {

        if (iAccumulator >= iMax - 1) {
          return -(iAccumulator + 1);
        }

        return iAccumulator + 1;
      }

      return iAccumulator;
    }


  }


  internal class Program
  {
    static void Main(string[] args)
    {
      // 1. Working with whole content in memory
      EventManager ev = new EventManager("1.txt");
      List<Event> ar = ev.LoadEvents();
      foreach (Event pEvent in ar) {
        Console.WriteLine($"{pEvent.Message}");
      }
      // 2. Map, Filter, Reduce
      // 2.1. MAP
      List<string> ar2 = ev.Map(ev.fnMap, 100);
      foreach (string str in ar2) {
        Console.WriteLine($"{str}, ");
      }

      // 2.2. FILTER
      List<Event> ar3 = ev.Filter(
        ev.fnFilterToday, 100);
      foreach (Event pEvent in ar3) {
        Console.WriteLine($"{pEvent.Message}, ");
      }

      // 2.3. REDUCE
      int iCount = ev.Reduce(ev.fnReduceErrorsToday);
      Console.WriteLine($"{iCount}");
      int iCount2 = ev.ReduceWithBreak(
        ev.fnReduceBreak);
      Console.WriteLine($"{iCount2}");
    }
  }
}