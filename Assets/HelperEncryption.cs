using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class HelperClass
{

    private const string strEncryptionKey = "9!8@$7#6ios3746w$";

    /// <summary>
    /// التشغير
    /// </summary>
    /// <param name="TextToEncrypt"></param>
    /// <returns></returns>
    public static string Encrypt(string TextToEncrypt, string id)
    {
        byte[] MyEncryptedArray = UTF8Encoding.UTF8
           .GetBytes(TextToEncrypt);

        MD5CryptoServiceProvider MyMD5CryptoService = new
           MD5CryptoServiceProvider();

        byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
           (UTF8Encoding.UTF8.GetBytes(strEncryptionKey + id));

        MyMD5CryptoService.Clear();

        var MyTripleDESCryptoService = new
           TripleDESCryptoServiceProvider();

        MyTripleDESCryptoService.Key = MysecurityKeyArray;

        MyTripleDESCryptoService.Mode = CipherMode.ECB;

        MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var MyCrytpoTransform = MyTripleDESCryptoService
           .CreateEncryptor();

        byte[] MyresultArray = MyCrytpoTransform
           .TransformFinalBlock(MyEncryptedArray, 0,
           MyEncryptedArray.Length);

        MyTripleDESCryptoService.Clear();

        return Convert.ToBase64String(MyresultArray, 0,
           MyresultArray.Length);
    }

    /// <summary>
    /// فك التشفير
    /// </summary>
    /// <param name="TextToDecrypt"></param>
    /// <returns></returns>
    public static string Decrypt(string TextToDecrypt, string id)
    {
        byte[] MyDecryptArray = Convert.FromBase64String
           (TextToDecrypt);

        MD5CryptoServiceProvider MyMD5CryptoService = new
           MD5CryptoServiceProvider();

        byte[] MysecurityKeyArray = MyMD5CryptoService.ComputeHash
           (UTF8Encoding.UTF8.GetBytes(strEncryptionKey + id));

        MyMD5CryptoService.Clear();

        var MyTripleDESCryptoService = new
           TripleDESCryptoServiceProvider();

        MyTripleDESCryptoService.Key = MysecurityKeyArray;

        MyTripleDESCryptoService.Mode = CipherMode.ECB;

        MyTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var MyCrytpoTransform = MyTripleDESCryptoService
           .CreateDecryptor();

        byte[] MyresultArray = MyCrytpoTransform
           .TransformFinalBlock(MyDecryptArray, 0,
           MyDecryptArray.Length);

        MyTripleDESCryptoService.Clear();

        return UTF8Encoding.UTF8.GetString(MyresultArray);
    }

}
