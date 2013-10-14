package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.StringTokenizer;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IdGenerator;
import org.iringtools.refdata.federation.IdGeneratorList;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.NamespaceList;
import org.iringtools.refdata.federation.RepositoryList;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.apache.struts2.json.JSONException;
import org.apache.struts2.json.JSONUtil;
import java.util.ArrayList;

import com.opensymphony.xwork2.ActionContext;

public class FederationModel
{
  private Map<String, Object> session = null;
  private Federation federation = null;
  private HttpClient httpClient = null;

  public FederationModel(Map<String, Object> session)
  {
    this.session = session;
    
    try
    {
      String uri = ActionContext.getContext().getApplication().get("RefDataServiceUri").toString();
      httpClient = new HttpClient(uri);
      HttpUtils.addHttpHeaders(this.session, httpClient);
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

  public IdGeneratorList getIdgeneratorlist()
  {
    return federation.getIdgeneratorlist();
  }

  public NamespaceList getNamespacelist()
  {
    return federation.getNamespacelist();
  }

  public RepositoryList getRepositorylist()
  {
    return federation.getRepositorylist();
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
    generatorNodeDef.getProperties().put("Id", "0");    
    generatorNodeDef.setLeaf(true);
    generatorNodeDef.setHidden(true);
    generatorNodes.add(generatorNodeDef);

    for (IdGenerator idgenerator : federation.getIdgeneratorlist().getItems())
    {
      LeafNode generatorNode = new LeafNode();
      generatorNode.setIdentifier(Integer.toString(idgenerator.getId()));
      generatorNode.setText(idgenerator.getName());
      generatorNode.setIconCls("generator");
      generatorNode.setLeaf(true);

      Map<String, String> properties = generatorNode.getProperties();
      properties.put("Id", Integer.toString(idgenerator.getId()));
      properties.put("Name", idgenerator.getName());
      properties.put("URI", idgenerator.getUri());
      properties.put("Description", idgenerator.getDescription());

      generatorNodes.add(generatorNode);
    }

    // Namespacelist
    TreeNode namespacesNode = new TreeNode();
    namespacesNode.setIdentifier("namespace");
    namespacesNode.setText("Namespacelist");
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

    for (Namespace namespace : federation.getNamespacelist().getItems())
    {
      LeafNode namespaceNode = new LeafNode();
      namespaceNode.setIdentifier(Integer.toString(namespace.getId()));
      namespaceNode.setText(namespace.getPrefix());
      namespaceNode.setIconCls("namespace");
      namespaceNode.setLeaf(true);

      Map<String, String> properties = namespaceNode.getProperties();
      properties.put("Id", Integer.toString(namespace.getId()));
      properties.put("Alias", namespace.getPrefix());
      properties.put("URI", namespace.getUri());
      properties.put("Description", namespace.getDescription());
      properties.put("Writable", String.valueOf(namespace.getIswriteable()));
      
      if (namespace.getIdgenerator() > -1)
      {
        properties.put("ID Generator", Integer.toString(namespace.getIdgenerator()));
      }

      namespaceNodes.add(namespaceNode);
    }

    // Repositorylist
    TreeNode repositoriesNode = new TreeNode();
    repositoriesNode.setIdentifier("repository");
    repositoriesNode.setText("Repositorylist");
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

    for (Repository repository : federation.getRepositorylist().getItems())
    {
      LeafNode repositoryNode = new LeafNode();
      repositoryNode.setIdentifier(repository.getId());
      repositoryNode.setText(repository.getName());
      repositoryNode.setIconCls("repository");
      repositoryNode.setLeaf(true);

      Map<String, String> properties = repositoryNode.getProperties();

      
      properties.put("Name", repository.getName());
      properties.put("Description", repository.getDescription());
      properties.put("Repository Type", repository.getRepositorytype().value());
      properties.put("Id", repository.getId());
      properties.put("Read Only", String.valueOf(repository.getIsreadonly()));
      properties.put("URI", repository.getUri());
      properties.put("Update URI", repository.getUpdateUri());
      
      
      if (repository.getNamespaces() != null)
      {
        Map<String, String> namespaces = new HashMap<String, String>();
        
        for (Namespace namespace : repository.getNamespaces().getItems())
        {
          namespaces.put(Integer.toString(namespace.getId()), namespace.getPrefix());
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

  public boolean readTree(HttpServletRequest httpRequest)
  {
    try
    {
      Response response = null;
      
      String nameSpaceId = "";
      System.out.println("###" + httpRequest.getParameter("parentNodeID") + "### ---"
          + httpRequest.getParameter("nodeID"));
      if ("idGenerator".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        IdGenerator idgenerator = new IdGenerator();
        if (httpRequest.getParameter("nodeID") != null)
        {
        	String nodeIdParameter = httpRequest.getParameter("nodeID");
        	if (nodeIdParameter.contains("idgenerator"))
        		nodeIdParameter = nodeIdParameter.replaceFirst("idgenerator", "");
          idgenerator.setId(Integer.parseInt(nodeIdParameter));
        }
        idgenerator.setName(httpRequest.getParameter("Name"));
        idgenerator.setUri(httpRequest.getParameter("URI"));
        idgenerator.setDescription(httpRequest.getParameter("Description"));

        response = httpClient.post(Response.class, "/idgenerator", idgenerator);

      }
      else if ("namespace".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        Namespace namespace = new Namespace();
        if (httpRequest.getParameter("nodeID") != null)
        {
        	String nodeIdParameter = httpRequest.getParameter("nodeID");
        	if (nodeIdParameter.contains("namespace"))
        		nodeIdParameter = nodeIdParameter.replaceFirst("namespace", "");
          namespace.setId(Integer.parseInt(nodeIdParameter));
        }
        namespace.setUri(httpRequest.getParameter("URI"));
        namespace.setPrefix(httpRequest.getParameter("Prefix"));
        namespace.setIswriteable(Boolean.parseBoolean(httpRequest.getParameter("Writable")));
        namespace.setDescription(httpRequest.getParameter("Description"));
        
        String idGeneratorParameter="";
        try {
        	idGeneratorParameter = httpRequest.getParameter("ID Generator");
        	idGeneratorParameter = idGeneratorParameter.replaceFirst("idgenerator", "");          
        }
        catch (Exception e)
        {
        	idGeneratorParameter="0";
          e.printStackTrace();
        }
        namespace.setIdgenerator(Integer.parseInt(idGeneratorParameter));
        response = httpClient.post(Response.class, "/namespace", namespace);
      }
      else if ("repository".equalsIgnoreCase(httpRequest.getParameter("parentNodeID")))
      {
        Repository repository = new Repository();
        if (httpRequest.getParameter("nodeID") != null) 
        {
        	String nodeIdParameter = httpRequest.getParameter("nodeID");
        	if (nodeIdParameter.contains("repository"))
        		nodeIdParameter = nodeIdParameter.replaceFirst("repository", "");
          repository.setId(nodeIdParameter);
        }
        
        System.out.println("Description :" + httpRequest.getParameter("Description"));
        repository.setName(httpRequest.getParameter("Name"));
        repository.setDescription(httpRequest.getParameter("Description"));
        repository.setRepositorytype(RepositoryType.fromValue(httpRequest.getParameter("Repository Type")));
        repository.setIsreadonly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
        repository.setUri(httpRequest.getParameter("URI"));        
        repository.setUpdateUri(httpRequest.getParameter("Update URI"));
        if (httpRequest.getParameter("Namespace List") != null)
        {
          StringTokenizer st = new StringTokenizer(httpRequest.getParameter("Namespace List"), ",");
          NamespaceList namespaces = new NamespaceList();
          List<Namespace> namespaceList = new ArrayList<Namespace>();
          namespaces.setItems(namespaceList);
          
          while (st.hasMoreTokens())
          {
        	nameSpaceId = st.nextToken().replaceFirst("namespace", "");
            Namespace namespace = new Namespace();
        	namespace.setId(Integer.parseInt(nameSpaceId));
        	namespaceList.add(namespace);
          }
          repository.setNamespaces(namespaces);
          //System.out.println("Namespace List :" + namespaceList.getItems());
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
        IdGenerator idgenerator = new IdGenerator();
        idgenerator.setId(Integer.parseInt(nodeId.replaceFirst("idgenerator", "")));

        response = httpClient.post(Response.class, "/idgenerator/delete", idgenerator);

      }
      else if ("namespace".equalsIgnoreCase(parentNodeID))
      {
        Namespace namespace = new Namespace();
        namespace.setId(Integer.parseInt(nodeId.replaceFirst("namespace", "")));

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
