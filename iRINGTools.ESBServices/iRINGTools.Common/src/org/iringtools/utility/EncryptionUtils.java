package org.iringtools.utility;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import javax.crypto.Cipher;
import javax.crypto.SecretKey;
import javax.crypto.spec.SecretKeySpec;

import org.apache.log4j.Logger;

import sun.misc.BASE64Decoder;
import sun.misc.BASE64Encoder;

public class EncryptionUtils
{
  private static final Logger logger = Logger.getLogger(EncryptionUtils.class);

  private static String encoding = "utf8";
  private static Cipher cipher = null;
  private static SecretKey key = null;

  static
  {
    try
    {
      String algorithm = "AES";
      String cipherKey = "k6KCxFRB2h5nWGMDdYFZ/w==";

      // javax.crypto.KeyGenerator keyGen = javax.crypto.KeyGenerator.getInstance(algorithm);
      // keyGen.init(128);
      // System.out.println("Key: " + new BASE64Encoder().encode(keyGen.generateKey().getEncoded()));

      byte[] keyBytes = new BASE64Decoder().decodeBuffer(cipherKey);
      key = new SecretKeySpec(keyBytes, algorithm);

      cipher = Cipher.getInstance(algorithm);
    }
    catch (Exception e)
    {
      logger.error("Initialization error: " + e);
    }
  }

  public static String encrypt(String plainText) throws EncryptionException
  {
    if (cipher != null)
    {
      try
      {
        cipher.init(Cipher.ENCRYPT_MODE, key);
        byte[] encryptedBytes = cipher.doFinal(plainText.getBytes(encoding));
        return new BASE64Encoder().encode(encryptedBytes);
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Encryption error: " + ex);
      }
    }
    
    throw new EncryptionException("Encryptor not initialized.");
  }

  public static synchronized String decrypt(String cipherText) throws EncryptionException
  {
    if (cipher != null)
    {
      try
      {
        cipher.init(Cipher.DECRYPT_MODE, key);
        byte[] encryptedBytes = new BASE64Decoder().decodeBuffer(cipherText);
        byte[] plainText = cipher.doFinal(encryptedBytes);
        return new String(plainText, encoding);
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Decryption error: " + ex);
      }
    }
    
    throw new EncryptionException("Encryptor not initialized.");
  }

  public static void main(String[] args) throws EncryptionException
  {
    String plainText = "";
    
    if (args.length == 0)
    {
      System.out.print("Enter text to encrypt: ");
      BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
      
      try
      {
        plainText = in.readLine();
      }
      catch (IOException e)
      {
        System.out.println("Error reading text: " + e);
      }
    }
    else
    {
      plainText = args[0];
    }
    
    String cipherText = encrypt(plainText);
    System.out.println("Cipher text: " + cipherText);
  }
}
