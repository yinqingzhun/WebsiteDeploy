using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebDeploy.Utils
{
    public class HashHelper
    {
        public static byte[] ComputeHash(HashName hashName, string input)
        {
            HashAlgorithm algorithm = HashAlgorithm.Create(hashName.ToString());
            return algorithm.ComputeHash(Encoding.Unicode.GetBytes(input));
        }
        public static byte[] ComputeHash(HashName hashName, Stream inputStream)
        {
            HashAlgorithm algorithm = HashAlgorithm.Create(hashName.ToString());
            return algorithm.ComputeHash(inputStream);
        }
        public static string ComputeHashString(HashName hashName, string input)
        {
            return BitConverter.ToString(ComputeHash(hashName, input)).Replace("-", ""); 
        }

        public static string ComputeHashString(HashName hashName, byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string ComputeHashString(HashName hashName, Stream inputStream)
        {
            return BitConverter.ToString(ComputeHash(hashName, inputStream)).Replace("-", "");
        }

        public enum HashName
        {
            SHA,
            SHA1,
            MD5,
            SHA256,
            SHA384,
            SHA512
        }
    }
}
