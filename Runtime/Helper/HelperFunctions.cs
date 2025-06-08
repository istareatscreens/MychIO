using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MychIO.Helper
{
    public class HelperFunctions
    {


        public static string GenerateUniqueHashFromStrings(Array strings)
        {
            if (strings == null) throw new ArgumentNullException(nameof(strings));

            var stringList = new List<string>();
            foreach (var s in strings)
            {
                if (s is string str)
                    stringList.Add(str);
            }
            var uniqueSorted = new SortedSet<string>(stringList);

            var concatenated = string.Join("|", uniqueSorted);

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(concatenated);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }


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