package org.iringtools.utility;

import java.io.File;
import java.io.IOException;
import java.lang.reflect.Constructor;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.List;

import org.iringtools.utility.IOUtils;

public final class ObjectLoader
{ 
  private ObjectLoader() {}
  
  public static <T> T load(String className) throws ObjectLoaderException
  {    
    try
    {
      Class<T> cls = loadClass(className);    
      return cls.newInstance();
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  public static <T> T load(String className, Object... initArgs) throws ObjectLoaderException
  {  
    try
    {
      Class<T> cls = loadClass(className);      
      Constructor<T> ctor = findConstructor(cls, initArgs);      
      return (T) ctor.newInstance(initArgs);
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  public static <T> T load(String className, String path) throws ObjectLoaderException
  {
    return ObjectLoader.<T>load(className, new String[]{ path });    
  }
  
  public static <T> T load(String className, String path, Object... initArgs) throws ObjectLoaderException
  {
    return ObjectLoader.<T>load(className, new String[]{ path }, initArgs);    
  }
  
  public static <T> T load(String className, String[] paths) throws ObjectLoaderException
  {   
    try
    {
      Class<T> cls = loadClass(className, paths);
      return cls.newInstance();
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  public static <T> T load(String className, String[] paths, Object... initArgs) throws ObjectLoaderException
  {
    try
    {
      Class<T> cls = loadClass(className, paths);
      Constructor<T> ctor = findConstructor(cls, initArgs);      
      return (T) ctor.newInstance(initArgs);
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  private static <T> Class<T> loadClass(String className) throws ObjectLoaderException
  {    
    try
    {
      @SuppressWarnings("unchecked")
      Class<T> cls = (Class<T>) Class.forName(className);    
      return cls;
    }
    catch (Exception e)
    {
      throw new ObjectLoaderException(e);
    }
  }
  
  private static <T> Class<T> loadClass(String className, String[] paths) throws ObjectLoaderException
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
        //clr.close(); // resolve resource leakage warning - HL: not available in java 6 
        
        return cls;
      }
      catch (Exception e)
      {
        throw new ObjectLoaderException(e);
      }
    }
    
    throw new ObjectLoaderException("Path is unspecified.");
  }
  
  private static <T> Constructor<T> findConstructor(Class<T> cls, Object... initArgs) throws ObjectLoaderException
  {
    @SuppressWarnings("unchecked")
    Constructor<T>[] ctors = (Constructor<T>[]) cls.getDeclaredConstructors();
    
    for (Constructor<T> ctor : ctors)
    {
      Class<?>[] types = ctor.getParameterTypes();
      
      if (types.length == initArgs.length)
      {
        boolean argsMatched = true;
        
        for (int i = 0; i < initArgs.length; i++)
        {
          if (!types[i].getName().equals(initArgs[i].getClass().getName()))
          {
            argsMatched = false;
            break;
          }
        }
        
        if (argsMatched)
        {
          return ctor;
        }
      }
    }
    
    throw new ObjectLoaderException("No matching constructor found.");
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
