package org.iringtools.models;

import java.lang.reflect.Method;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

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
import org.iringtools.utility.CommonUtility;
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
    List<Node> treeNodes = tree.getTreeNodes();
    //CommonUtility commUtil = new CommonUtility();
    
    TreeNode generatorsNode = new TreeNode();
    generatorsNode.setText("ID Generators");
    generatorsNode.setId("idGenerator");
    generatorsNode.setIconCls("folder");
    treeNodes.add(generatorsNode);

    List<Node> generatorNodes = generatorsNode.getChildren();

    for (IDGenerator idgenerator : federation.getIdGenerators().getItems())
    {
      LeafNode generatorNode = new LeafNode();
      generatorNode.setId(idgenerator.getName());
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
    treeNodes.add(namespacesNode);

    List<Node> namespaceNodes = namespacesNode.getChildren();

    for (Namespace namespace : federation.getNamespaces().getItems())
    {
      LeafNode namespaceNode = new LeafNode();
      namespaceNode.setId(namespace.getAlias());
      namespaceNode.setText(namespace.getAlias());
      namespaceNode.setIconCls("namespace");
      namespaceNode.setLeaf(true);

      List<Property> properties = namespaceNode.getProperties();
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
    treeNodes.add(repositoriesNode);

    List<Node> repositoryNodes = repositoriesNode.getChildren();

    for (Repository repository : federation.getRepositories().getItems())
    {
      LeafNode repositoryNode = new LeafNode();
      repositoryNode.setId(repository.getName());
      repositoryNode.setText(repository.getName());
      repositoryNode.setIconCls("repository");
      repositoryNode.setLeaf(true);

      List<Property> properties = repositoryNode.getProperties();
      /*Map map = commUtil.getRepoGetterMap();
	  Iterator iterator = map.keySet().iterator();
	  try{
	      while(iterator.hasNext()){   
	    	  String key = iterator.next().toString();
	          properties.add(WidgetUtil.createProperty(key, 
	        		  String.valueOf(commUtil.executeString(repository,(String) map.get(key)))));
	      	}
      }
      catch(Exception e){
    	  
      }*/
      properties.add(WidgetUtil.createProperty("URI", repository.getUri()));
      properties.add(WidgetUtil.createProperty("Description", repository.getDescription()));
      properties.add(WidgetUtil.createProperty("Read Only", String.valueOf(repository.isIsReadOnly())));
      properties.add(WidgetUtil.createProperty("Repository Type", repository.getRepositoryType()));
      properties.add(WidgetUtil.createProperty("Update URI", repository.getUpdateUri()));

      repositoryNodes.add(repositoryNode);
    }

    return tree;

  }

  public void readTree(HttpServletRequest httpRequest)
  {
		try{
				//String method = httpRequest.getMethod() ; 
				//System.out.println("Request reaching here :"+ method);
				//String ParameterNames = "";
				//IDGenerator idgenerator;
				//Namespace namespace;
				//Repository repository;
				
				Response response=null;
				/*for(Enumeration e = httpRequest.getParameterNames();e.hasMoreElements();){
					ParameterNames = (String)e.nextElement();
					System.out.println(ParameterNames+":"+httpRequest.getParameter("ParameterNames"));
				}*/
				System.out.println("###"+httpRequest.getParameter("parentNodeID")+"###");
				if("idGenerator".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					IDGenerator idgenerator = new IDGenerator();
					idgenerator.setName(httpRequest.getParameter("Name"));
					idgenerator.setUri(httpRequest.getParameter("URI"));
					idgenerator.setDescription(httpRequest.getParameter("Description"));
					
					response = httpClient.post(Response.class, "/idgenerator", idgenerator);
					
				}else if("namespace".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Namespace namespace = new Namespace();
					namespace.setUri(httpRequest.getParameter("URI"));
					namespace.setAlias(httpRequest.getParameter("Alias"));
					namespace.setIsWritable(Boolean.parseBoolean(httpRequest.getParameter("Writable")));
					namespace.setDescription(httpRequest.getParameter("Description"));
					
					response = httpClient.post(Response.class, "/namespace", namespace);
					
				}else if("repository".equalsIgnoreCase(httpRequest.getParameter("parentNodeID"))){
					Repository repository = new Repository();
					repository.setDescription(httpRequest.getParameter("Description"));
					repository.setUri(httpRequest.getParameter("URI"));
					repository.setRepositoryType(httpRequest.getParameter("Repository Type"));
					repository.setUpdateUri(httpRequest.getParameter("Update URI"));
					repository.setIsReadOnly(Boolean.parseBoolean(httpRequest.getParameter("Read Only")));
					
					response = httpClient.post(Response.class, "/repository", repository);

				}
				if(response!=null){
					System.out.println("response.getLevel().value() :" + response.getLevel().value());
				}else{
					System.out.println("response.getLevel().value() : null");
				}
		}catch(Exception e){
			e.printStackTrace();
			
		}
   
  }
  
  private String hello="rashmi";
/*  public String hello(){
	  System.out.println("hello");
	  return "elkdjdf";
  }
  
  public static void main(String args[]) throws Exception{
	  CommonUtility c = new CommonUtility();
	  Map map = c.getRepoGetterMap();
	  Iterator iterator = map.keySet().iterator();
      while(iterator.hasNext()){   
    	  String key = iterator.next().toString();
          System.out.println(key+":"+(String) map.get(key));
          properties.add(WidgetUtil.createProperty(key, (String) map.get(key)));
      }

	 }*/

public String getHello() {
	return hello;
}

public void setHello(String hello) {
	this.hello = hello;
}
}
