package org.iringtools.utility;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.xml.stream.FactoryConfigurationError;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLOutputFactory;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.stream.XMLStreamWriter;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.builder.StAXOMBuilder;

public final class IOUtils
{
  private static final int DEFAULT_BUFFER_SIZE = 1024;

  public static byte[] toByteArray(InputStream stream) throws IOException 
  {
    ByteArrayOutputStream output = new ByteArrayOutputStream();

    byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
    int n = 0;
    
    while ((n = stream.read(buffer)) != -1) {
      output.write(buffer, 0, n);
    }
    
    return output.toByteArray();
  }
  
  public static String toString(InputStream stream) throws IOException
  {
    BufferedReader reader = new BufferedReader(new InputStreamReader(stream, "UTF-8"));
    StringWriter writer = new StringWriter();
    char[] buffer = new char[DEFAULT_BUFFER_SIZE];

    int n;
    while ((n = reader.read(buffer)) != -1) {
      writer.write(buffer, 0, n);
    }
    
    return writer.toString();
  }

  public static String readString(String filePath) throws IOException
  {
    BufferedInputStream stream = new BufferedInputStream(new FileInputStream(filePath));
    return toString(stream);
  }

  public static void writeString(String string, String filePath) throws IOException
  {
    BufferedWriter out = new BufferedWriter(new FileWriter(filePath));
    out.write(string);
    out.close();
  }

  public static OMElement readXml(String filePath) throws FileNotFoundException, XMLStreamException,
      FactoryConfigurationError
  {
    XMLStreamReader parser = XMLInputFactory.newInstance().createXMLStreamReader(new FileInputStream(filePath));
    StAXOMBuilder builder = new StAXOMBuilder(parser);
    return builder.getDocumentElement();
  }

  public static OMElement stringToXml(String text) throws XMLStreamException, FactoryConfigurationError
  {
    XMLStreamReader parser = XMLInputFactory.newInstance().createXMLStreamReader(new StringReader(text));
    StAXOMBuilder builder = new StAXOMBuilder(parser);
    return builder.getDocumentElement();
  }

  public static void writeXml(OMElement xml, String filePath) throws FileNotFoundException, XMLStreamException,
      FactoryConfigurationError
  {
    XMLStreamWriter writer = XMLOutputFactory.newInstance().createXMLStreamWriter(new FileOutputStream(filePath));
    xml.serialize(writer);
    writer.flush();
  }

  public static Properties loadProperties(String filePath) throws IOException
  {
    Properties props = new Properties();
    InputStream fis = new FileInputStream(filePath);
    props.load(fis);
    fis.close();
    return props;
  }

  public static List<String> getFiles(String path) throws IOException
  {
    File[] files = new File(path).listFiles();
    List<String> fileNames = new ArrayList<String>();

    for (File file : files)
    {
      if (file.isFile())
      {
        fileNames.add(file.getName());
      }
    }

    return fileNames;
  }
  
  // hello world -> HelloWorld
  public static String toCamelCase(String value)
  {
    if (value == null || value.length() == 0)
      return value;
    
    StringBuilder returnValue = new StringBuilder();
    String[] words = value.split("\\s+");

    for (String word : words)
    {     
      if (word.length() > 0)
      {
        returnValue.append(word.substring(0, 1).toUpperCase() + word.substring(1).toLowerCase());        
      }
    }
    
    return returnValue.toString();
  }
  
  public static void copyFile(String sourceFile, String destinationFile) throws IOException
  {    
    File inputFile = new File(sourceFile);
    File outputFile = new File(destinationFile);

    FileReader in = new FileReader(inputFile);
    FileWriter out = new FileWriter(outputFile);
    int c;

    while ((c = in.read()) != -1) {
      out.write(c);
    }
    
    in.close();
    out.close();
  }
  
  public static boolean isNullOrEmpty(String str)
  {
    return (str == null || str.length() == 0);
  }
  
  public static boolean fileExists(String filePath)
  {
    File file = new File(filePath);
    return file.exists();
  }
  
  public static String getStackTrace(Exception e) 
  {
    StringWriter writer = new StringWriter();
    e.printStackTrace(new PrintWriter(writer));
    return writer.toString();
  }
  
  public static Map<String, String> splitQueryParams(String str)
  {
    Map<String, String> map = new HashMap<String, String>();
    
    if (!isNullOrEmpty(str))
    {
      String[] pairs = str.split("&");
      
      for (String pair : pairs)
      {
        String[] keyValue = pair.split("=");
        map.put(keyValue[0], keyValue[1]);
      }
    }
    
    return map;
  }
}
