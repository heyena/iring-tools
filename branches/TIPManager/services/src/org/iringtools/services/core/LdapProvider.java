package org.iringtools.services.core;

import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Properties;

import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.directory.Attributes;
import javax.naming.directory.BasicAttribute;
import javax.naming.directory.DirContext;
import javax.naming.directory.ModificationItem;
import javax.naming.directory.SearchControls;
import javax.naming.directory.SearchResult;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Endpoints;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Exchanges;
import org.iringtools.directory.Folder;
import org.iringtools.directory.Folders;
import org.iringtools.directory.Locator;
import org.iringtools.directory.Locators;
import org.iringtools.directory.Resource;
import org.iringtools.directory.Resources;
import org.iringtools.ldap.tree.Node;
import org.iringtools.ldap.tree.Tree;
import org.iringtools.security.LdapEndpoint;
import org.iringtools.security.LdapExchange;
import org.iringtools.security.LdapFolder;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class LdapProvider extends ResourceProvider
{
  private static final Logger logger = Logger.getLogger(LdapProvider.class);

  private DirContext dctx = null;
  private String baseDir = "o=iringtools,dc=iringug,dc=org"; // ou = directory, ou = groups, ou = users, ou = roles
  private String groupCn = "ou=groups";
  private String userCn = "ou=users";
  private String directoryCn = "ou=directory";
  private String roleCn = "ou=rights", delBase = "ou=directory,o=test,dc=iringug,dc=org";
  private String oldLdapFullName, newLdapFullName, oldLdapName, newLdapName;

  public LdapProvider(Map<String, Object> settings)
  {
    super();
    this.settings = settings;
    setDirContext(settings);
  }

  public LdapProvider()
  {
    super();
    setDirContext();
  }

  private void setDirContext()
  {
    try
    {
      String path = (settings == null || settings.size() == 0)
        ? System.getProperty("user.dir")
        : settings.get("baseDirectory").toString() + "/WEB-INF";
      
      String ldapConfigPath = path + "/config/ldap.conf";
      
      Properties properties = new Properties();
      properties.load(new FileInputStream(ldapConfigPath));
      
      dctx = IOUtils.initDirContext(properties);
    }
    catch (Exception e)
    {
      logger.error("Error initializing directory context: " + e.getMessage());
    }
  }

  public void setDirContext(Map<String, Object> settings)
  {
    this.settings = settings;
    setDirContext();
  }

  public void closeContext()
  {
    try
    {
      dctx.close();
    }
    catch (Exception e)
    {
      logger.error(e.getMessage());
    }
  }

  public void storeDirectoryEndpoint(LdapEndpoint dirEntry, String base_dir)
  {
    if (dctx != null)
    {
      String cName = "cn=" + dirEntry.getName() + ",";
      String nodeName = cName + base_dir;
      bindLdapEndpoint(dirEntry, nodeName);
    }
  }

  public void storeDirectoryExchange(LdapExchange dirEntry, String base_dir)
  {
    if (dctx != null)
    {
      String cName = "cn=" + dirEntry.getName() + ",";
      String nodeName = cName + base_dir;

      try
      {
        dctx.bind(nodeName, dirEntry);
      }
      catch (Exception ex)
      {
        logger.error(ex.getMessage().toString());
      }
    }
  }

  public String getSecurityRole(String user)
  {
    String[] attrIDs = { "member, cn" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    String filter = "member=uid=" + user + "," + userCn + "," + baseDir;
    String role = "", group;

    try
    {
      NamingEnumeration<?> queryGroupResult = dctx.search(groupCn + "," + baseDir, filter, ctls);

      while (queryGroupResult.hasMore())
      {
        SearchResult groupEntry = (SearchResult) queryGroupResult.next();
        ctls = new SearchControls();
        ctls.setReturningAttributes(attrIDs);
        ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
        group = groupEntry.getName().toLowerCase();
        filter = "(&(member=" + group + "," + groupCn + "," + baseDir + ")(cn=*))";
        NamingEnumeration<?> queryRoleResult = dctx.search(roleCn + "," + baseDir, filter, ctls);

        while (queryRoleResult.hasMore())
        {
          SearchResult roleEntry = (SearchResult) queryRoleResult.next();
          role = addRoles(removeHeader("cn", roleEntry.getName().toLowerCase()), role);
        }
      }
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }

    return role;
  }

  private String getGroups(String user)
  {
    String[] attrIDs = { "member, cn" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    String filter = "member=uid=" + user + "," + userCn + "," + baseDir;
    String superGroup = "", adminGroup = "", group = "";

    try
    {
      NamingEnumeration<?> queryGroupResult = dctx.search(groupCn + "," + baseDir, filter, ctls);

      while (queryGroupResult.hasMore())
      {
        SearchResult groupEntry = (SearchResult) queryGroupResult.next();
        ctls = new SearchControls();
        ctls.setReturningAttributes(attrIDs);
        ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
        group = groupEntry.getName().toLowerCase();
        filter = "(&(member=" + group + "," + groupCn + "," + baseDir + ")(cn=*))";
        NamingEnumeration<?> queryRoleResult = dctx.search(roleCn + "," + baseDir, filter, ctls);

        while (queryRoleResult.hasMore())
        {
          SearchResult roleEntry = (SearchResult) queryRoleResult.next();
          if (removeHeader("cn", roleEntry.getName().toLowerCase()).equals("rootadmin"))
          	superGroup = group;
          
          if (removeHeader("cn", roleEntry.getName().toLowerCase()).equals("treenodeadmin"))
            adminGroup = group;
        }
      }
      
      if (superGroup != "")
      	return superGroup;
      
      if (adminGroup != "")
      	return adminGroup;
      
      return null;
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }

    return null;
  }

  public Directory getDirectory(String user)
  {
    logger.debug("Creating directory for user [" + user + "]");
    
    Tree tree = new Tree();
    String[] attrIDs = { "member, cn" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    String filter = "member=uid=" + user + "," + userCn + "," + baseDir;
    String role, baseName, group;

    try
    {
      NamingEnumeration<?> queryGroupResult = dctx.search(groupCn + "," + baseDir, filter, ctls);

      while (queryGroupResult.hasMore())
      {
        role = "";
        SearchResult groupEntry = (SearchResult) queryGroupResult.next();
        ctls = new SearchControls();
        ctls.setReturningAttributes(attrIDs);
        ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
        group = groupEntry.getName().toLowerCase();
        filter = "(&(member=" + group + "," + groupCn + "," + baseDir + ")(cn=*))";
        NamingEnumeration<?> queryRoleResult = dctx.search(roleCn + "," + baseDir, filter, ctls);

        while (queryRoleResult.hasMore())
        {
          SearchResult roleEntry = (SearchResult) queryRoleResult.next();
          role = addRoles(removeHeader("cn", roleEntry.getName().toLowerCase()), role);
        }

        String[] directoryAttrIDs = { "type" };
        ctls = new SearchControls();
        ctls.setReturningAttributes(directoryAttrIDs);
        ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
        baseName = directoryCn + "," + baseDir;

        if (role.contains("rootadmin"))
        {
          filter = "cn=*";
        }

        NamingEnumeration<?> queryDirectoryResult = dctx.search(baseName, filter, ctls);

        while (queryDirectoryResult.hasMore())
        {
          SearchResult directoryEntry = (SearchResult) queryDirectoryResult.next();
          Attributes attrs = directoryEntry.getAttributes();
          tree = getPartTree(directoryEntry, tree, role, baseName, removeHeader("type", attrs.get("type").toString()));
        }
      }

      Directory d = getDirectoryFromTree(tree);
      //System.out.println(JaxbUtils.toXml(d, true));
      return d;
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }
    return null;
  }

  private Tree getPartTree(SearchResult entry, Tree tree, String role, String baseName, String type)
  {
    String lpath = entry.getName();
    traverseLpath(lpath, tree, baseName, "user");
    Node node = tree.searchNode(lpath + "," + baseName);
    addRolesToNode(role, node);

    if (type.compareTo("endpoint") != 0 || type.compareTo("exchange") != 0)
      addSubTree(lpath, tree, role, baseName, 0);

    return tree;
  }

  private void traverseLpath(String lpath, Tree tree, String baseName, String role)
  {
    int index = lpath.indexOf(",");
    Node node = null;
    String lpathFullName = lpath + "," + baseName;

    if (index > 0)
    {
      String parentLpath = lpath;
      node = tree.searchNode(lpathFullName);

      if (node != null)
      {
        tree.setNodeAddSibling(node);
        addRolesToNode(role, node);
        return;
      }
      else
      {
        lpath = lpath.substring(index + 1);
        traverseLpath(lpath, tree, baseName, role);
      }

      lpathFullName = parentLpath + "," + baseName;
      node = tree.searchNode(lpathFullName);

      if (node != null)
      {
        tree.setNodeAddSibling(node);
        return;
      }

      addNodeToTree(tree, parentLpath, role, baseName);
      tree.setNodeAddSibling(null);
      return;
    }
    else
    {
      if (tree.getNodes() != null)
      {
        node = tree.searchNode(lpathFullName);

        if (node != null)
        {
          tree.setNodeAddSibling(node);

          return;
        }

        tree.setNodeAddSibling(new Node("newbranch"));
      }
      addNodeToTree(tree, lpath, role, baseName);
      tree.setNodeAddSibling(null);
      return;
    }
  }

  private void addNodeToTree(Tree tree, String lpath, String role, String baseName)
  {
    String type = "", name, description, context = "", baseUrl = "", assembly = "";
    Node treeNode = new Node();
    String ldapFullName = lpath + "," + baseName;
    treeNode.setLdapName(ldapFullName);
    treeNode.setSecurityRole(role);
    List<Node> nodeList = new ArrayList<Node>();

    try
    {
      Attributes attributes = dctx.getAttributes(ldapFullName);

      if (attributes.get("type") != null)
      {
        type = removeHeader("type", attributes.get("type").toString());
        treeNode.setType(type);
      }
      if (attributes.get("name") != null)
      {
        name = removeHeader("name", attributes.get("name").toString());
        treeNode.setName(name);
      }
      if (attributes.get("description") != null)
      {
        description = removeHeader("description", attributes.get("description").toString());
        treeNode.setDescription(description);
      }
      if (attributes.get("context") != null)
      {
        context = removeHeader("context", attributes.get("context").toString());
        treeNode.setContext(context);
      }
      if (attributes.get("baseurl") != null)
      {
        baseUrl = removeHeader("baseurl", attributes.get("baseurl").toString());
        treeNode.setBaseUrl(baseUrl);
      }
      if (attributes.get("assembly") != null)
      {
        assembly = removeHeader("assembly", attributes.get("assembly").toString());
        treeNode.setAssembly(assembly);
      }
      if (type.compareTo("endpoint") == 0)
      {

      }
      else if (type.compareTo("exchange") == 0)
      {
        String id;

        if (attributes.get("id") != null)
        {
          id = removeHeader("id", attributes.get("id").toString());
          treeNode.setDescription(id);
        }
      }

      if (tree.getNodeAddChild() == null)
      {
        tree.setNodes(nodeList);
        nodeList.add(treeNode);
      }
      else if (tree.getNodeAddSibling() != null)
      {
        Node sibliingParentNode = tree.getNodeAddSibling();

        if (sibliingParentNode.getName().compareTo("newbranch") == 0)
          nodeList = tree.getNodes();
        else
          nodeList = sibliingParentNode.getChildren();

        nodeList.add(treeNode);
        treeNode.setParentNode(sibliingParentNode);
      }
      else
      {
        Node parentNode = tree.getNodeAddChild();
        parentNode.setChildren(nodeList);
        nodeList.add(treeNode);
        treeNode.setParentNode(parentNode);
      }

      tree.setNodeAddChild(treeNode);
    }
    catch (Exception ex)
    {
      logger.error(ex.toString());
    }
  }

  private void addRolesToChilren(Node node)
  {
    String multiRoles = node.getSecurityRole();

    for (Node child : node.getChildren())
    {
      child.setSecurityRole(multiRoles);
      addRolesToChilren(child);
    }
  }

  private void addRolesToNode(String role, Node node)
  {
    String multiRoles;

    if (node.getSecurityRole() != null)
    {
      multiRoles = addRoles(role, node.getSecurityRole());
    }
    else
      multiRoles = role;

    node.setSecurityRole(multiRoles);
  }

  private String addRoles(String newRole, String possessedRole)
  {
    if (newRole != null)
    {
      if (newRole != "" && possessedRole != "" && newRole.compareTo(possessedRole) != 0)
      {
        if (Integer.parseInt(rightPriority.get(newRole.toLowerCase())) < Integer.parseInt(rightPriority
            .get(possessedRole.toLowerCase())))
          return newRole;
        else
          return possessedRole;
      }
      else if (newRole != "")
        return newRole;
    }
    return possessedRole;
  }

  private String getRealString(String valve)
  {
    if (valve.equals("."))
      return "";
    else
      return valve;
  }

  private Directory getDirectoryFromTree(Tree tree)
  {
    Directory directory = new Directory();
    List<Folder> folderList = new ArrayList<Folder>();
    directory.setFolderList(folderList);

    for (Node node : tree.getNodes())
    {
      Folder folder = new Folder();

      folder.setType(node.getType());
      folder.setName(node.getName());
      folder.setDescription(getRealString(node.geDescription()));
      folder.setSecurityRole(node.getSecurityRole());
      folder.setContext(getRealString(node.getContext()));
      folderList.add(folder);
      traverseTree(directory, node, folder);
    }
    return directory;
  }

  private String tryTrim(String value)
  {
    if (value != null)
      return value.trim();
    return null;
  }

  private void traverseTree(Directory directory, Node node, Folder folder)
  {
    String type, description, name, securityRole, id, context, baseUrl, assembly;

    for (Node child : node.getChildren())
    {
      type = child.getType();
      description = child.geDescription();
      name = child.getName();
      securityRole = child.getSecurityRole();
      context = child.getContext();
      baseUrl = child.getBaseUrl();
      assembly = child.getAssembly();

      if (type != null)
      {
        if (type.compareTo("endpoint") == 0)
        {
          List<Endpoint> endpointList = null;
          Endpoints endpoints = null;
          Endpoint endpoint = new Endpoint();
          endpoint.setName(name);
          endpoint.setContext(context);
          endpoint.setBaseUrl(getRealString(baseUrl));
          endpoint.setAssembly(assembly);
          endpoint.setDescription(getRealString(description));
          endpoint.setSecurityRole(securityRole);
          endpoint.setType(type);

          if (folder.getEndpoints() != null)
          {
            endpoints = folder.getEndpoints();
            if (folder.getEndpoints().getItems() != null)
              endpointList = folder.getEndpoints().getItems();
            else
              endpointList = new ArrayList<Endpoint>();
          }
          else
          {
            endpointList = new ArrayList<Endpoint>();
            endpoints = new Endpoints();
          }

          endpointList.add(endpoint);
          endpoints.setItems(endpointList);
          folder.setEndpoints(endpoints);

        }
        else if (type.compareTo("exchange") == 0)
        {
          id = child.getId();
          Exchange exchange = new Exchange();
          exchange.setDescription(getRealString(description));
          exchange.setContext(getRealString(context));
          exchange.setId(id);
          exchange.setName(name);
          exchange.setSecurityRole(securityRole);
          Exchanges exchanges = null;
          List<Exchange> exchangeList = null;

          if (folder.getExchanges() != null)
          {
            exchanges = folder.getExchanges();
            if (folder.getExchanges().getItems() != null)
              exchangeList = folder.getExchanges().getItems();
            else
              exchangeList = new ArrayList<Exchange>();
          }
          else
          {
            exchangeList = new ArrayList<Exchange>();
            exchanges = new Exchanges();
          }

          exchangeList.add(exchange);
          exchanges.setItems(exchangeList);
          folder.setExchanges(exchanges);
        }
        else if (type.compareTo("folder") == 0)
          addFolderToDirectory(type, description, securityRole, name, folder, directory, child, context);
      }
      else
      {
        addFolderToDirectory(type, description, securityRole, name, folder, directory, child, context);
      }
    }
  }

  private void addFolderToDirectory(String type, String description, String securityRole, String name, Folder folder,
      Directory directory, Node child, String context)
  {
    List<Folder> newFolderList = null;
    Folders newFolders = null;
    Folder newFolder = new Folder();
    newFolder.setType(type);
    newFolder.setDescription(getRealString(description));
    newFolder.setName(name);
    newFolder.setSecurityRole(securityRole);
    newFolder.setContext(getRealString(context));

    if (folder.getFolders() != null)
    {
      newFolders = folder.getFolders();
      if (folder.getFolders().getItems() != null)
        newFolderList = folder.getFolders().getItems();
      else
        newFolderList = new ArrayList<Folder>();
    }
    else
    {
      newFolderList = new ArrayList<Folder>();
      newFolders = new Folders();
    }

    newFolderList.add(newFolder);
    newFolders.setItems(newFolderList);
    folder.setFolders(newFolders);
    traverseTree(directory, child, newFolder);
  }

  private String removeHeader(String header, String value)
  {
    return tryTrim(value.substring(header.length() + 1, value.length()));
  }

  private void addSubTree(String lpath, Tree tree, String role, String baseName, int goback)
  {
    SearchControls ctls = new SearchControls();
    String[] attrIDs = { "cn" };
    String filter = "cn=*";
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.ONELEVEL_SCOPE);
    String entryName = "", newBaseName;

    try
    {
      newBaseName = lpath + "," + baseName;
      NamingEnumeration<?> queryResult = dctx.search(newBaseName, filter, ctls);

      if (queryResult != null)
      {
        while (queryResult.hasMore())
        {
          SearchResult entry = (SearchResult) queryResult.next();
          entryName = entry.getName();
          addNodeToSubTree(tree, entryName, role, newBaseName, goback);
          goback = 0;
          addSubTree(entryName, tree, role, newBaseName, goback);
          goback = 1;
        }
      }
    }
    catch (Exception ex)
    {
      logger.error(ex.toString());
    }
  }

  private void addNodeToSubTree(Tree tree, String entryName, String role, String newBaseName, int goback)
  {
    if (goback == 1)
    {
      Node node = tree.searchNode(newBaseName);
      tree.setNodeAddSibling(node);
    }

    String lpathFullName = entryName + "," + newBaseName;
    Node node = tree.searchNode(lpathFullName);

    if (node != null)
    {
      tree.setNodeAddSibling(node);
      addRolesToNode(role, node);
      addRolesToChilren(node);
      return;
    }
    addNodeToTree(tree, entryName, role, newBaseName);
    tree.setNodeAddSibling(null);
  }

  public String storeDirectoryFolder(LdapFolder dirEntry, String base_dir)
  {
    if (dctx != null)
    {
      String cName = "cn=" + dirEntry.getName() + ",";
      String nodeName = cName + base_dir;
      base_dir = nodeName;
      bindLdapFolder(dirEntry, nodeName);
    }

    return base_dir;
  }

  private void bindLdapFolder(LdapFolder ldapObject, String ldapFullPath)
  {
    try
    {
      dctx.bind(ldapFullPath, ldapObject);
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }
  }

  private void bindLdapEndpoint(LdapEndpoint ldapObject, String ldapFullPath)
  {
    try
    {
      dctx.bind(ldapFullPath, ldapObject);
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }
  }

  private void getLdapFullPathFromPath(String path, String name)
  {
    path = path.replace('.', '/');
    String[] level = path.split("/");
    int maxIndex = level.length - 1;
    oldLdapName = "cn=" + level[maxIndex];
    newLdapName = "cn=" + name;

    if (maxIndex >= 1)
    {
      if (maxIndex > 2)
      {
        for (int i = maxIndex - 1; i >= 0; i--)
        {
          oldLdapName = oldLdapName + ",cn=" + level[i];
          newLdapName = newLdapName + ",cn=" + level[i];
        }
      }
      else if (maxIndex > 1)
      {
        oldLdapName = oldLdapName + ",cn=" + level[1] + ",cn=" + level[0];
        newLdapName = newLdapName + ",cn=" + level[1] + ",cn=" + level[0];
      }
      else
      {
        oldLdapName = oldLdapName + ",cn=" + level[0];
        newLdapName = newLdapName + ",cn=" + level[0];
      }
    }

    oldLdapFullName = oldLdapName + "," + directoryCn + "," + baseDir;
    newLdapFullName = newLdapName + "," + directoryCn + "," + baseDir;
  }

  public boolean hasLdapNode(String ldapFullPath)
  {
    boolean hasTheNode = false;
    String[] attrIDs = { "cn" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    String filter = "cn=*";
    try
    {
      NamingEnumeration<?> queryGroupResult = dctx.search(ldapFullPath, filter, ctls);

      if (queryGroupResult != null)
        hasTheNode = true;
    }
    catch (Exception ex)
    {}
    return hasTheNode;
  }

  public void updateDirectoryNode(String user, String path, String name, String type, String description,
      String context, String baseUrl, String assembly)
  {    
    String nameLastPath, currentContext = "";
    String group = getGroups(user);
    int indexOfPoint = path.indexOf('.');

    if (indexOfPoint > -1)
      nameLastPath = path.substring(path.lastIndexOf('.') + 1);
    else
      nameLastPath = path;

    getLdapFullPathFromPath(path, name);

    if (hasLdapNode(oldLdapFullName))
    {
      try
      {
        ModificationItem[] mods;

        Attributes attributes = dctx.getAttributes(oldLdapFullName);

        if (attributes.get("context") != null)
          currentContext = removeHeader("context", attributes.get("context").toString());

        if (type.compareTo("folder") == 0)
        {
          if (!currentContext.equals(context))
          {
            // changeContext(context, oldLdapFullName);
            modifyContext(context, oldLdapFullName);
          }

          mods = new ModificationItem[3];
          mods[0] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("name", name));
          mods[1] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("description", description));
          mods[2] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("context", context));

          dctx.modifyAttributes(oldLdapFullName, mods);
        }
        else if (type.compareTo("endpoint") == 0)
        {
          mods = new ModificationItem[4];
          mods[0] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("name", name));
          mods[1] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("description", description));
          mods[2] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("baseurl", baseUrl));
          mods[3] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("assembly", assembly));

          dctx.modifyAttributes(oldLdapFullName, mods);          
        }
      }
      catch (Exception ex)
      {
        logger.error(ex.toString());
      }

      if (nameLastPath.compareTo(name) != 0)
      {
        try
        {
          dctx.rename(oldLdapFullName, newLdapFullName);
        }
        catch (NamingException ex)
        {
          logger.error(ex.getMessage().toString());
        }
      }
    }
    else
    {
      if (type.compareTo("folder") == 0)
      {
      	String memeber = "";
        
      	if (indexOfPoint < 0)
        {
          memeber = group + "," + groupCn + "," + baseDir;
        }
          
        LdapFolder directoryEntry = new LdapFolder(name, description, type, context, memeber);
        bindLdapFolder(directoryEntry, newLdapFullName);        
      }
      else if (type.compareTo("endpoint") == 0)
      {
        String endpointContext = getParentContext(newLdapFullName);
        LdapEndpoint directoryEndpoint = new LdapEndpoint(name, description, type, endpointContext, baseUrl, assembly, "");
        bindLdapEndpoint(directoryEndpoint, newLdapFullName);
      }
    }
  }

  public void deleteTree()
  {
    traverseDeleteTree(delBase);
  }

  public void traverseDeleteTree(String traverseDelBase)
  {
    String[] attrIDs = { "cn", "name" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.ONELEVEL_SCOPE);
    String name, newDelBase;

    try
    {
      NamingEnumeration<?> queryResult = dctx.search(traverseDelBase, "cn=*", ctls);

      while (queryResult.hasMore())
      {
        SearchResult entry = (SearchResult) queryResult.next();
        Attributes attrs = entry.getAttributes();
        name = removeHeader("name", attrs.get("name").toString());

        if (name.isEmpty())
          continue;

        newDelBase = "cn=" + name + "," + traverseDelBase;
        traverseDeleteTree(newDelBase);
        dctx.unbind(newDelBase);
      }
    }
    catch (Exception ex)
    {
      if (!delBase.equals(""))
        traverseDeleteTree(delBase);
    }
  }

  private void modifyContext(String context, String ldapFullPath)
  {
    SearchControls ctls = new SearchControls();
    String[] attrIDs = { "cn" };
    String filter = "cn=*";
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.ONELEVEL_SCOPE);
    String entryName = "", childLdapFullPath;

    try
    {
      NamingEnumeration<?> queryResult = dctx.search(ldapFullPath, filter, ctls);

      if (queryResult != null)
      {
        while (queryResult.hasMore())
        {
          SearchResult entry = (SearchResult) queryResult.next();
          entryName = entry.getName();
          childLdapFullPath = entryName + "," + ldapFullPath;
          changeContext(context, childLdapFullPath);
          modifyContext(context, childLdapFullPath);
        }
      }
    }
    catch (Exception ex)
    {
      logger.error(ex.toString());
    }
  }

  private void changeContext(String context, String ldapFullPath)
  {
    ModificationItem[] mods = new ModificationItem[1];
    mods[0] = new ModificationItem(DirContext.REPLACE_ATTRIBUTE, new BasicAttribute("context", context));

    try
    {
     dctx.modifyAttributes(ldapFullPath, mods);
    }
    catch (Exception ex)
    {
      logger.error(ex.toString());
    }
  }

  private String getParentContext(String ldapFullPath)
  {
    String context = "";
    int index = ldapFullPath.indexOf(",");

    if (index >= 0)
    {
      String parentLdapFullPath = ldapFullPath.substring(index + 1);

      try
      {
        Attributes attributes = dctx.getAttributes(parentLdapFullPath);

        if (attributes.get("context") != null)
        {
          context = removeHeader("context", attributes.get("context").toString());
        }
      }
      catch (Exception ex)
      {
        logger.error(ex.getMessage().toString());
      }
    }
    return context;
  }

  private boolean hasChildren(String path)
  {
    String[] attrIDs = { "cn" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
    String filter = "cn=*";
    int childrenNumber = 0;
    try
    {
      NamingEnumeration<?> queryGroupResult = dctx.search(path, filter, ctls);

      while (queryGroupResult.hasMore())
      {
        childrenNumber++;
        if (childrenNumber > 1)
        {
          return true;
        }
      }
    }
    catch (Exception ex)
    {}
    return false;
  }

  public void deleteLdapItem(String path)
  {
    getLdapFullPathFromPath(path, "");

    try
    {
      if (hasChildren(oldLdapFullName))
      {
        delBase = oldLdapFullName;
        traverseDeleteTree(oldLdapFullName);
      }

      dctx.destroySubcontext(oldLdapFullName);

    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }
  }

  public void setBaseUri(String baseDir)
  {
    this.baseDir = baseDir;
  }

  private String toPath(String lpath)
  {
  	String path = "";
  	String pathItem[] = lpath.substring(3).split(",cn=");
  	for (int i = pathItem.length-1; i >= 0; i--)
  	{
  		path = path + pathItem[i];
  		if (i > 0)
  			path = path + "/";
  	}
  	return path;
  }
  
  public Resources getResources()
  {
    Resources recources = new Resources();
    List<Resource> resoureList = recources.getResourceList();    

    String[] attrIDs = { "type", "context", "baseurl", "name", "assembly", "description" };
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.SUBTREE_SCOPE);
    String filter = "(type=endpoint)";
    String baseUrl = "";       
    List<Locator> locatorList = null;
    
    try
    {
      NamingEnumeration<?> queryResourceResult = dctx.search(directoryCn + "," + baseDir, filter, ctls);

      while (queryResourceResult.hasMore())
      {
        SearchResult resourceEntry = (SearchResult) queryResourceResult.next();
        Attributes attrs = resourceEntry.getAttributes();

        if (attrs.get("context") == null || attrs.get("name") == null || attrs.get("baseurl") == null)
          continue;        
        
        baseUrl = removeHeader("baseurl", attrs.get("baseurl").toString());        
        locatorList = createResource(resoureList, baseUrl);              
        prepareResource(resourceEntry, attrs, locatorList);        
      }
      return recources;
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage().toString());
    }

    return null;
  }
  
  public Resource getResource(String baseUrl)
  {
  	Resource recource = new Resource();  	
  	recource.setBaseUrl(baseUrl);    	
  	Locators locators = new Locators();	
  	List<Locator> locatorList = locators.getItems();  
  	recource.setLocators(locators); 
	    
  	String[] attrIDs = {"type", "context", "baseurl", "name", "assembly", "description"};
    SearchControls ctls = new SearchControls();
    ctls.setReturningAttributes(attrIDs);
    ctls.setSearchScope(SearchControls.SUBTREE_SCOPE); 
    String filter = "(&(type=endpoint)(baseurl=" + baseUrl + "))";     
    
    try
  	{  		
    	NamingEnumeration<?> queryResourceResult = dctx.search(directoryCn + "," + baseDir, filter, ctls); 
    	
    	while (queryResourceResult.hasMore())
    	{
    		SearchResult resourceEntry = (SearchResult)queryResourceResult.next();   
    		Attributes attrs = resourceEntry.getAttributes(); 
    		
    		if (attrs.get("context") == null || attrs.get("name") == null || attrs.get("assembly") == null)
    			continue;
    		
    		prepareResource(resourceEntry, attrs, locatorList);
    	}
    	
    	return recource;
  	}
    catch(Exception ex)
  	{
  		logger.error(ex.getMessage().toString());
  	}
    			
  	return null;
  }
  
  private void prepareResource(SearchResult resourceEntry, Attributes attrs, List<Locator> locatorList)
  {
  	String description = "", assembly = "", lpath = "";
  	if (resourceEntry.getName() != null)
    	lpath = toPath(resourceEntry.getName());
    
    String context = removeHeader("context", attrs.get("context").toString());
    String endpoint = removeHeader("name", attrs.get("name").toString());
		
		if (attrs.get("description") != null)
    	description = removeHeader("description", attrs.get("description").toString());
    
    if (attrs.get("assembly") != null)
    	assembly = removeHeader("assembly", attrs.get("assembly").toString());
    
		setResource(locatorList, context, assembly, endpoint, description, lpath);
  }
}
