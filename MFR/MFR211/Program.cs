using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MFR211
{
  internal class Program
  {
    public class Message
    {
      public string From;
      public string To;
      public string Content;

      public string AsString()
      {
        return JsonConvert.SerializeObject(this);
      }
    }

    public class MessageDatabase
    {
      private readonly string m_sFn;

      // private List<Message> MessageList = new List<Message>();
      /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
      public MessageDatabase(string sFn)
      {
        m_sFn = sFn;
      }

      #region low-level methods

      public List<Message> LoadWhole()
      {
        List<Message> arList = new List<Message>();
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            string sContent = Encoding.UTF8.GetChars(btContent).ToString();
            Message message = JsonConvert.DeserializeObject<Message>(sContent);
            arList.Add(message);
          }
        }

        return arList;
      }

      public Message LoadByIndex(int iIndex)
      {
        int ii = 0;
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            if (ii == iIndex) {
              string sContent = Encoding.UTF8.GetChars(btContent).ToString();
              Message message = JsonConvert.DeserializeObject<Message>(sContent);
              return message;
            }

            ii++;
          }
        }

        return null;
      }

      public void Save(List<Message> messageList)
      {
        using (Stream pStream = File.OpenWrite(m_sFn)) {
          StreamWriter pStreamWriter = new StreamWriter(pStream);
          foreach (Message message in messageList) {
            string sMsg = message.AsString();
            pStreamWriter.Write((char)sMsg.Length);
            pStreamWriter.Write(sMsg);
          }
        }
      }

      public int GetCount()
      {
        int ii = 0;
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            ii++;
          }
        }

        return ii;
      }

      #endregion

      #region Map

      /// <summary>
      /// 1 -> 1
      /// </summary>
      public IEnumerable<string> Map(
        Func<Message, string> fnMap,
        int iTop)
      {
        List<string> arList = new List<string>();
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            string sContent = Encoding.UTF8.GetChars(btContent).ToString();
            Message message = JsonConvert.DeserializeObject<Message>(sContent);
            // -----------
            string sResult = fnMap(message);
            arList.Add(sResult);
            if (arList.Count >= iTop) break;
          }
        }

        return arList;
      }


      public string FnMap(Message arg)
      {
        return $"{arg.From} / {arg.Content.Length}";
      }

      public string FnMap2(Message arg)
      {
        return $"{arg.From}";
      }


      #endregion

      #region Filter

      /// <summary>
      /// 1 -> (0-1)
      /// </summary>
      /// <param name="fnFilter"></param>
      /// <param name="iTopCount"></param>
      /// <returns></returns>
      public IEnumerable<Message> Filter(
        Func<Message, bool> fnFilter,
        int iTopCount)
      {
        List<Message> arList = new List<Message>();
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            string sContent = Encoding.UTF8.GetChars(btContent).ToString();
            Message message = JsonConvert.DeserializeObject<Message>(sContent);
            // -----------
            bool bAdd = fnFilter(message);
            if (bAdd) {
              arList.Add(message);
            }

            if (arList.Count >= iTopCount) break;
          }
        }

        return arList;
      }

      /// <summary>
      /// 1->1
      /// </summary>
      /// <param name="arg"></param>
      /// <returns></returns>
      public bool FnFilter(Message arg)
      {
        return true;
      }

      /// <summary>
      /// by condition
      /// </summary>
      /// <param name="arg"></param>
      /// <returns></returns>
      public bool FnFilterSelf(Message arg)
      {
        return (arg.From == arg.To);
      }

      #endregion

      #region Reduce

      public int Reduce(
        Func<Message, int, int> fnReduce)
      {
        int acc = 0;
        using (Stream pStream = File.OpenRead(m_sFn)) {
          while (pStream.Position < pStream.Length) {
            byte[] btHeader = new byte[0] { };
            pStream.Read(btHeader, 0, 1);
            byte[] btContent = new byte[btHeader[0]];
            pStream.Read(btContent, 0, btContent.Length);
            string sContent = Encoding.UTF8.GetChars(btContent).ToString();
            Message message = JsonConvert.DeserializeObject<Message>(sContent);
            // -----------
            acc = fnReduce(message, acc);
            if (acc < 0) {
              return -acc;
            }
          }
        }

        return acc;
      }

      private Dictionary<string, int> arCounter = new Dictionary<string, int>() {
        { "0", 0 },
        { "1", 0 },
        { "2", 0 },
        { "3", 0 },
        { "4", 0 },
      };

      /// <summary>
      /// Подсчет сообщений от заданных отправителей,
      /// но при нахождении больше
      /// трех от любого из них прекращает счет
      /// </summary>
      /// <param name="arg1"></param>
      /// <param name="iAcc"></param>
      /// <returns></returns>
      public int FnReduce(
        Message arg1, int iAcc)
      {
        if (arCounter.ContainsKey(arg1.From)) {
          arCounter[arg1.From]++;
          iAcc++;
          if (arCounter[arg1.From] > 3) {
            return -iAcc;
          }
        }
        return iAcc;
      }


      public int FnReduce2(
        Message arg1, int iAcc)
      {
        return iAcc + arg1.Content.Length;
      }

      #endregion
    }

    static void Main(string[] args)
    {
      const string sFn = "1.txt";
      MessageDatabase md = new MessageDatabase(sFn);
      // 1. Whole load
      //List<Message> arList = md.LoadWhole(sFn);
      // 2. load one by one
      int iiCount = md.GetCount();
      for (int ii = 0; ii < iiCount; ii++) {
        Message msg = md.LoadByIndex(ii);
        Console.WriteLine($"[loaded msg] {msg.From}");
      }
      // 3. Map - Filter - Reduce
      // 3.1. MAP
      IEnumerable<string> ar =
        md.Map(md.FnMap, 100);
      foreach (string str in ar) {
        Console.WriteLine($"[map msg] {str}");
      }

      // 3.2. FILTER
      IEnumerable<Message> ar2 =
        md.Filter(md.FnFilterSelf, 100);
      foreach (Message msg in ar2) {
        Console.WriteLine($"[filter msg] {msg.Content.Length}");
      }

      // 3.3. REDUCE
      int iCnt =
        md.Reduce(md.FnReduce);
      Console.WriteLine($"[reducemsg] {iCnt}");
    }
  }
}