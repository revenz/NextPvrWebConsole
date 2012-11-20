using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Helpers
{
    public class Encrypter
    {
        public static string GetCpuId()
        {
            string cpuInfo = string.Empty;
            System.Management.ManagementClass mc = new System.Management.ManagementClass("win32_processor");
            foreach (System.Management.ManagementObject mo in mc.GetInstances())
            {
                return mo.Properties["processorID"].Value.ToString();
            }
            throw new Exception("Failed to locate CPU ID.");
        }

        public static string Encrypt(string Text, string EncryptionKey = null)
        {
            if (EncryptionKey == null)
                EncryptionKey = new Configuration().PrivateSecret;
            Encoding unicode = Encoding.Unicode;

            string result = Convert.ToBase64String(Encrypt(unicode.GetBytes(EncryptionKey), unicode.GetBytes(Text)));
            return result.Replace("/", "_").Replace("+", "-"); // replace these characters for URLs
        }

        public static string Decrypt(string Text, string EncryptionKey = null)
        {
            if (EncryptionKey == null)
                EncryptionKey = new Configuration().PrivateSecret;
            Encoding unicode = Encoding.Unicode;
            Text = Text.Replace("_", "/").Replace("-", "+"); // replace the characters used for URLs

            return unicode.GetString(Encrypt(unicode.GetBytes(EncryptionKey), Convert.FromBase64String(Text)));
        }

        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        public static byte[] Decrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        private static byte[] EncryptInitalize(byte[] key)
        {
            byte[] s = Enumerable.Range(0, 256)
              .Select(i => (byte)i)
              .ToArray();

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + s[i]) & 255;

                Swap(s, i, j);
            }

            return s;
        }

        private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
        {
            byte[] s = EncryptInitalize(key);

            int i = 0;
            int j = 0;

            return data.Select((b) =>
            {
                i = (i + 1) & 255;
                j = (j + s[i]) & 255;

                Swap(s, i, j);

                return (byte)(b ^ s[(s[i] + s[j]) & 255]);
            });
        }

        private static void Swap(byte[] s, int i, int j)
        {
            byte c = s[i];

            s[i] = s[j];
            s[j] = c;
        }
    }
}