using System;
using System.Security.Cryptography;
using System.Text;

namespace RuPengMessageHub.Helpers
{
    public static class SecurityHelper
    {
        public static String GetHash(String input)
        {
            //Create SHA1 Instance
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                //convert mystr into byte[]
                UTF8Encoding enc = new UTF8Encoding();
                byte[] dataToHash = enc.GetBytes(input);

                //Hash calculation
                byte[] dataHashed = sha.ComputeHash(dataToHash);

                //convert byte[] into string
                string hash = BitConverter.ToString(dataHashed).Replace("-", "");

                return hash;
            }
        }
    }
}
