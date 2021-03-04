using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lowscope.Saving.Encryption
{
    public class AESEncryption
    {
        public AESEncryption(string key, string iv)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Key = CreateMD5(key);
            }

            if (!string.IsNullOrEmpty(iv))
            {
                IV = CreateMD5(iv);
            }
        }

        public string Key = "KA0DABN4F91BE9XB"; // must be 16 character
        public string IV = "CD2D6X2F334ICYRR"; // must be 16 character

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().Substring(16);
            }
        }

        public string Encrypt(string message)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            byte[] data = Encoding.UTF8.GetBytes(message);
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(dest);
            }
        }

        public byte[] Encrypt(byte[] bytes)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            byte[] data = bytes;
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
                return dest;
            }
        }

        public string Decrypt(string encryptedText)
        {
            string plaintext = null;
            using (AesManaged aes = new AesManaged())
            {
                byte[] cipherText = Convert.FromBase64String(encryptedText);
                byte[] aesIV = Encoding.UTF8.GetBytes(IV);
                byte[] aesKey = Encoding.UTF8.GetBytes(Key);
                ICryptoTransform decryptor = aes.CreateDecryptor(aesKey, aesIV);
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        public byte[] Decrypt(byte[] bytes)
        {
            using (AesManaged aes = new AesManaged())
            {
                byte[] cipherText = bytes;
                byte[] aesIV = Encoding.UTF8.GetBytes(IV);
                byte[] aesKey = Encoding.UTF8.GetBytes(Key);
                ICryptoTransform decryptor = aes.CreateDecryptor(aesKey, aesIV);

                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(cipherText, 0, cipherText.Length);

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
