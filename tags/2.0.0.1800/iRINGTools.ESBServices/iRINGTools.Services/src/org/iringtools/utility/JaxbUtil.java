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
	public static <T> void write(T object, String path, boolean indent) throws JAXBException, IOException
  {		
		OutputStream stream = new FileOutputStream(path);
		toXml(object, stream, indent);
  }
	
	public static <T> String toXml(T object, boolean indent) throws JAXBException, IOException 
  {
	  OutputStream stream = toStream(object, indent);
    return stream.toString();   
  }
  
	public static <T> OutputStream toStream(T object, boolean indent) throws JAXBException, IOException
  {
    OutputStream stream = new ByteArrayOutputStream();    
    if (object != null) toXml(object, stream, indent);    
    return stream;
  }
  
  public static <T> void toXml(T object, OutputStream stream, boolean indent) throws JAXBException, IOException
  {		
    @SuppressWarnings("unchecked")
    Class<T> c = (Class<T>)object.getClass();
		String pkgName = c.getPackage().getName();
		JAXBContext jc = JAXBContext.newInstance(pkgName);
	  Marshaller m = jc.createMarshaller();
	  m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, indent);
	  m.marshal(object, stream);
  }
	
  public static <T> T read(Class<T> clazz, String filePath) throws JAXBException, FileNotFoundException 
  {
  	InputStream stream = new FileInputStream(filePath);
	  return toObject(clazz, stream);
	}
  
  public static <T> T toObject(Class<T> clazz, String xml) throws JAXBException 
  {
    if (xml == null || xml.length() == 0) return null;
    InputStream stream = new ByteArrayInputStream(xml.getBytes());
	  return toObject(clazz, stream);
	}
  
  public static <T> T toObject(Class<T> clazz, InputStream stream) throws JAXBException 
  {
	  String pkgName = clazz.getPackage().getName();
	  JAXBContext jc = JAXBContext.newInstance(pkgName);
	  Unmarshaller u = jc.createUnmarshaller();
	  @SuppressWarnings("unchecked")
	  T object = (T)u.unmarshal(stream);
	  return object;
	}
}

