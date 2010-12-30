package org.iringtools.models;

import java.util.Enumeration;
import java.util.List;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.WidgetUtil;

import com.opensymphony.xwork2.ActionContext;

public class FederationModel
{
  private Federation federation = null;
  private HttpClient httpClient = null;

  public FederationModel()
  {
    try
    {
      String uri = ActionContext.getContext().getApplication().get("RefDataServiceUri").toString();
      httpClient = new HttpClient(uri);
    }
    catch (Exception e)
    {
      System.out.println("Exception in RefDataServiceUri :" + e);
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
    generatorsNode.setText("ID Generators");
    generatorsNode.setId("idGenerator");
    generatorsNode.setIconCls("folder");
    List<Property> idNodeProperties = generatorsNode.getProperties();
    idNodeProperties.add(WidgetUtil.createProperty("Name", ""));
    idNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    idNodeProperties.add(WidgetUtil.createProperty("Description", ""));

    treeNodes.add(generatorsNode);

    List<Node> generatorNodes = generatorsNode.getChildren();

    for (IDGenerator idgenerator : federation.getIdGenerators().getItems())
    {
      LeafNode generatorNode = new LeafNode();
      generatorNode.setId("idgenerator" + idgenerator.getId());
      generatorNode.setText(idgenerator.getName());
      generatorNode.setIconCls("generator");
      generatorNode.setLeaf(true);

      List<Property> properties = generatorNode.getProperties();
      properties.add(WidgetUtil.createProperty("Name", idgenerator.getName()));
      properties.add(WidgetUtil.createProperty("URI", idgenerator.getUri()));
      properties.add(WidgetUtil.createProperty("Description", idgenerator.getDescription()));

      generatorNodes.add(generatorNode);
    }

    // Namespaces
    TreeNode namespacesNode = new TreeNode();
    namespacesNode.setText("Namespaces");
    namespacesNode.setId("namespace");
    namespacesNode.setIconCls("folder");
    List<Property> nsNodeProperties = namespacesNode.getProperties();
    nsNodeProperties.add(WidgetUtil.createProperty("Alias", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("Description", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("Writable", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("ID Generator", ""));

    treeNodes.add(namespacesNode);

    List<Node> namespaceNodes = namespacesNode.getChildren();

    for (Namespace namespace : federation.getNamespaces().getItems())
    {
      LeafNode namespaceNode = new LeafNode();
      namespaceNode.setId("namespace" + namespace.getId());
      namespaceNode.setText(namespace.getAlias());
      namespaceNode.setIconCls("namespace");
      namespaceNode.setLeaf(true);

      List<Property> properties = namespaceNode.getProperties();
      properties.add(WidgetUtil.createProperty("Alias", namespace.getAlias()));
      properties.add(WidgetUtil.createProperty("URI", namespace.getUri()));
      properties.add(WidgetUtil.createProperty("Description", namespace.getDescription()));
      properties.add(WidgetUtil.createProperty("Writable", String.valueOf(namespace.isIsWritable())));
      properties.add(WidgetUtil.createProperty("ID Generator", String.valueOf(namespace.getIdGenerator())));

      namespaceNodes.add(namespaceNode);
    }

    // Repositories

    TreeNode repositoriesNode = new TreeNode();
    repositoriesNode.setText("Repositories");
    repositoriesNode.setId("repository");
    repositoriesNode.setIconCls("folder");
    List<Property> repoNodeProperties = repositoriesNode.getProperties();
    repoNodeProperties.add(WidgetUtil.createProperty("Name", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Description", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Read Only", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Repository Type", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Update URI", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Namespace List", ""));

    treeNodes.add(repositoriesNode);

    List<Node> repositoryNodes = repositoriesNode.getChildren();

    for (Repository repository : federation.getRepositories().getItems())
    {
      LeafNode repositoryNode = new LeafNode();
      repositoryNode.setId("repository" + repository.getId());
      repositoryNode.setText(repository.getName());
      repositoryNode.setIconCls("repository");
      repositoryNode.setLeaf(true);

      List<Property> properties = repositoryNode.getProperties();

      properties.add(WidgetUtil.createProperty("Name", repository.getName()));
      properties.add(WidgetUtil.createProperty("Description", repository.getDescription()));
      properties.add(WidgetUtil.createProperty("Read Only", String.valueOf(repository.isIsReadOnly())));
      properties.add(WidgetUtil.createProperty("Repository Type", repository.getRepositoryType().value()));
      properties.add(WidgetUtil.createProperty("Update URI", repository.getUpdateUri()));
      properties.add(WidgetUtil.createProperty("URI", repository.getUri()));
      if(repository.getNamespaces()!=null){
	      properties.add(WidgetUtil.createProperty("Namespace List", repository.getNamespaces()));
	    		  //WidgetUtil.createNameSpaceList(federation.getNamespaces(),repository.getNamespaces())));
      }else{
    	  properties.add(WidgetUtil.createProperty("Namespace List", null));
      }
      repositoryNodes.add(repositoryNode);
    }

    return tree;

  }

  public boolean readTree(HttpServletRequest httpRequest)
  {
		try{
				
				Response response=null;

				System.out.println("###"+httpRequest.getParameter("parentNodeID")+"### ---"+httpRequest.getParameter("nodeID"));
				if("idGenerator".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					IDGenerator idgenerator = new IDGenerator();
					idgenerator.setId(httpRequest.getParameter("nodeID").replaceFirst("idgenerator", ""));
					idgenerator.setName(httpRequest.getParameter("Name"));
					idgenerator.setUri(httpRequest.getParameter("URI"));
					idgenerator.setDescription(httpRequest.getParameter("Description"));
					
					response = httpClient.post(Response.class, "/idgenerator", idgenerator);
					
				}else if("namespace".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Namespace namespace = new Namespace();
					namespace.setId(httpRequest.getParameter("nodeID").replaceFirst("namespace", ""));
					namespace.setUri(httpRequest.getParameter("URI"));
					namespace.setAlias(httpRequest.getParameter("Alias"));
					namespace.setIsWritable(Boolean.parseBoolean(httpRequest.getParameter("Writable")));
					namespace.setDescription(httpRequest.getParameter("Description"));
					namespace.setIdGenerator(Integer.parseInt(httpRequest.getParameter("ID Generator")));
					
					response = httpClient.post(Response.class, "/namespace", namespace);
					
				}else if("repository".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Repository repository = new Repository();
					repository.setId(httpRequest.getParameter("nodeID").replaceFirst("repository", ""));
					System.out.println("Description :"+httpRequest.getParameter("Description"));
					repository.setDescription(httpRequest.getParameter("Description"));
					repository.setUri(httpRequest.getParameter("URI"));
					repository.setName(httpRequest.getParameter("Name"));
					repository.setRepositoryType(RepositoryType.fromValue(httpRequest.getParameter("Repository Type")));
					repository.setUpdateUri(httpRequest.getParameter("Update URI"));
					repository.setIsReadOnly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
					System.out.println("Namespace List :"+httpRequest.getParameter("Namespace List"));
					response = httpClient.post(Response.class, "/repository", repository);

				}
				if(response!=null){
					System.out.println("response.getLevel().value() :" + response.getLevel().value());
					if("success".equalsIgnoreCase(response.getLevel().value())){
						return true;
					}else
						return false;
				}else{
					System.out.println("response.getLevel().value() : null");
					return false;
				}
		}catch(Exception e){
			System.out.println("Error Occured");
			e.printStackTrace();
			return false;
			
		}
   
  }
  
  public boolean deleteNode(String nodeId, String parentNodeID){
	  try{
		  Response response=null;
		  if("idGenerator".equalsIgnoreCase(parentNodeID)){
				IDGenerator idgenerator = new IDGenerator();
				idgenerator.setId(nodeId.replaceFirst("idgenerator", ""));
				
				response = httpClient.post(Response.class, "/idgenerator/delete", idgenerator);
				
			}else if("namespace".equalsIgnoreCase(parentNodeID)){
				Namespace namespace = new Namespace();
				namespace.setId(nodeId.replaceFirst("namespace", ""));
				
				response = httpClient.post(Response.class, "/namespace/delete", namespace);
				
			}else if("repository".equalsIgnoreCase(parentNodeID)){
				Repository repository = new Repository();
				repository.setId(nodeId.replaceFirst("repository", ""));
				
				response = httpClient.post(Response.class, "/repository/delete", repository);

			}
		  
		  if(response!=null){
				System.out.println("response.getLevel().value() :" + response.getLevel().value());
				if("success".equalsIgnoreCase(response.getLevel().value())){
					return true;
				}else
					return false;
			}else{
				System.out.println("response.getLevel().value() : null");
				return false;
			}
	  }catch(Exception e){
			e.printStackTrace();
			return false;
			
		}
  }

}
