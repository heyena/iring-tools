package org.iringtools.library.directory;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  private Map<String, Object> settings;

  public DirectoryProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public Directory getDirectory() throws Exception
  {
    String path = settings.get("basePath").toString().concat("WEB-INF/data/directory.xml");

    if (IOUtils.fileExists(path))
    {
      return JaxbUtils.read(Directory.class, path);
    }
    else
    {
      logger.info("Directory file does not exist. Create empty one.");
      
      Directory directory = new Directory();
      JaxbUtils.write(directory, path, false);

      return directory;
    }
  }
}
