using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace CApp
{
    /// <summary>
    /// RSA加解密
    /// </summary>
    public class RSACryption
    {

        #region RSA 的密钥产生  
        /// <summary>  
        /// RSA产生密钥  
        /// </summary>  
        /// <param name="xmlKeys">私钥</param>  
        /// <param name="xmlPublicKey">公钥</param>  
        public void RSAKey(out string xmlKeys, out string xmlPublicKey)
        {
            try
            {
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                xmlKeys = rsa.ToXmlString(true);
                xmlPublicKey = rsa.ToXmlString(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        /// <summary>
        /// 加密不限制长度
        /// </summary>
        /// <param name="rawInput"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public string RsaEncrypt(string rawInput, string publicKey)
        {
            if (string.IsNullOrEmpty(rawInput))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new ArgumentException("Invalid Public Key");
            }

            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = Encoding.UTF8.GetBytes(rawInput);//有含义的字符串转化为字节流
                rsaProvider.FromXmlString(publicKey);//载入公钥
                int bufferSize = (rsaProvider.KeySize / 8) - 11;//单块最大长度
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    { //分段加密
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var encryptedBytes = rsaProvider.Encrypt(temp, false);
                        outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());//转化为字节流方便传输
                }
            }
        }



        /// <summary>
        /// 解密不限制长度
        /// </summary>
        /// <param name="encryptedInput"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public string RsaDecrypt(string encryptedInput, string privateKey)
        {
            if (string.IsNullOrEmpty(encryptedInput))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new ArgumentException("Invalid Private Key");
            }

            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = Convert.FromBase64String(encryptedInput);
                rsaProvider.FromXmlString(privateKey);
                int bufferSize = rsaProvider.KeySize / 8;
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    {
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var rawBytes = rsaProvider.Decrypt(temp, false);
                        outputStream.Write(rawBytes, 0, rawBytes.Length);
                    }
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }


    }

    public class KeyGenerator
    {
        /// <summary>  
        /// 随机生成秘钥（对称算法）  
        /// </summary>  
        /// <param name="key">秘钥(base64格式)</param>  
        /// <param name="iv">iv向量(base64格式)</param>  
        /// <param name="keySize">要生成的KeySize，每8个byte是一个字节，注意每种算法支持的KeySize均有差异，实际可通过输出LegalKeySizes来得到支持的值</param>  
        public static void CreateSymmetricAlgorithmKey<T>(out string key, out string iv, int keySize)
            where T : SymmetricAlgorithm, new()
        {
            using (T t = new T())
            {
                t.KeySize = keySize;
                t.GenerateIV();
                t.GenerateKey();
                iv = Convert.ToBase64String(t.IV);
                key = Convert.ToBase64String(t.Key);
            }
        }

        /// <summary>  
        /// 随机生成秘钥（非对称算法）  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="publicKey">公钥（Xml格式）</param>  
        /// <param name="privateKey">私钥（Xml格式）</param>  
        /// <param name="provider">用于生成秘钥的非对称算法实现类，因为非对称算法长度需要在构造函数传入，所以这里只能传递算法类</param>  
        public static void CreateAsymmetricAlgorithmKey<T>(out string publicKey, out string privateKey, T provider = null)
            where T : AsymmetricAlgorithm, new()
        {
            if (provider == null)
            {
                provider = new T();
            }
            using (provider)
            {
                publicKey = provider.ToXmlString(false);
                privateKey = provider.ToXmlString(true);
            }
        }

    }
}
