package org.iringtools.services.core;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Endpoints;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.directory.Exchanges;
import org.iringtools.directory.Folder;
import org.iringtools.security.LdapEndpoint;
import org.iringtools.security.LdapExchange;
import org.iringtools.security.LdapFolder;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);

  private Map<String, Object> settings = null;
  private String groupBase = "ou=groups,o=test,dc=iringug,dc=org";
  private String base_dir = "ou=directory,o=iringtools,dc=iringug,dc=org";

  public DirectoryProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public void setBaseDir(String base_dir)
  {
    this.base_dir = base_dir;
  }

  public Directory getDirectory() throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";

    try
    {
      if (IOUtils.fileExists(path))
      {
        return JaxbUtils.read(Directory.class, path);
      }

      Directory directory = new Directory();
      JaxbUtils.write(directory, path, false);
      return directory;
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definitions: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  public ExchangeDefinition getExchangeDefinition(String scope, String id) throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + scope + "-" + id + ".xml";
    try
    {
      return JaxbUtils.read(ExchangeDefinition.class, path);
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definition of [" + scope + "." + id + "]: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  public void postDirectory(Directory directory, String baseDir) throws ServiceProviderException
  {
    LdapProvider ldapProvider = null;

    if (settings != null)
    {
      ldapProvider = new LdapProvider(settings);
    }
    else
    {
      ldapProvider = new LdapProvider();
    }

    if (baseDir != null)
      ldapProvider.setBaseUri(baseDir);

    postDirectoryToLdap(directory, ldapProvider);
  }

  public void postDirectoryToLdap(Directory directory, LdapProvider ldapProvider) throws ServiceProviderException
  {
    String base_dir1;

    try
    {
      for (Folder folder : directory.getFolderList())
      {
        LdapFolder dirFolder = new LdapFolder(folder.getName(), folder.getDescription(), folder.getType(),
            folder.getContext(), getMember(folder.getGroup(), folder));
        base_dir1 = ldapProvider.storeDirectoryFolder(dirFolder, base_dir);
        traverseDirectory(folder, base_dir1, ldapProvider);
      }
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definitions: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  private String traverseDirectory(Folder folder, String base_dir, LdapProvider ldapProvider)
  {
    Endpoints endpoints = folder.getEndpoints();
    Exchanges exchanges = folder.getExchanges();
    String baseDir1;

    if (endpoints != null)
    {
      for (Endpoint endpoint : endpoints.getItems())
      {
        LdapEndpoint dirEndpoint = new LdapEndpoint(endpoint.getName(), endpoint.getDescription(), endpoint.getType(),
            endpoint.getContext(), endpoint.getBaseUrl(), endpoint.getAssembly(), getMember(endpoint.getGroup(), endpoint));
        ldapProvider.storeDirectoryEndpoint(dirEndpoint, base_dir);
      }
    }

    if (exchanges != null)
    {
      for (Exchange exchange : exchanges.getItems())
      {
        LdapExchange dirExchange = new LdapExchange(exchange.getId(), exchange.getName(), exchange.getDescription(),
            exchange.getContext(), "");
        ldapProvider.storeDirectoryExchange(dirExchange, base_dir);
      }
    }

    if (folder.getFolders() == null)
      return base_dir;
    else
    {
      for (Folder subFolder : folder.getFolders().getItems())
      {
        LdapFolder dirEntry = new LdapFolder(subFolder.getName(), subFolder.getDescription(), subFolder.getType(),
            subFolder.getContext(), getMember(subFolder.getGroup(), subFolder));
        baseDir1 = ldapProvider.storeDirectoryFolder(dirEntry, base_dir);
        traverseDirectory(subFolder, baseDir1, ldapProvider);
      }
    }
    return base_dir;
  }

  private String getMember(String group, Folder folder)
  {
    String member = "";
    group = folder.getGroup();

    if (group != null)
      member = "cn=" + group + "," + groupBase;

    return member;
  }

  private String getMember(String group, Endpoint endpoint)
  {
    String member = "";
    group = endpoint.getGroup();

    if (group != null)
      member = "cn=" + group + "," + groupBase;

    return member;
  }

}
