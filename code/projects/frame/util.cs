using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections;

public class Util
{
    public static long GetMillSecond()
    {
        return DateTime.Now.Ticks / 10000;
    }

    public static string Md5(byte[] data)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] dest = md5.ComputeHash(data);
        StringBuilder sb = new StringBuilder(32);
        for (int i = 0; i < dest.Length; i++)
        {
            sb.Append(dest[i].ToString("x").PadLeft(2, '0'));
        }

        return sb.ToString();
    }

    public static string EncryptBase64(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    public static byte[] DecryptBase64(string src)
    {
        return Convert.FromBase64String(src);
    }

    public static byte[] EncryptDes(byte[] data, string key)
    {
        if (key.Length >= 8)
        {
            key = key.Substring(0, 8);
        }
        else
        {
            key = key.PadRight(8, '0');
        }

        byte[] key_bytes = Encoding.UTF8.GetBytes(key);

        DES des = new DESCryptoServiceProvider();
        des.Key = key_bytes;
        des.IV = des.Key;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        ICryptoTransform icrypto = des.CreateEncryptor();
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, icrypto, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        cs.Close();
        return ms.ToArray();
    }

    public static byte[] DecryptDes(byte[] data, string key)
    {
        if (key.Length >= 8)
        {
            key = key.Substring(0, 8);
        }
        else
        {
            key = key.PadRight(8, '0');
        }

        byte[] key_bytes = Encoding.UTF8.GetBytes(key);


        DES des = new DESCryptoServiceProvider();
        des.Key = key_bytes;
        des.IV = des.Key;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        ICryptoTransform icrypto = des.CreateDecryptor();
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, icrypto, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        cs.Close();
        return ms.ToArray();
    }

    //public void Test()
    //{
    //    string src = "abcdefg";
    //    byte[] src_bytes = Encoding.UTF8.GetBytes(src);
    //    string key = "1234567890";
    //    byte[] key_bytes = Encoding.UTF8.GetBytes(key);

    //    string md5_out = Util.Md5(src_bytes);
    //    Console.WriteLine(md5_out);
    //    Console.WriteLine();

    //    string base64_out = Util.EncryptBase64(src_bytes);
    //    string base64_out1 = Encoding.UTF8.GetString(Util.DecryptBase64(base64_out));

    //    Console.WriteLine(base64_out);
    //    Console.WriteLine(base64_out1);
    //    Console.WriteLine();

    //    byte[] des_out = Util.EncryptDes(src_bytes, key);
    //    Console.WriteLine(Convert.ToBase64String(des_out));
    //    byte[] des_out1 = Util.DecryptDes(des_out, "1234567890");
    //    Console.WriteLine(Encoding.UTF8.GetString(des_out1));
    //    Console.WriteLine();

    //    Console.WriteLine(Encoding.UTF8.GetString(src_bytes));
    //}

}
