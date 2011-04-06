package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.StringTokenizer;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.NamespaceList;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;
import org.iringtools.utility.HttpClient;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;

import com.opensymphony.xwork2.ActionContext;

public class FederationModel
{
  private Federation federation = null;
  private HttpClient httpClient = null;

  public FederationModel()
  {
    try
    {
      String uri = ActionContext.getContext().getApplication().get("FederationServiceUri").toString();
      httpClient = new HttpClient(uri);
    }
    catch (Exception e)
    {
      System.out.println("Exception in FederationServiceUri :" + e);
    }
    federation = null;
  }

  public void populate()
  {
    try
    {
      federation = httpClient.get(Federation.class, "/federation");
    }
    catch (Exception e)
    {
      System.out.println("Exception in populate");
      e.printStackTrace();
    }
  }

  public void save()
  {
    try
    {
      Response response = httpClient.post(Response.class, "/federation", federation);
      System.out.println("response.getLevel().value() :" + response.getLevel().value());
    }
    catch (Exception e)
    {
      System.out.println("Exception :" + e);
    }
  }

  public IDGenerators getIdGenerators()
  {
    return federation.getIdGenerators();
  }

  public Namespaces getNamespaces()
  {
    return federation.getNamespaces();
  }

  public Repositories getRepositories()
  {
    return federation.getRepositories();
  }

  public Federation getFederation()
  {
    return federation;
  }

  public Tree toTree()
  {
    Tree tree = new Tree();
    List<Node> treeNodes = tree.getNodes();

    TreeNode generatorsNode = new TreeNode();
    generatorsNode.setIdentifier("idGenerator");
    generatorsNode.setText("ID Generators");
    generatorsNode.setIconCls("folder");
    generatorsNode.getProperties().put("Id", "idGenerator");
    
    // For New Form
    generatorsNode.getProperties().put("Name", "");
    generatorsNode.getProperties().put("URI", "");
    generatorsNode.getProperties().put("Description", "");

    treeNodes.add(generatorsNode);
    List<Node> generatorNodes = generatorsNode.getChildren();

    // Default Node
    LeafNode generatorNodeDef = new LeafNode();
    generatorNodeDef.setIdentifier("idGenerator0");
    generatorNodeDef.setText("None");
    generatorNodeDef.setIconCls("generator");
    generatorNodeDef.getProperties().put("Id", "idGenerator0");    
    generatorNodeDef.setLeaf(true);
    generatorNodeDef.setHidden(true);
    generatorNodes.add(generatorNodeDef);

    for (IDGenerator idgenerator : federation.getIdGenerators().getItems())
    {
      LeafNode generatorNode = new LeafNode();
      generatorNode.setIdentifier(idgenerator.getId());
      generatorNode.setText(idgenerator.getName());
      generatorNode.setIconCls("generator");
      generatorNode.setLeaf(true);

      Map<String, String> properties = generatorNode.getProperties();
      properties.put("Id", idgenerator.getId());
      properties.put("Name", idgenerator.getName());
      properties.put("URI", idgenerator.getUri());
      properties.put("Description", idgenerator.getDescription());

      generatorNodes.add(generatorNode);
    }

    // Namespaces
    TreeNode namespacesNode = new TreeNode();
    namespacesNode.setIdentifier("namespace");
    namespacesNode.setText("Namespaces");
    namespacesNode.setIconCls("folder");
    namespacesNode.getProperties().put("Id", "namespace");
    namespacesNode.setIdentifier("namespace");
    
    // For New Form
    namespacesNode.getProperties().put("Alias","");
    namespacesNode.getProperties().put("URI","");
    namespacesNode.getProperties().put("Description","");
    namespacesNode.getProperties().put("Writable","");
    namespacesNode.getProperties().put("ID Generator","");
    
    treeNodes.add(namespacesNode);
    List<Node> namespaceNodes = namespacesNode.getChildren();

    for (Namespace namespace : federation.getNamespaces().getItems())
    {
      LeafNode namespaceNode = new LeafNode();
      namespaceNode.setIdentifier(namespace.getId());
      namespaceNode.setText(namespace.getAlias());
      namespaceNode.setIconCls("namespace");
      namespaceNode.setLeaf(true);

      Map<String, String> properties = namespaceNode.getProperties();
      properties.put("Id", namespace.getId());
      properties.put("Alias", namespace.getAlias());
      properties.put("URI", namespace.getUri());
      properties.put("Description", namespace.getDescription());
      properties.put("Writable", String.valueOf(namespace.isIsWritable()));
      
      if (namespace.getIdGenerator() != null)
      {
        properties.put("ID Generator", "idgenerator" + namespace.getIdGenerator());
      }

      namespaceNodes.add(namespaceNode);
    }

    // Repositories
    TreeNode repositoriesNode = new TreeNode();
    repositoriesNode.setIdentifier("repository");
    repositoriesNode.setText("Repositories");
    repositoriesNode.setIconCls("folder");
    repositoriesNode.getProperties().put("Id", "repository");
    repositoriesNode.setIdentifier("repository");

    // For New Form
    repositoriesNode.getProperties().put("Name", "");
    repositoriesNode.getProperties().put("Description", "");    
    repositoriesNode.getProperties().put("Repository Type", "");
    repositoriesNode.getProperties().put("Namespace List", "");
    repositoriesNode.getProperties().put("Read Only", "");
    repositoriesNode.getProperties().put("URI", "");
    repositoriesNode.getProperties().put("Update URI", "");    
    
    treeNodes.add(repositoriesNode);
    List<Node> repositoryNodes = repositoriesNode.getChildren();

    for (Repository repository : federation.getRepositories().getItems())
    {
      LeafNode repositoryNode = new LeafNode();
      repositoryNode.setIdentifier(repository.getId());
      repositoryNode.setText(repository.getName());
      repositoryNode.setIconCls("repository");
      repositoryNode.setLeaf(true);

      Map<String, String> properties = repositoryNode.getProperties();

      
      properties.put("Name", repository.getName());
      properties.put("Description", repository.getDescription());
      properties.put("Repository Type", repository.getRepositoryType().value());
      properties.put("Id", repository.getId());
      properties.put("Read Only", String.valueOf(repository.isIsReadOnly()));
      properties.put("URI", repository.getUri());
      properties.put("Update URI", repository.getUpdateUri());
      
      
      if (repository.getNamespaces() != null)
      {
        Map<String, String> namespaces = new HashMap<String, String>();
        
        for (String namespaceId : repository.getNamespaces().getItems())
        {
          Namespace namespace = getNamespace(namespaceId);
          namespaces.put(namespaceId, namespace.getAlias());
        }       
        
        try
        {
          String nameList = JSONUtil.serialize(namespaces);
          properties.put("Namespace List", nameList);
        }
        catch (JSONException ex)
        {
          properties.put ("Namespace List", null);
        }
      }
      else
      {
        properties.put("Namespace List", null);
      }
      
      repositoryNodes.add(repositoryNode);
    }

    return tree;
  }
  
  private Namespace getNamespace(String namespaceId)
  {
    for (Namespace namespace : federation.getNamespaces().getItems())
    {
      if (namespace.getId().equalsIgnoreCase(namespaceId))
      {
        return namespace;
      }
    }
    
    return null;
  }

  public boolean readTree(HttpServletRequest httpRequest)
  {
    try
    {
      Response response = null;

      System.out.println("###" + httpRequest.getParameter("parentNodeID") + "### ---"
          + httpRequest.getParameter("nodeID"));
      if ("idGenerator".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        IDGenerator idgenerator = new IDGenerator();
        if (httpRequest.getParameter("nodeID") != null)
          idgenerator.setId(httpRequest.getParameter("nodeID").replaceFirst("idgenerator", ""));
        idgenerator.setName(httpRequest.getParameter("Name"));
        idgenerator.setUri(httpRequest.getParameter("URI"));
        idgenerator.setDescription(httpRequest.getParameter("Description"));

        response = httpClient.post(Response.class, "/idgenerator", idgenerator);

      }
      else if ("namespace".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        Namespace namespace = new Namespace();
        if (httpRequest.getParameter("nodeID") != null)
          namespace.setId(httpRequest.getParameter("nodeID").replaceFirst("namespace", ""));
        namespace.setUri(httpRequest.getParameter("URI"));
        namespace.setAlias(httpRequest.getParameter("Alias"));
        namespace.setIsWritable(Boolean.parseBoolean(httpRequest.getParameter("Writable")));
        namespace.setDescription(httpRequest.getParameter("Description"));
        namespace.setIdGenerator(httpRequest.getParameter("ID Generator").replaceFirst("idgenerator", ""));

        response = httpClient.post(Response.class, "/namespace", namespace);
      }
      else if ("repository".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        Repository repository = new Repository();
        if (httpRequest.getParameter("nodeID") != null)
          repository.setId(httpRequest.getParameter("nodeID").replaceFirst("repository", ""));
        System.out.println("Description :" + httpRequest.getParameter("Description"));
        repository.setName(httpRequest.getParameter("Name"));
        repository.setDescription(httpRequest.getParameter("Description"));
        repository.setRepositoryType(RepositoryType.fromValue(httpRequest.getParameter("Repository Type")));
        repository.setIsReadOnly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
        repository.setUri(httpRequest.getParameter("URI"));        
        repository.setUpdateUri(httpRequest.getParameter("Update URI"));
        if (httpRequest.getParameter("Namespace List") != null)
        {
          StringTokenizer st = new StringTokenizer(httpRequest.getParameter("Namespace List"), ",");
          NamespaceList namespaceList = new NamespaceList();
          List<String> namespaceItem = namespaceList.getItems();
          while (st.hasMoreTokens())
          {
            namespaceItem.add(st.nextToken().replaceFirst("namespace", ""));
          }
          repository.setNamespaces(namespaceList);
          System.out.println("Namespace List :" + namespaceList.getItems());
        }
        response = httpClient.post(Response.class, "/repository", repository);
      }
      if (response != null)
      {
        System.out.println("response.getLevel().value() :" + response.getLevel().value());
        if ("success".equalsIgnoreCase(response.getLevel().value()))
        {
          return true;
        }
        else
          return false;
      }
      else
      {
        System.out.println("response.getLevel().value() : null");
        return false;
      }
    }
    catch (Exception e)
    {
      System.out.println("Error Occured");
      e.printStackTrace();
      return false;
    }
  }

  public boolean deleteNode(String nodeId, String parentNodeID)
  {
    try
    {
      Response response = null;
      if ("idGenerator".equalsIgnoreCase(parentNodeID))
      {
        IDGenerator idgenerator = new IDGenerator();
        idgenerator.setId(nodeId.replaceFirst("idgenerator", ""));

        response = httpClient.post(Response.class, "/idgenerator/delete", idgenerator);

      }
      else if ("namespace".equalsIgnoreCase(parentNodeID))
      {
        Namespace namespace = new Namespace();
        namespace.setId(nodeId.replaceFirst("namespace", ""));

        response = httpClient.post(Response.class, "/namespace/delete", namespace);

      }
      else if ("repository".equalsIgnoreCase(parentNodeID))
      {
        Repository repository = new Repository();
        repository.setId(nodeId.replaceFirst("repository", ""));

        response = httpClient.post(Response.class, "/repository/delete", repository);

      }

      if (response != null)
      {
        System.out.println("response.getLevel().value() :" + response.getLevel().value());
        if ("success".equalsIgnoreCase(response.getLevel().value()))
        {
          return true;
        }
        else
          return false;
      }
      else
      {
        System.out.println("response.getLevel().value() : null");
        return false;
      }
    }
    catch (Exception e)
    {
      e.printStackTrace();
      return false;
    }
  }
}
