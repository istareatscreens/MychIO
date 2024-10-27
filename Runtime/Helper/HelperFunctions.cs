using System;
using System.Linq;
using System.Text;

namespace MychIO.Helper
{
    // Mainly functions for debugging
    public class HelperFunctions
    {

        public static string ConvertByteArrayToBitString(byte[] byteArray)
        {
            StringBuilder bitStringBuilder = new StringBuilder();

            foreach (byte b in byteArray)
            {
                bitStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0')); // Convert byte to binary string
            }

            return bitStringBuilder.ToString();
        }
        public static string BytesToString(byte[] byteArray) => new string(byteArray.Select(b => b == 0x00 ? '*' : (char)b).ToArray());

        public static string ByteArrayToBitString(byte[] byteArray)
        {
            if (byteArray == null)
            {
                return "";
            }

            var bitString = new StringBuilder();

            foreach (byte b in byteArray)
            {
                bitString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return bitString.ToString();
        }

        public static string ByteToBitString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

    }
}