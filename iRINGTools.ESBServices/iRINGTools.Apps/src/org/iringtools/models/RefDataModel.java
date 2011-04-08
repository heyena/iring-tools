package org.iringtools.models;

import java.net.URLEncoder;
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
    	
    	//query = URLEncoder.encode(query, "UTF-8");
    	query = query.replaceAll(" ", "%20");
    	//System.out.print("%%%%%%%%%"+query+"====="+start+"====="+limit);
    	
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
        	}else{
        		node.setIconCls("template");
        	}
        	node.getProperties().put("lang", entity.getLang());
        	node.getProperties().put("uri", entity.getUri());
        	List<Node> childrenNodes = node.getChildren();
        	LeafNode childNode = new LeafNode();
        	childNode.setText("Classifications");
        	childrenNodes.add(childNode);
        	childNode = new LeafNode();
        	childNode.setText("Superclasses");
        	childrenNodes.add(childNode);
        	childNode = new LeafNode();
        	childNode.setText("Subclasses");
        	childrenNodes.add(childNode);
        	childNode = new LeafNode();
        	childNode.setText("Templates");
        	childrenNodes.add(childNode);
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
}
