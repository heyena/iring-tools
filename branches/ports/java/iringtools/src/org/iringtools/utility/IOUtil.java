package org.iringtools.utility;

import java.io.BufferedInputStream;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;

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
	public static String readString(String filePath) throws IOException
	{
		byte[] buffer = new byte[(int) new File(filePath).length()];		
    BufferedInputStream stream = new BufferedInputStream(new FileInputStream(filePath));
    stream.read(buffer);    
    return new String(buffer);
	}
	
	public static void writeString(String string, String filePath) throws IOException
	{
		BufferedWriter out = new BufferedWriter(new FileWriter(filePath));
		out.write(string);
		out.close();		
	}
	
	public static OMElement readXml(String filePath) throws FileNotFoundException, XMLStreamException, FactoryConfigurationError 
	{
	  XMLStreamReader parser = XMLInputFactory.newInstance().createXMLStreamReader(new FileInputStream(filePath));
    StAXOMBuilder builder = new StAXOMBuilder(parser);
    return builder.getDocumentElement();
	}
  
  public static void writeXml(OMElement xml, String filePath) throws FileNotFoundException, XMLStreamException, FactoryConfigurationError 
  {
    XMLStreamWriter writer = XMLOutputFactory.newInstance().createXMLStreamWriter(new FileOutputStream(filePath));
    xml.serialize(writer); 
    writer.flush();
  }
}
