package org.iringtools.models;

import java.util.HashMap;
import java.util.List;


import javax.servlet.http.HttpServletRequest;

import org.ids_adi.ns.qxf.model.ClassDefinition;
import org.ids_adi.ns.qxf.model.Classification;
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
        	node.setRecord(entity);
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
	  HashMap<String, String> hsMap = new HashMap<String, String>();
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/classes/"+id);
		  
		  List<Node> treeNodes = tree.getNodes();
	      LeafNode node = new LeafNode();
	      ClassDefinition classDefinition = qmxf.getClassDefinitions().get(0);
	      node.setText(qmxf.getClassDefinitions().get(0).getNames().get(0).getValue());
	      node.setIdentifier(classDefinition.getId().
	    		  substring(classDefinition.getId().indexOf("#")+1,classDefinition.getId().length()));
	      hsMap.put("Identifier", classDefinition.getSpecializations().get(0).getReference());
	      hsMap.put("Repository", classDefinition.getRepository());
	      hsMap.put("Entity Type", classDefinition.getEntityType().getReference());
	      hsMap.put("Name", classDefinition.getNames().get(0).getValue());
	      hsMap.put("Status Authority", classDefinition.getStatuses().get(0).getAuthority());
	      hsMap.put("Status Class", classDefinition.getStatuses().get(0).getClazz());
	      hsMap.put("Status From", classDefinition.getStatuses().get(0).getFrom());
	      hsMap.put("Status To", classDefinition.getStatuses().get(0).getTo());
	      hsMap.put("Description", classDefinition.getDescriptions().get(0).getValue());
	      hsMap.put("URI", classDefinition.getSpecializations().get(0).getReference());
	      
	      node.setRecord(hsMap);
	      node.setIconCls("class");
  		  node.setType(Type.CLASS.value());
  		  node.setHidden(true);

    	  treeNodes.add(node);
    	  
    	  for(Classification classification : classDefinition.getClassifications()){
    		  TreeNode treeNode = new TreeNode();
    		  List<Node> childrenNodes = treeNode.getChildren();
        	  childrenNodes = getDefaultChildren(childrenNodes);
        	  treeNode.setIconCls("class");
        	  treeNode.setText(classification.getLabel());
        	  treeNode.setType(Type.CLASS.value());
        	  treeNode.setIdentifier(classification.getReference().
    	    		  substring(classification.getReference().indexOf("#")+1,classification.getReference().length()));
        	  treeNodes.add(treeNode);
    		  
    	  }
	      
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
	  		  node.setType(Type.CLASS.value());
	  		  List<Node> childrenNodes = node.getChildren();
	    	  childrenNodes = getDefaultChildren(childrenNodes);
	    	  treeNodes.add(node);
	       }
	      
	  
		  
	  }catch(Exception e){
		  
	  }
	  return tree;
	  
	  
  }
  public Tree getTemplates(String id){
	  Tree tree = new Tree();
	  System.out.println("Inside getTemplates");
	  try{
		  response = httpClient.get(Response.class, "/classes/"+id+"/templates");
		  List<Node> treeNodes = tree.getNodes();
	      LeafNode node;
	      for (Entity entity : response.getEntities().getItems())
	       {
	    	  node = new LeafNode();
		      node.setText(entity.getLabel());
		      node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
		      node.setRecord(entity);
		      node.setIconCls("template");
	  		  node.setType(Type.TEMPLATENODE.value());
	    	  treeNodes.add(node);
	       }
	  }catch(Exception e){
		  
	  }
	  return tree;
	  
	  
}
  public Qmxf getRole(String templateId){
	  Qmxf qmxf = null;
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/templates/"+templateId);
			  //R85736598359
	  }catch(Exception e){
		  
	  }
	  return qmxf;
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
