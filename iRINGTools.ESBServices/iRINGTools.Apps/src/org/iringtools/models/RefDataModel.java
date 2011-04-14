package org.iringtools.models;

import java.util.List;

import javax.servlet.http.HttpServletRequest;

import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.refdata.response.Entity;
import org.iringtools.refdata.response.Response;
import org.iringtools.utility.HttpClient;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;
import org.iringtools.widgets.tree.Type;

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
    	
    	query = query.replaceAll(" ", "%20");
    	
    	response = httpClient.get(Response.class, "/search/"+query+"/"+start+"/"+limit);
        
        List<Node> treeNodes = tree.getNodes();
        TreeNode node = null;

    	for (Entity entity : response.getEntities().getItems())
        {
    		node = new TreeNode();
        	node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
        	node.setText(entity.getLabel()+" ("+entity.getRepository()+")");
        	if(entity.getUri().contains("rdl.rdlfacade.org")){
        		node.setIconCls("class");
        		node.setType(Type.CLASS.value());
        	}else{
        		node.setIconCls("template");
        		node.setType(Type.TEMPLATE.value());
        	}
        	node.getProperties().put("lang", entity.getLang());
        	node.getProperties().put("uri", entity.getUri());
        	List<Node> childrenNodes = node.getChildren();
        	childrenNodes = getDefaultChildren(childrenNodes);
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
  
  public Tree getClass(String id){
	  Qmxf qmxf = null;
	  Tree tree = new Tree();
	  System.out.println("Inside getClass");
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/classes/"+id);
		  
		  List<Node> treeNodes = tree.getNodes();
	      TreeNode node = new TreeNode();
	      node.setText(qmxf.getClassDefinitions().get(0).getNames().get(0).getValue());
	      node.setRecord(qmxf.getClassDefinitions().get(0));
	      node.setIconCls("class");
  		  node.setType(Type.CLASS.value());
  		  List<Node> childrenNodes = node.getChildren();
    	  childrenNodes = getDefaultChildren(childrenNodes);
    	  treeNodes.add(node);
	      
	  }catch(Exception e){
		  System.out.println("Exception in getClass");
	      e.printStackTrace();
	  }
	  return tree;
	  
	  
  }
  public Tree getSubSuperClasses(String id, String classType){
	  Tree tree = new Tree();
	  System.out.println("Inside getSuperClasses");
	  try{
		  String url = classType.equalsIgnoreCase("Sub")?"/classes/"+id+"/subclasses":"/classes/"+id+"/superclasses";
		  response = httpClient.get(Response.class,url);
		  
		  List<Node> treeNodes = tree.getNodes();
	      TreeNode node;
	      for (Entity entity : response.getEntities().getItems())
	       {
	    	  node = new TreeNode();
		      node.setText(entity.getLabel());
		      node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
		      node.setRecord(entity);
		      node.setIconCls("class");
	  		  node.setType(classType.equalsIgnoreCase("Sub")?Type.SUBCLASS.value():Type.SUPERCLASS.value());
	  		  List<Node> childrenNodes = node.getChildren();
	    	  childrenNodes = getDefaultChildren(childrenNodes);
	    	  treeNodes.add(node);
	       }
	      
	  
		  
	  }catch(Exception e){
		  
	  }
	  return tree;
	  
	  
  }
  public Response getTemplates(String id){
	  try{
		  response = httpClient.get(Response.class, "/classes/"+id+"/templates");
	  }catch(Exception e){
		  
	  }
	  return response;
	  
	  
}
  private List<Node> getDefaultChildren(List<Node> childrenNodes)
  {
	  
	  	LeafNode childNode = new LeafNode();
	  	childNode.setText("Classifications");
	  	childNode.setType(Type.CLASSIFICATION.value());
	  	childrenNodes.add(childNode);
	  	childNode = new LeafNode();
	  	childNode.setText("Superclasses");
	  	childNode.setType(Type.SUPERCLASS.value());
	  	childrenNodes.add(childNode);
	  	childNode = new LeafNode();
	  	childNode.setText("Subclasses");
	  	childNode.setType(Type.SUBCLASS.value());
	  	childrenNodes.add(childNode);
	  	childNode = new LeafNode();
	  	childNode.setText("Templates");
	  	childNode.setType(Type.CLASSTEMPLATE.value());
	  	childrenNodes.add(childNode);

    return childrenNodes;
  }
}
