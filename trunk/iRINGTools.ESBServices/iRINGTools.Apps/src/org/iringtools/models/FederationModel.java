package org.iringtools.models;

import java.util.List;

import javax.servlet.http.HttpServletRequest;
import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
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
    List<Node> treeNodes = tree.getItems();
    
    TreeNode generatorsNode = new TreeNode();
    generatorsNode.setText("ID Generators");
    generatorsNode.setId("idGenerator");
    generatorsNode.setIconCls("folder");
    List<Property> idNodeProperties = generatorsNode.getItems();
    idNodeProperties.add(WidgetUtil.createProperty("Name", ""));
    idNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    idNodeProperties.add(WidgetUtil.createProperty("Description", ""));

    treeNodes.add(generatorsNode);

    List<Node> generatorNodes = generatorsNode.getChildren();

    for (IDGenerator idgenerator : federation.getIdGenerators().getItems())
    {
      LeafNode generatorNode = new LeafNode();
      generatorNode.setId(idgenerator.getId());
      generatorNode.setText(idgenerator.getName());
      generatorNode.setIconCls("generator");
      generatorNode.setLeaf(true);

      List<Property> properties = generatorNode.getItems();
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
    List<Property> nsNodeProperties = namespacesNode.getItems();
    nsNodeProperties.add(WidgetUtil.createProperty("Alias", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("Description", ""));
    nsNodeProperties.add(WidgetUtil.createProperty("Writable", ""));

    treeNodes.add(namespacesNode);

    List<Node> namespaceNodes = namespacesNode.getChildren();

    for (Namespace namespace : federation.getNamespaces().getItems())
    {
      LeafNode namespaceNode = new LeafNode();
      namespaceNode.setId(namespace.getId());
      namespaceNode.setText(namespace.getAlias());
      namespaceNode.setIconCls("namespace");
      namespaceNode.setLeaf(true);

      List<Property> properties = namespaceNode.getItems();
      properties.add(WidgetUtil.createProperty("Alias", namespace.getAlias()));
      properties.add(WidgetUtil.createProperty("URI", namespace.getUri()));
      properties.add(WidgetUtil.createProperty("Description", namespace.getDescription()));
      properties.add(WidgetUtil.createProperty("Writable", String.valueOf(namespace.isIsWritable())));

      namespaceNodes.add(namespaceNode);
    }

    // Repositories

    TreeNode repositoriesNode = new TreeNode();
    repositoriesNode.setText("Repositories");
    repositoriesNode.setId("repository");
    repositoriesNode.setIconCls("folder");
    List<Property> repoNodeProperties = repositoriesNode.getItems();
    repoNodeProperties.add(WidgetUtil.createProperty("URI", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Name", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Description", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Read Only", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Repository Type", ""));
    repoNodeProperties.add(WidgetUtil.createProperty("Update URI", ""));

    treeNodes.add(repositoriesNode);

    List<Node> repositoryNodes = repositoriesNode.getChildren();

    for (Repository repository : federation.getRepositories().getItems())
    {
      LeafNode repositoryNode = new LeafNode();
      repositoryNode.setId(repository.getId());
      repositoryNode.setText(repository.getName());
      repositoryNode.setIconCls("repository");
      repositoryNode.setLeaf(true);

      List<Property> properties = repositoryNode.getItems();

      properties.add(WidgetUtil.createProperty("URI", repository.getUri()));
      properties.add(WidgetUtil.createProperty("Name", repository.getName()));
      properties.add(WidgetUtil.createProperty("Description", repository.getDescription()));
      properties.add(WidgetUtil.createProperty("Read Only", String.valueOf(repository.isIsReadOnly())));
      properties.add(WidgetUtil.createProperty("Repository Type", repository.getRepositoryType()));
      properties.add(WidgetUtil.createProperty("Update URI", repository.getUpdateUri()));

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
					idgenerator.setId(httpRequest.getParameter("nodeID"));
					idgenerator.setName(httpRequest.getParameter("Name"));
					idgenerator.setUri(httpRequest.getParameter("URI"));
					idgenerator.setDescription(httpRequest.getParameter("Description"));
					
					response = httpClient.post(Response.class, "/idgenerator", idgenerator);
					
				}else if("namespace".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Namespace namespace = new Namespace();
					namespace.setId(httpRequest.getParameter("nodeID"));
					namespace.setUri(httpRequest.getParameter("URI"));
					namespace.setAlias(httpRequest.getParameter("Alias"));
					namespace.setIsWritable(Boolean.parseBoolean(httpRequest.getParameter("Writable")));
					namespace.setDescription(httpRequest.getParameter("Description"));
					
					response = httpClient.post(Response.class, "/namespace", namespace);
					
				}else if("repository".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Repository repository = new Repository();
					repository.setId(httpRequest.getParameter("nodeID"));
					repository.setDescription(httpRequest.getParameter("Description"));
					repository.setUri(httpRequest.getParameter("URI"));
					repository.setName(httpRequest.getParameter("Name"));
					repository.setRepositoryType(httpRequest.getParameter("Repository Type"));
					repository.setUpdateUri(httpRequest.getParameter("Update URI"));
					repository.setIsReadOnly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
					
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
			e.printStackTrace();
			return false;
			
		}
   
  }
  
  /*public static void main(String args[]){
	  String uniqueID = UUID.randomUUID().toString();
	  System.out.println(uniqueID);
	  //System.out.println(get());
  }
  static long current= System.currentTimeMillis();
  static public synchronized long get(){
    return current++;
    }*/

}
