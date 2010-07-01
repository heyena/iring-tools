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
		serialize(object, stream);
  }
	
	public static <T> String serialize(T object) throws JAXBException, IOException 
	{
		OutputStream stream = new ByteArrayOutputStream();   
		serialize(object, stream);
		return stream.toString();   
	}
	
	@SuppressWarnings("unchecked")
  public static <T> void serialize(T object, OutputStream stream) throws JAXBException, IOException
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
	  return deserialize(clazz, stream);
	}
  
  public static <T> T deserialize(Class<T> clazz, String document) throws JAXBException 
  {
  	InputStream stream = new ByteArrayInputStream(document.getBytes());
	  return deserialize(clazz, stream);
	}
  
  @SuppressWarnings("unchecked")
  public static <T> T deserialize(Class<T> clazz, InputStream stream) throws JAXBException 
  {
	  String pkgName = clazz.getPackage().getName();
	  JAXBContext jc = JAXBContext.newInstance(pkgName);
	  Unmarshaller u = jc.createUnmarshaller();
	  return (T)u.unmarshal(stream);
	}
}

