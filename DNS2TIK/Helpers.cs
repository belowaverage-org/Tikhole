using System.Net;
using System.Text;

namespace DNS2TIK
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
    }
}