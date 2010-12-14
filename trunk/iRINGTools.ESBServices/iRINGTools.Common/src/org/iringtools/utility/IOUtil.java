package org.iringtools.utility;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import javax.xml.stream.FactoryConfigurationError;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLOutputFactory;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.stream.XMLStreamWriter;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.builder.StAXOMBuilder;

public class IOUtil
{
  public static String toString(InputStream stream) throws IOException
  {
    BufferedReader reader = new BufferedReader(new InputStreamReader(stream, "UTF-8"));
    StringWriter writer = new StringWriter();
    char[] buffer = new char[2048];

    int actualSize;
    while ((actualSize = reader.read(buffer)) != -1)
      writer.write(buffer, 0, actualSize);

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

  public static List<String> getFiles(String directory) throws IOException
  {
    File[] files = new File(directory).listFiles();
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
}
