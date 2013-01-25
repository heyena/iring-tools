package org.iringtools.utility;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.security.MessageDigest;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

import org.apache.commons.codec.binary.Base64;

public class EncryptionUtils
{
  private static final String ALGORITHM = "AES";
  private static final String ENCODING = "UTF-8";
  private static final String SECRET_KEY = "secret";

  private static Cipher cipher;
  private static SecretKeySpec keySpec;
  private static IvParameterSpec ivSpec;

  /*
   * Using internal secret key
   */
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
      System.out.println("Error initializing cryptor: " + e);
    }
  }

  public static synchronized String encrypt(String plainText) throws EncryptionException
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
      String message = "Error decrypting [" + cipherText + "]. " + e;
      throw new EncryptionException(message);
    }
  }

  /*
   * Using external secret key
   */
  // available key size: 128, 192, 256
  public static void generateKey(int keySize, String filePath) throws Exception
  {
    try
    {
      // generate key
      KeyGenerator keyGen = KeyGenerator.getInstance("AES");
      keyGen.init(keySize);
      byte[] key = keyGen.generateKey().getEncoded();

      // write key to file
      OutputStream out = new FileOutputStream(filePath);
      out.write(key);
      out.close();
    }
    catch (Exception e)
    {
      throw new Exception("Error generating key: " + e);
    }
  }

  private static void initKey(String keyFile) throws Exception
  {
    InputStream is = new FileInputStream(keyFile);
    byte[] keyBytes = new byte[is.available()];
    is.read(keyBytes);
    is.close();

    // use message digest to get 16-byte secret key
    MessageDigest md = MessageDigest.getInstance("MD5");
    md.update(keyBytes);
    keyBytes = md.digest();

    keySpec = new SecretKeySpec(keyBytes, ALGORITHM);
    ivSpec = new IvParameterSpec(keyBytes);
  }

  public static String encrypt(String plainText, String keyFile) throws EncryptionException
  {
    try
    {
      initKey(keyFile);
      Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
      cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);
      byte[] cipherTextBytes = cipher.doFinal(plainText.getBytes(ENCODING));
      return Base64.encodeBase64String(cipherTextBytes);
    }
    catch (Exception e)
    {
      String message = "Error encrypting [" + plainText + "]. " + e;
      throw new EncryptionException(message);
    }
  }

  public static String decrypt(String cipherText, String keyFile) throws EncryptionException
  {
    try
    {
      initKey(keyFile);
      Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
      cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
      byte[] plainTextBytes = cipher.doFinal(Base64.decodeBase64(cipherText));
      return new String(plainTextBytes, ENCODING);
    }
    catch (Exception e)
    {
      String message = "Error decrypting [" + cipherText + "]. " + e;
      throw new EncryptionException(message);
    }
  }

  private static int getOption(BufferedReader in) throws IOException
  {
    System.out.println("Select one of the following options: \n");
    System.out.println("  1. Generate Secret Key file & Encrypt");
    System.out.println("  2. Encrypt with existing Secret Key file");
    System.out.println("  3. Encrypt with default Secret Key");
    System.out.println("  4. Exit\n");

    String option = in.readLine();
    return Integer.parseInt(option);
  }

  public static void main(String[] args)
  {
    BufferedReader in = new BufferedReader(new InputStreamReader(System.in));

    try
    {
      int option = getOption(in);

      switch (option)
      {
      case 1:
      {
        System.out.print("Enter key file path: ");
        String keyFile = in.readLine();

        EncryptionUtils.generateKey(256, keyFile);

        System.out.print("Enter text to encrypt: ");
        String plainText = in.readLine();

        String cipherText = EncryptionUtils.encrypt(plainText, keyFile);
        System.out.println("Cipher text: " + cipherText);

        break;
      }
      case 2:
      {
        System.out.print("Enter key file path: ");
        String keyFile = in.readLine();

        System.out.print("Enter text to encrypt: ");
        String plainText = in.readLine();

        String cipherText = EncryptionUtils.encrypt(plainText, keyFile);
        System.out.println("Cipher text: " + cipherText);

        break;
      }
      case 3:
      {
        System.out.print("Enter text to encrypt: ");
        String plainText = in.readLine();

        String cipherText = EncryptionUtils.encrypt(plainText);
        System.out.println("Cipher text: " + cipherText);

        break;
      }
      default:
        System.exit(0);
      }
    }
    catch (Exception e)
    {
      System.out.println("Error occurred: " + e);
    }
    finally
    {
      try
      {
        in.close();
      }
      catch (IOException e) {}
    }
  }
}
