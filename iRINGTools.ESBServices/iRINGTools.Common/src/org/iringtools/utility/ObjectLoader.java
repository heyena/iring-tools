package org.iringtools.utility;

import java.io.File;
import java.io.IOException;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.List;

import org.iringtools.utility.IOUtils;

public final class ObjectLoader
{ 
  private ObjectLoader(){}
  
  public static <T> T load(String className) throws ObjectLoaderException
  {    
    try
    {
      @SuppressWarnings("unchecked")
      Class<T> cls = (Class<T>) Class.forName(className);    
      return cls.newInstance();
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  public static <T> T load(String className, String path) throws ObjectLoaderException
  {
    return load(className, new String[]{ path });    
  }
  
  public static <T> T load(String className, String[] paths) throws ObjectLoaderException
  {
    if (paths.length > 0)
    {
      try
      {
        List<URL> urls = new ArrayList<URL>();
        
        for (String path : paths)
        {
          if (!path.endsWith(String.valueOf(File.separatorChar)))
          {
            path += File.separatorChar;
          }
          
          urls.add(new URL("file:/" + path));
          urls.addAll(getJarURLs(path));
        }
        
        URL[] urlArray = new URL[urls.size()];
        URLClassLoader clr = new URLClassLoader(urls.toArray(urlArray));
        
        @SuppressWarnings("unchecked")
        Class<T> cls = (Class<T>) clr.loadClass(className);
        
        return cls.newInstance();
      }
      catch (Exception e)
      {
        throw new ObjectLoaderException(e);
      }
    }
    
    throw new ObjectLoaderException("Path can not be empty.");
  }
  
  private static List<URL> getJarURLs(String path) throws IOException
  {
    List<URL> jarURLs = new ArrayList<URL>();    
    List<String> files = IOUtils.getFiles(path);
    
    for (String file : files)
    {
      if (file.toLowerCase().endsWith(".jar"))
      {
        jarURLs.add(new URL("file:/" + path + file));
      }
    }
    
    return jarURLs;
  }
}
