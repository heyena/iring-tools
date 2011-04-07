package org.iringtools.models;

import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpServletRequest;

import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.response.Entities;
import org.iringtools.refdata.response.Entity;
import org.iringtools.refdata.response.Response;
import org.iringtools.utility.HttpClient;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

import com.opensymphony.xwork2.ActionContext;

public class RefDataModel
{
  private Response response = null;
  //private Entity responseEntity =null;
  
  private HttpClient httpClient = null;

  public RefDataModel()
  {
    try
    {
      String uri = ActionContext.getContext().getApplication().get("RefDataServiceUri").toString();
      httpClient = new HttpClient(uri);
    }
    catch (Exception e)
    {
      System.out.println("Exception in FederationServiceUri :" + e);
    }
    response = null;
    //responseEntity=null;
  }

  public Tree populate(HttpServletRequest httpRequest)
  {
	  Tree tree = new Tree();
	  try
	  {	
    	String query=httpRequest.getParameter("query");
    	String start=httpRequest.getParameter("start");
    	String limit=httpRequest.getParameter("limit");
    	System.out.print("%%%%%%%%%"+query+"====="+start+"====="+limit);
    	
    	response = httpClient.get(Response.class, "/search/"+query+"/"+start+"/"+limit);
        
        List<Node> treeNodes = tree.getNodes();
        LeafNode node = null;

    	for (Entity entity : response.getEntities().getItems())
        {
    		node = new LeafNode();
        	node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
        	node.setText(entity.getLabel()+" ("+entity.getRepository()+")");
        	if(entity.getUri().contains("rdl.rdlfacade.org")){
        		node.setIconCls("class");
        	}else{
        		node.setIconCls("template");
        	}
        	node.getProperties().put("lang", entity.getLang());
        	node.getProperties().put("uri", entity.getUri());

        	treeNodes.add(node);
        }
    }
    catch (Exception e)
    {
      System.out.println("Exception in populate");
      e.printStackTrace();
    }
    return tree;
  }
  
  public Qmxf getClass(String id){
	  
	  Qmxf qmxf = null;
	  
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/classes/"+id);
	  }catch(Exception e){
		  
	  }
	  return qmxf;
	  
	  
  }
  public Response getSuperClasses(String id){
	  
	  
	  try{
		  response = httpClient.get(Response.class, "/classes/"+id+"/superclasses");
	  }catch(Exception e){
		  
	  }
	  return response;
	  
	  
  }
  public Response getSubClasses(String id){
	  
	  
	  try{
		  response = httpClient.get(Response.class, "/classes/"+id+"/subclasses");
	  }catch(Exception e){
		  
	  }
	  return response;
	  
	  
}
  public Response getTemplates(String id){
	  
  
	  try{
		  response = httpClient.get(Response.class, "/classes/"+id+"/templates");
	  }catch(Exception e){
		  
	  }
	  return response;
	  
	  
}

/*  public Tree toTree()
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
    //generatorNodes.add(generatorNodeDef);

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
    repositoriesNode.getProperties().put("Read Only", "");
    repositoriesNode.getProperties().put("Repository Type", "");
    repositoriesNode.getProperties().put("Update URI", "");
    repositoriesNode.getProperties().put("URI", "");
    repositoriesNode.getProperties().put("Namespace List", "");
    
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

      properties.put("Id", repository.getId());
      properties.put("Name", repository.getName());
      properties.put("Description", repository.getDescription());
      properties.put("Read Only", String.valueOf(repository.isIsReadOnly()));
      properties.put("Repository Type", repository.getRepositoryType().value());
      properties.put("Update URI", repository.getUpdateUri());
      properties.put("URI", repository.getUri());
      
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
        repository.setDescription(httpRequest.getParameter("Description"));
        repository.setUri(httpRequest.getParameter("URI"));
        repository.setName(httpRequest.getParameter("Name"));
        repository.setRepositoryType(RepositoryType.fromValue(httpRequest.getParameter("Repository Type")));
        repository.setUpdateUri(httpRequest.getParameter("Update URI"));
        repository.setIsReadOnly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
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
  }*/

}
