package org.iringtools.utility;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

import org.apache.commons.codec.binary.Base64;
import org.apache.log4j.Logger;

public class EncryptionUtils
{
  private static final Logger logger = Logger.getLogger(EncryptionUtils.class);
  private static final String ALGORITHM = "AES";
  private static final String ENCODING = "UTF-8";
  private static final String SECRET_KEY = "secret";
  
  private static Cipher cipher;
  private static SecretKeySpec keySpec;
  private static IvParameterSpec ivSpec;
  
  static
  {
    try
    {      
      byte[] keyBytes = new byte[16];
      byte[] buf = SECRET_KEY.getBytes(ENCODING);
      int len = (buf.length > keyBytes.length) ? keyBytes.length : buf.length;
      System.arraycopy(buf, 0, keyBytes, 0, len);
      
      cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
      keySpec = new SecretKeySpec(keyBytes, ALGORITHM);
      ivSpec = new IvParameterSpec(keyBytes);
    }
    catch (Exception e)
    {
      logger.error("Error initializing cryptor: " + e);
    }
  }
  
  public static String encrypt(String plainText) throws EncryptionException
  {    
    try
    {
      cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);
      byte[] cipherTextBytes = cipher.doFinal(plainText.getBytes(ENCODING));    
      return Base64.encodeBase64String(cipherTextBytes);
    }
    catch (Exception e)
    {
      String message = "Error encrypting [" + plainText + "]" + e;
      logger.error(message);
      throw new EncryptionException(message);
    }
  }
  
  public static synchronized String decrypt(String cipherText) throws EncryptionException
  {
    try
    {
      cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
      byte[] plainTextBytes = cipher.doFinal(Base64.decodeBase64(cipherText));
      return new String(plainTextBytes, ENCODING);
    }
    catch (Exception e)
    {
      String message = "Error decrypting [" + cipherText + "]" + e;
      logger.error(message);
      throw new EncryptionException(message);
    }
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
