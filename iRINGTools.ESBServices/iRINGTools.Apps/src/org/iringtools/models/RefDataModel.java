package org.iringtools.models;

import java.util.HashMap;
import java.util.List;

import javax.servlet.http.HttpServletRequest;

import org.ids_adi.ns.qxf.model.ClassDefinition;
import org.ids_adi.ns.qxf.model.Classification;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.ids_adi.ns.qxf.model.RoleQualification;
import org.ids_adi.ns.qxf.model.Specialization;
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
        

    	for (Entity entity : response.getEntities().getItems())
        {
        	
        		LeafNode node = new LeafNode();
	        	node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
	        	node.setText(entity.getLabel()+" ("+entity.getRepository()+")");
	        	
	        	if(entity.getUri().contains("rdl.rdlfacade.org")){
	        		node.setIconCls("class");
	        		node.setType(Type.CLASS.value());
	        	}else{
	        		node.setIconCls("template");
	        		node.setType(Type.TEMPLATENODE.value());
	        		node.setRecord(entity);

	        	}
            	//***node.setRecord(entity);
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
		  ClassDefinition classDefinition = qmxf.getClassDefinitions().get(0);
		  //First hidden property node
	      LeafNode node = new LeafNode();
	      
	      node.setText(qmxf.getClassDefinitions().get(0).getNames().get(0).getValue());
	      node.setIdentifier(classDefinition.getId().
	    		  substring(classDefinition.getId().indexOf("#")+1,classDefinition.getId().length()));
	      hsMap.put("Identifier", classDefinition.getId());
	      hsMap.put("URI", classDefinition.getId());
	      hsMap.put("Repository", classDefinition.getRepository());
	      hsMap.put("Entity Type", classDefinition.getEntityType().getReference());
	      hsMap.put("Name", classDefinition.getNames().get(0).getValue());
	      hsMap.put("Status Authority", classDefinition.getStatuses().get(0).getAuthority());
	      hsMap.put("Status Class", classDefinition.getStatuses().get(0).getClazz());
	      hsMap.put("Status From", classDefinition.getStatuses().get(0).getFrom());
	      hsMap.put("Status To", classDefinition.getStatuses().get(0).getTo());
	      hsMap.put("Description", classDefinition.getDescriptions().get(0).getValue());
	      
	      
	      node.setRecord(hsMap);
	      node.setIconCls("class");
  		  node.setType(Type.CLASS.value());
  		  node.setHidden(true);
  		  treeNodes.add(node);
		  
		  
		  
		   //other child nodes -- Classification Node
		  	TreeNode childNode = new TreeNode();
		  	//**** childNode.setText("Classifications");
		  	childNode.setType(Type.CLASSIFICATION.value());
		  	List<Node> childClassNodes = childNode.getChildren();
		  	
		  	for(Classification classification : classDefinition.getClassifications()){
	    		  LeafNode classTreeNode = new LeafNode();
	    		  classTreeNode.setIconCls("class");
	    		  classTreeNode.setText(classification.getLabel());
	    		  classTreeNode.setType(Type.CLASS.value());
	    		  classTreeNode.setIdentifier(classification.getReference().
	    	    		  substring(classification.getReference().indexOf("#")+1,classification.getReference().length()));
	    		  childClassNodes.add(classTreeNode);
	    		  
	    	  }
			childNode.setText("Classifications ("+classDefinition.getClassifications().size()+")");
		  	treeNodes.add(childNode);
		  	
		  	
		  	
		  	// Super class Node
		  	childNode = new TreeNode();
		  	//*** childNode.setText("Superclasses");
		  	childNode.setType(Type.SUPERCLASS.value());
		  	List<Node> childSuperNodes = childNode.getChildren();
		  	
		  	for(Specialization specialization : classDefinition.getSpecializations()){
		  		LeafNode superTreeNode = new LeafNode();
	    		  superTreeNode.setIconCls("class");
	    		  superTreeNode.setText(specialization.getLabel());
	    		  superTreeNode.setType(Type.CLASS.value());
	    		  superTreeNode.setIdentifier(specialization.getReference().
	    	    		  substring(specialization.getReference().indexOf("#")+1,specialization.getReference().length()));
	    		  childSuperNodes.add(superTreeNode);
	    		  
	    	  }
		  	childNode.setText("Superclasses ("+classDefinition.getSpecializations().size()+")");
		  	treeNodes.add(childNode);
		  	
		  	//Sub class Node
		  	LeafNode subChildNode = new LeafNode();
		  	subChildNode.setText("Subclasses");
		  	subChildNode.setType(Type.SUBCLASS.value());
		  	treeNodes.add(subChildNode);
		  	
		  	//Template Node
		  	subChildNode = new LeafNode();
		  	subChildNode.setText("Templates");
		  	subChildNode.setType(Type.CLASSTEMPLATE.value());
		  	treeNodes.add(subChildNode);

    	  
    	  
	      
	  }catch(Exception e){
		  System.out.println("Exception in getClass");
	      e.printStackTrace();
	  }
	  return tree;
	  
	  
  }
  public Tree getSubSuperClasses(String id, String classType){
	  Tree tree = new Tree();
	 // System.out.println("Inside getSuperClasses");
	  try{
		  String url = classType.equalsIgnoreCase("Sub")?"/classes/"+id+"/subclasses":"/classes/"+id+"/superclasses";
		  response = httpClient.get(Response.class,url);
		  
		  List<Node> treeNodes = tree.getNodes();
		  LeafNode node;
	      for (Entity entity : response.getEntities().getItems())
	       {
	    	  node = new LeafNode();
		      node.setText(entity.getLabel());
		      node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
		     //*** node.setRecord(entity);
		      node.setIconCls("class");
	  		  node.setType(Type.CLASS.value());
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
  
  public Tree getRole(String templateId){
	  System.out.println("Inside getRoles & the TemplateId: "+templateId);
	  Qmxf qmxf = null;
	  Tree tree = new Tree();
	  LeafNode node;
	  List<Node> treeNodes = tree.getNodes();
	  
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/templates/"+templateId);
			  //R85736598359
		  HashMap<String, String> hsMap;// = new HashMap<String, String>();
		  
		  for(RoleQualification roleQualifications:qmxf.getTemplateQualifications().get(0).getRoleQualifications()){
			  hsMap = new HashMap<String, String>();
			  
			  node = new LeafNode();
		      node.setIdentifier(roleQualifications.getId());
		      node.setIconCls("role");
		      hsMap.put("Identifier",roleQualifications.getQualifies());
		      hsMap.put("Name",roleQualifications.getNames().get(0).getValue());
		      //hsMap.put("URI", roleQualifications.);
		      node.setRecord(hsMap);
	  		  node.setType(Type.ROLENODE.value());
	  		  node.setText(roleQualifications.getNames().get(0).getValue());
	  			
	  		  
	  		try{
	  		 	if(!roleQualifications.getRange().isEmpty()){
		  			if(roleQualifications.getRange().contains("rdl.rdlfacade.org")){
		        	node.setLeaf(true);
		        	/*String classId= roleQualifications.getRange().substring(roleQualifications.getRange().indexOf("#")+1,roleQualifications.getRange().length());
		  			Tree roleClassNode = getClass(classId);*/
		  			}else{
		        		node.setLeaf(false);
		        	}
	  		 	}	  
	  		 }catch(Exception e){}
	    	treeNodes.add(node);  
		  }
		  
	  }catch(Exception e){
		  
	  }
	  return tree;
  }
}
