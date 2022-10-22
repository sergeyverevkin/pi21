
using System.Text;
using System.Xml;

namespace ConsoleApp1
{
    public static class Program
    {
        private static string fileSimple = "XmlFile1.xml";
        private static string fileHard = "XmlFile1.hard";
        private const int size = 1999999;

        public static void Main(params string[] args)
        {
            DateTime dtStart = DateTime.Now;
            h_HardDaysWrite();
            Console.WriteLine($"Creating file {(DateTime.Now - dtStart).TotalMilliseconds}ms");

            dtStart = DateTime.Now;
            // Loads immediatelly all lines in memory
            string[]? lines = File.ReadAllLines(fileHard);
            Console.WriteLine($"Read all lines file {(DateTime.Now - dtStart).TotalMilliseconds}ms");

            dtStart = DateTime.Now;
            // Loads (reads) every line one by one
            h_HardDaysRead();
            Console.WriteLine($"Read line by line file {(DateTime.Now - dtStart).TotalMilliseconds}ms");

            h_LoadStream();
            h_LoadDOM();
            h_LoadSAX();
        }
        private static void h_HardDaysWrite()
        {
            using (Stream stream = File.Create(fileHard))
            {
                for (int ii = 0; ii < size; ii++)
                {
                    stream.Write(new byte[] { 64 });
                    if (ii % 50 == 0)
                    {
                        stream.Write(new byte[] { 13, 10 });
                    }
                }
            }
        }
        private static void h_HardDaysRead()
        {
            using (Stream stream = File.OpenRead(fileHard))
            {
                using (StreamReader reader = new StreamReader(
                    stream, Encoding.ASCII))
                {
                    // reader.BaseStream.Position = 0;
                    string? sLine = reader.ReadLine();
                }
            }
        }

        private static void h_LoadStream()
        {
            byte[] bt = new byte[1024];
            using (Stream stream = new MemoryStream(bt))
            {
                //...
                Console.WriteLine(stream.Position);
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                if (stream.CanWrite)
                {
                    stream.Write(new byte[] { 88 });
                }
                if (stream.CanRead)
                {
                    byte[] btBuff = new byte[5];
                    stream.Read(btBuff, 0, 5);
                }
            }
            using (Stream stream = File.OpenRead("XMLFile1.hard"))
            {
                using (StreamReader reader = new StreamReader(
                    stream, Encoding.ASCII))
                {
                    reader.BaseStream.Position = 0;
                    string sLine = reader.ReadLine();
                 }
            }
        }

        private static void h_LoadSAX()
        {
            //            XmlReader.Create()
        }

        private static void h_LoadDOM()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("XMLFile1.xml");
            XmlAttribute pAttr = xmlDocument.CreateAttribute("aaa");
            pAttr.Value = "aq1";
            xmlDocument.ChildNodes[1]?.Attributes?.Append(pAttr);
            Console.WriteLine($"Nodes: {h_Count(0, xmlDocument.ChildNodes)}");
            xmlDocument.Save("XMLFile2.xml");
        }

        private static int h_Count(int iInitialCount, XmlNodeList ar)
        {
            // TODO
            if (ar == null) return iInitialCount;
            int iCount = iInitialCount;
            foreach (XmlNode node in ar)
            {
                iCount = h_Count(iCount + ar.Count, node.ChildNodes);
            }
            return iCount;
        }
    }

}
