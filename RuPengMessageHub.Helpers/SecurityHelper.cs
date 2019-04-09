using System;
using System.Security.Cryptography;
using System.Text;

namespace RuPengMessageHub.Helpers
{
    public static class SecurityHelper
    {
        public static String GetHash(String input)
        {
            //建立SHA1对象
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                //将mystr转换成byte[]
                UTF8Encoding enc = new UTF8Encoding();
                byte[] dataToHash = enc.GetBytes(input);

                //Hash运算
                byte[] dataHashed = sha.ComputeHash(dataToHash);

                //将运算结果转换成string
                string hash = BitConverter.ToString(dataHashed).Replace("-", "");

                return hash;
            }
        }
    }
}
