using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ChromeAutoUpdate
{
    public class rsa
    {

        private string dev_public = "<RSAKeyValue><Modulus>vlGWGWZRjS0EWSId260zvcLc7hPx6yruN5zUZGAbdo7VAo6PfCX8e6VPBY8I9QXqD8mFYH92/mF/0SlIjOSTZYdSwW9a+wEb87UZTbpCQxiWfys+8jdPPg2CLB7Em4OMTKlnh5+3Qx3OlZ197eLtpZ/6cHfrvAtOLqAYJkHi1Sz2o7SV74R+4YHECjBHnbA94FYY7QSJbDxv/GwhS2DrRQ5PR4JQBil7ZC2hjGNq2yJAFfVr95kr4iizzoyiGFc2fobF52wEEIqZ5+OwHqzXtu3ipxxV3MIvuxekbNVhZCOHBMjyuoG8VZD/LOlOU699wX5IUyq2KEx5p6wBSXsnt0uUNvR19Z+Iriqm2o72QilGCae7EjGXZmocBgh1yXphRRICPnTg0lYTl9xaPjVzyY2iMYAm4jKZAtGu+l+b9cXk2rUeVTOXecUwdTG37XfF7mteUUlYmH/boBTJjEksdfBNAo4YjKQm6m1zlfM52/kLtlLE3ceGDNBkZKuPEFMsufOFMHM9ZISFMU7fFVMNzu5oPkzbJSaWYLpRrTIs6BvGkxkrSjMzhaPyef/GxlDLu0bPut2qKBdJ5PF9oeg/A0F3Emf4IkCO6K+tfvEi95/48w+xxmj5t8yUJySkr1VEsWis4TBgoKxEIA6gKEHIqXGVpXZB9PKnDWqmQydAuE0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";


        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="fileName">要计算哈希值的 Stream</param>
        /// <returns>哈希值字节数组</returns>
        public string sha1_file( string fileName)
        {

            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] hashBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(fs);
            fs.Close();
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="str">要计算哈希值的字符串</param>
        /// <returns>sha1</returns>
        public string sha1(string str)
        {
            byte[] StrRes = Encoding.Default.GetBytes(str);
            byte[] hashBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(StrRes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        /// <summary>
        /// 生成公私钥
        /// </summary>
        /// <param name="PrivateKeyPath"></param>
        /// <param name="PublicKeyPath"></param>
        public void RSAKey(string PrivateKeyPath, string PublicKeyPath)
        {
            try
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider(4096);

                this.writeFile(PrivateKeyPath, provider.ToXmlString(true));
                this.writeFile(PublicKeyPath, provider.ToXmlString(false));

            }
            catch (Exception exception)
            {
                throw exception;
            }
        }


        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="xmlPublicKey">公钥</param>
        /// <param name="m_strEncryptString">MD5加密后的数据</param>
        /// <returns>RSA公钥加密后的数据</returns>
        public string RSAEncrypt(string xmlPublicKey, string m_strEncryptString)
        {
            string str2;
            try
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(xmlPublicKey);
                byte[] bytes = new UnicodeEncoding().GetBytes(m_strEncryptString);
                str2 = Convert.ToBase64String(provider.Encrypt(bytes, false));
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        public void writeFile(string path, string txt)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine(txt);
                sw.Close();
                file.Close();
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string readFile(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            string str = reader.ReadToEnd();
            reader.Close();
            return str.Trim();
        }
    }
}
