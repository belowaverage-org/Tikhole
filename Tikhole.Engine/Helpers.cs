using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace Tikhole.Engine
{
    public static class Helpers
    {
        public static uint ToUInt(this byte[] Bytes, int Index)
        {
            byte[] reversed =
            [
                Bytes[Index + 3],
                Bytes[Index + 2],
                Bytes[Index + 1],
                Bytes[Index],
            ];
            return BitConverter.ToUInt32(reversed);
        }
        public static ushort ToUShort(this byte[] Bytes, int Index)
        {
            byte[] reversed =
            [
                Bytes[Index + 1],
                Bytes[Index],
            ];
            return BitConverter.ToUInt16(reversed);
        }
        public static string ToLabelsString(this byte[] Bytes, ref int Index)
        {
            string result = "";
            while (true)
            {
                int lLength = Bytes[Index];
                if ((Bytes[Index] & 0b11000000) == 0b11000000)
                {
                    byte[] pBytes = 
                    [
                        (byte)(Bytes[Index] & 0b00111111),
                        Bytes[Index + 1]
                    ];
                    int pointer = pBytes.ToUShort(0);
                    Index += 1;
                    return result + ToLabelsString(Bytes, ref pointer);
                }
                if (lLength == 0)
                {
                    if (result.Length > 0) result = result.Remove(result.Length - 1);
                    break;
                }
                result += Encoding.ASCII.GetString(Bytes, Index + 1, lLength) + '.';
                Index += lLength + 1;
            }
            return result;
        }
        public static IPAddress[] ToAddresses(this DNSResourceRecord DNSResourceRecord)
        {
            IPAddress[] addresses = new IPAddress[0];
            if (DNSResourceRecord.Type == DNSType.A)
            {
                int addressCount = DNSResourceRecord.Data.Length / 4;
                addresses = new IPAddress[addressCount];
                for (int i = 0; i < addressCount; i++)
                {
                    addresses[i] = new IPAddress(DNSResourceRecord.Data.AsSpan(i * 4, i * 4 + 4));
                }
            }
            if (DNSResourceRecord.Type == DNSType.AAAA)
            {
                int addressCount = DNSResourceRecord.Data.Length / 16;
                addresses = new IPAddress[addressCount];
                for (int i = 0; i < addressCount; i++)
                {
                    addresses[i] = new IPAddress(DNSResourceRecord.Data.AsSpan(i * 16, i * 16 + 16));
                }
            }
            return addresses;
        }
        public static string[] SendSentence(this TcpClient Client, string[] Words)
        {
            List<byte> sentenceBytes = new();
            foreach (string word in Words) sentenceBytes.AddWord(word);
            sentenceBytes.AddWord(string.Empty);
            Client.Client.Send(sentenceBytes.ToArray());
            return Client.ReadSentence();
        }
        private static void AddWord(this List<byte> SentenceBytes, string Word)
        {
            byte[] wordBytes = Encoding.ASCII.GetBytes(Word);
            byte[] wLength = BitConverter.GetBytes((uint)wordBytes.Length);
            if (wordBytes.Length <= 0x7f)
            {
                SentenceBytes.Add(wLength[0]);
            }
            else if (wordBytes.Length <= 0x3fff)
            {
                SentenceBytes.Add((byte)(wLength[1] | 0x80));
                SentenceBytes.Add(wLength[0]);
            }
            else if (wordBytes.Length <= 0x1fffff)
            {
                SentenceBytes.Add((byte)(wLength[2] | 0xc0));
                SentenceBytes.Add(wLength[1]);
                SentenceBytes.Add(wLength[0]);
            }
            else if (wordBytes.Length <= 0xfffffff)
            {
                SentenceBytes.Add((byte)(wLength[3] | 0xe0));
                SentenceBytes.Add(wLength[2]);
                SentenceBytes.Add(wLength[1]);
                SentenceBytes.Add(wLength[0]);
            }
            else
            {
                SentenceBytes.Add((byte)(wLength[4] | 0xf0));
                SentenceBytes.Add(wLength[3]);
                SentenceBytes.Add(wLength[2]);
                SentenceBytes.Add(wLength[1]);
                SentenceBytes.Add(wLength[0]);
            }
            SentenceBytes.AddRange(wordBytes);
        }
        private static string[] ReadSentence(this TcpClient Client)
        {
            Client.Client.Poll(TimeSpan.FromMilliseconds(1000), SelectMode.SelectRead);
            List<string> sentence = new();
            if (Client.Available == 0) return sentence.ToArray();
            byte[] bytes = new byte[Client.Available];
            Client.Client.Receive(bytes);
            int index = 0;
            while (true)
            {
                int length = bytes[index++];
                if (index == bytes.Length) break;
                if (length == 0) continue;
                sentence.Add(Encoding.ASCII.GetString(bytes, index, length));
                index += length;
            }
            return sentence.ToArray();
        }
        public static void AddSetting(this XmlNode Parent, string Name, string InnerText)
        {
            if (Parent.OwnerDocument == null) return;
            XmlNode child = Parent.OwnerDocument.CreateElement(Name);
            child.InnerText = InnerText;
            Parent.AppendChild(child);
        }
        public static void ReadSetting(this XmlNode Parent, string XPath, ref string Value)
        {
            XmlNode? node = Parent.SelectSingleNode(XPath);
            if (node != null) Value = node.InnerText;
        }
        public static void ReadSetting(this XmlNode Parent, string XPath, ref IPEndPoint Value)
        {
            XmlNode? node = Parent.SelectSingleNode(XPath);
            if (node != null)
            {
                IPEndPoint.TryParse(node.InnerText, out IPEndPoint? iepValue);
                if (iepValue != null) Value = iepValue;
            }
        }
        public static void ReadSetting(this XmlNode Parent, string XPath, ref bool Value)
        {
            XmlNode? node = Parent.SelectSingleNode(XPath);
            if (node != null) bool.TryParse(node.InnerText, out Value);
        }
        public static void ReadSetting(this XmlNode Parent, string XPath, ref uint Value)
        {
            XmlNode? node = Parent.SelectSingleNode(XPath);
            if (node != null) uint.TryParse(node.InnerText, out Value);
        }
    }
}