package org.iringtools.utility;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.Unmarshaller;

public class JaxbUtil
{
	public static <T> void write(T object, String path) throws JAXBException, IOException
  {		
		OutputStream stream = new FileOutputStream(path);
		toXml(object, stream);
  }
	
	public static <T> String toXml(T object) throws JAXBException, IOException 
  {
    OutputStream stream = toStream(object);
    return stream.toString();   
  }
  
	public static <T> OutputStream toStream(T object) throws JAXBException, IOException
  {
    OutputStream stream = new ByteArrayOutputStream();   
    toXml(object, stream);
    return stream;
  }
  
  @SuppressWarnings("unchecked")
  public static <T> void toXml(T object, OutputStream stream) throws JAXBException, IOException
  {		
		Class<T> c = (Class<T>)object.getClass();
		String pkgName = c.getPackage().getName();
		JAXBContext jc = JAXBContext.newInstance(pkgName);
	  Marshaller m = jc.createMarshaller();
	  m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.TRUE);
	  m.marshal(object, stream);
  }
	
  public static <T> T read(Class<T> clazz, String filePath) throws JAXBException, FileNotFoundException 
  {
  	InputStream stream = new FileInputStream(filePath);
	  return toObject(clazz, stream);
	}
  
  public static <T> T toObject(Class<T> clazz, String xml) throws JAXBException 
  {
  	InputStream stream = new ByteArrayInputStream(xml.getBytes());
	  return toObject(clazz, stream);
	}
  
  @SuppressWarnings("unchecked")
  public static <T> T toObject(Class<T> clazz, InputStream stream) throws JAXBException 
  {
	  String pkgName = clazz.getPackage().getName();
	  JAXBContext jc = JAXBContext.newInstance(pkgName);
	  Unmarshaller u = jc.createUnmarshaller();
	  return (T)u.unmarshal(stream);
	}
}

