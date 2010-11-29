package org.iringtools.utility;

import java.io.IOException;

import javax.xml.bind.JAXBException;
import com.google.gson.Gson;

public class JsonUtil
{
	public static <T> void write(Object object, String filePath) throws IOException
	{
		String json = serialize(object);
		IOUtil.writeString(json, filePath);
	}
	
	public static String serialize(Object object)
	{
		Gson gson = new Gson();
		return gson.toJson(object);	
	}
	
	public static <T> T read(Class<T> clazz, String filePath) throws JAXBException, IOException 
	{
		return deserialize(clazz, IOUtil.readString(filePath)); 
	}
	
	public static <T> T deserialize(Class<T> clazz, String json)
	{
		Gson gson = new Gson();
		return (T)gson.fromJson(json, clazz);
	}
}
