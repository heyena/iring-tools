package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.StringTokenizer;

import javax.servlet.http.HttpServletRequest;

import org.ids_adi.ns.qxf.model.ClassDefinition;
import org.ids_adi.ns.qxf.model.Classification;
import org.ids_adi.ns.qxf.model.Description;
import org.ids_adi.ns.qxf.model.EntityType;
import org.ids_adi.ns.qxf.model.Name;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.ids_adi.ns.qxf.model.RoleQualification;
import org.ids_adi.ns.qxf.model.Specialization;
import org.ids_adi.ns.qxf.model.TemplateDefinition;
import org.iringtools.refdata.response.Entity;
import org.iringtools.refdata.response.Response;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;
import org.iringtools.widgets.tree.Type;

import com.opensymphony.xwork2.ActionContext;

public class RefDataModel
{
  private Response response = null;
  private Map<String, Object> session = null;
  private HttpClient httpClient = null;
  private HttpClient idGenHttpClient = null;

  public RefDataModel(Map<String, Object> session)
  {
    this.session = session;
    
    try
    {
      String uri = ActionContext.getContext().getApplication().get("RefDataServiceUri").toString();
      httpClient = new HttpClient(uri);
      HttpUtils.addHttpHeaders(this.session, httpClient);
      uri = ActionContext.getContext().getApplication().get("IDGenServiceUri").toString();
      idGenHttpClient = new HttpClient(uri);
      HttpUtils.addHttpHeaders(this.session, idGenHttpClient);
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
	      hsMap.put("Lang", classDefinition.getNames().get(0).getLang());
	      
	      
	      node.setRecord(hsMap);
	      node.setIconCls("class");
  		  node.setType(Type.CLASS.value());
  		  node.setHidden(true);
  		  treeNodes.add(node);
		  
		  
		  
		   //other child nodes -- Classification Node
		  	TreeNode childNode = new TreeNode();
		  	//**** childNode.setText("Classifications");
		  	childNode.setType(Type.CLASSIFICATION.value());
		  	childNode.setIconCls("folder");
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
			childNode.setText("Member Of ("+classDefinition.getClassifications().size()+")");
		  	treeNodes.add(childNode);
		  	
		  	
		  	childNode = new TreeNode();
		  	//*** childNode.setText("Superclasses");
		  	childNode.setType(Type.MEMBERS.value());
		  	childNode.setIconCls("folder");
		  	List<Node> childMemberNodes = childNode.getChildren();
		  	childMemberNodes.addAll(getMembers(id).getNodes());
		  	childNode.setText("Members ("+childMemberNodes.size()+")");
		  	treeNodes.add(childNode);
		  	
		  	// Super class Node
		  	childNode = new TreeNode();
		  	//*** childNode.setText("Superclasses");
		  	childNode.setType(Type.SUPERCLASS.value());
		  	childNode.setIconCls("folder");
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
		  	childNode = new TreeNode();
		  	childNode.setType(Type.SUBCLASS.value());
		  	childNode.setIconCls("folder");
		  	List<Node> childSubNodes = childNode.getChildren();
		  	childSubNodes.addAll(getSubSuperClasses(id, "Sub").getNodes());
		  	childNode.setText("Subclasses ("+childSubNodes.size()+")");
		  	treeNodes.add(childNode);
		  	
		  	//Template Node
		  	childNode = new TreeNode();
		  	//childNode.setText("Templates");
		  	childNode.setType(Type.CLASSTEMPLATE.value());
		  	childNode.setIconCls("folder");
		  	childSubNodes = childNode.getChildren();
		  	childSubNodes.addAll(getTemplates(id).getNodes());
		  	childNode.setText("Templates ("+childSubNodes.size()+")");
		  	treeNodes.add(childNode);

    	  
    	  
	      
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
  
  public Tree getMembers(String id) {
	  Tree tree = new Tree();
	  
	  try {
		  response = httpClient.get(Response.class, "/classes/"+id+"/members");
		  List<Node> treeNodes = tree.getNodes();
		  LeafNode node;
		  for( Entity entity : response.getEntities().getItems()) {
			  node = new LeafNode();
			  node.setText(entity.getLabel());
			  node.setIdentifier(entity.getUri().substring(entity.getUri().indexOf("#")+1,entity.getUri().length()));
		      node.setRecord(entity);
		      node.setIconCls("class");
	  		  node.setType(Type.CLASS.value());
	  		  treeNodes.add(node);
		  }
	  }catch (Exception ex) {
		  
	  }
	  return tree;
  }
  
  public Tree getRole(String templateId){
	  Qmxf qmxf = null;
	  Tree tree = new Tree();
	  List<Node> treeNodes = tree.getNodes();
	  
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/templates/"+templateId);
		  HashMap<String, String> hsMap=new HashMap<String, String>();
		  
		  //First hidden property node
	      LeafNode baseNode = new LeafNode();
	      hsMap.put("Parent Template", qmxf.getTemplateQualifications().get(0).getQualifies());
	      hsMap.put("Authority", qmxf.getTemplateQualifications().get(0).getStatuses().get(0).getAuthority());
	      hsMap.put("Class", qmxf.getTemplateQualifications().get(0).getStatuses().get(0).getClazz());
	      
	      baseNode.setRecord(hsMap);		      
	      baseNode.setIconCls("template");
	      baseNode.setType(Type.TEMPLATENODE.value());
	      baseNode.setHidden(true);
  		  treeNodes.add(baseNode);
		  
		  
		  for(RoleQualification roleQualifications:qmxf.getTemplateQualifications().get(0).getRoleQualifications()){
			  hsMap = new HashMap<String, String>();
	  		  
	  		try{
		  		 	if(roleQualifications.getRange()!=null && roleQualifications.getRange().contains("rdl.rdlfacade.org")){
		  		 		TreeNode node = new TreeNode();
		  		        node.setIdentifier(roleQualifications.getId());
		  		        node.setIconCls("role");
		  		        hsMap.put("Identifier",roleQualifications.getQualifies());
		  		        hsMap.put("Name",roleQualifications.getNames().get(0).getValue());
		  		        node.setRecord(hsMap);
		  	  		    node.setType(Type.ROLENODE.value());
		  	  		    node.setText(roleQualifications.getNames().get(0).getValue());

		  	  		    //service call for class details
			        	String classId= roleQualifications.getRange().substring(roleQualifications.getRange().indexOf("#")+1,roleQualifications.getRange().length());
			        	LeafNode roleClassNode = new LeafNode();
			        	ClassDefinition classDefinition = getIndividualClassDtls(classId);
			        	roleClassNode.setText(classDefinition.getNames().get(0).getValue());
			        	roleClassNode.setIdentifier(classDefinition.getId().
			        			substring(classDefinition.getId().indexOf("#")+1,classDefinition.getId().length()));
			        	roleClassNode.setIconCls("class");
			        	roleClassNode.setType(Type.CLASS.value());

			        	List<Node> childClassNodes = node.getChildren();
			        	childClassNodes.add(roleClassNode);
			        	treeNodes.add(node); 
	  		 	}else{

		  			  LeafNode node = new LeafNode();
		  		      node.setIdentifier(roleQualifications.getId());
		  		      node.setIconCls("role");
		  		      hsMap.put("Identifier",roleQualifications.getQualifies());
		  		      hsMap.put("Name",roleQualifications.getNames().get(0).getValue());
		  		      node.setRecord(hsMap);
		  		      node.setLeaf(true);
		  	  		  node.setType(Type.ROLENODE.value());
		  	  		  node.setText(roleQualifications.getNames().get(0).getValue());
		  	  		 treeNodes.add(node); 
		        }

	  		 }catch(Exception e){
	  			 System.out.println("Exception in getRole: "+e);
	  		 }
		  }
		  
	  }catch(Exception e){
		  
	  }
	  return tree;
  }
  
  public ClassDefinition getIndividualClassDtls(String id){
	  Qmxf qmxf = null;
	  ClassDefinition classDefinition = null;
	  try{
		  qmxf = httpClient.get(Qmxf.class, "/classes/"+id);
		  classDefinition = qmxf.getClassDefinitions().get(0);
		  
	  }catch(Exception e){
		  
	  }
	  return classDefinition;
  }
  
  public void getRoleDetails(String roleId){
	  
  }
  
  public boolean postClass(HttpServletRequest httpRequest)
  {
    try
    {
    	//Enumeration<String> keys = httpRequest.getParameterNames();
    	Qmxf qmxf = new Qmxf();
    	org.iringtools.common.response.Response response = new org.iringtools.common.response.Response();
    	
    	List<ClassDefinition> classDefinitions = qmxf.getClassDefinitions();
    	
    	ClassDefinition classDefinition = new ClassDefinition();
    	
    	//Setting Class name
    	List<Name> names = classDefinition.getNames();
    	Name name = new Name();
    	name.setValue(httpRequest.getParameter("name"));
    	names.add(name);

    	//Setting Class id
    	String id = httpRequest.getParameter("classId");
    	if("addClass".equalsIgnoreCase(id)){
    		response = idGenHttpClient.get(org.iringtools.common.response.Response.class, "/acquireId/params?uri="+httpRequest.getParameter("name")+"&comment=newClass");
    		classDefinition.setId(response.getMessages().getItems().get(0));
    		System.out.println("Generated Id :"+response.getMessages().getItems().get(0));
    	}else
    		classDefinition.setId(httpRequest.getParameter("classId"));
    	
    	
    	//Setting Target Repository
    	qmxf.setTargetRepository(httpRequest.getParameter("targetRepo"));
    	 	
    	//Setting Entity type
    	EntityType entity = new EntityType();
    	entity.setReference(httpRequest.getParameter("entityType"));
    	classDefinition.setEntityType(entity);
    	
    	//Setting Description
    	List<Description> descriptions = classDefinition.getDescriptions();
    	Description desc = new Description();
    	desc.setValue(httpRequest.getParameter("description"));
    	descriptions.add(desc);
    	
    	//Setting Classifications
    	List<Classification> classifications = classDefinition.getClassifications();
        StringTokenizer st = new StringTokenizer(httpRequest.getParameter("classification"), ",");
        while (st.hasMoreTokens())
        {
        	Classification classification = new Classification();
        	ClassDefinition classDtls = getIndividualClassDtls(st.nextToken());
        	classification.setLabel(classDtls.getNames().get(0).getValue());
        	classification.setReference(classDtls.getId());
        	classifications.add(classification);
        }
    	
    	//Setting Specialization
        
    	List<Specialization> specializations = classDefinition.getSpecializations();
        StringTokenizer str = new StringTokenizer(httpRequest.getParameter("specialization"), ",");
        while (str.hasMoreTokens())
        {
        	Specialization specialization = new Specialization();
        	ClassDefinition classDtls = getIndividualClassDtls(str.nextToken());
        	specialization.setLabel(classDtls.getNames().get(0).getValue());
        	specialization.setReference(classDtls.getId());
        	specializations.add(specialization);
        }

        classDefinitions.add(classDefinition);
        
        response = httpClient.post(org.iringtools.common.response.Response.class, "/class", qmxf);
    	
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
        /*while( keys.hasMoreElements() ) {
    	    String key = keys.nextElement();

    	    for( String value : httpRequest.getParameterValues( key ) ) {
    	      System.out.println( "KEY: " + key + " VALUE: " + value.toString() );
    	    }
    	  }
        return true;*/

    }
    catch (Exception e)
    {
      System.out.println("Error Occured");
      e.printStackTrace();
      return false;
    }
  }
  
  public boolean postTemplate(HttpServletRequest httpRequest)
  {    	
	  Qmxf qmxf = new Qmxf();
//	  org.iringtools.common.response.Response response = new org.iringtools.common.response.Response();

	  try
	    {
	    	/*Enumeration<String> keys = httpRequest.getParameterNames();
	    	while( keys.hasMoreElements() ) {
	    	    String key = keys.nextElement();

	    	    for( String value : httpRequest.getParameterValues( key ) ) {
	    	      System.out.println( "KEY: " + key + " VALUE: " + value.toString() );
	    	    }
	    	  }*/
	    	List<TemplateDefinition> templateDefinitions = qmxf.getTemplateDefinitions();
	    	
	    	TemplateDefinition templateDefinition = new TemplateDefinition();
	    	//Setting Template Id
	    	templateDefinition.setId(httpRequest.getParameter("tempId"));
	    	
	    	//Setting Target Repository
	    	qmxf.setTargetRepository(httpRequest.getParameter("targetRepo").replaceFirst(" [Read Only]", ""));

	    	//Setting Description
	    	List<Description> descriptions = templateDefinition.getDescriptions();
	    	Description desc = new Description();
	    	desc.setValue(httpRequest.getParameter("description"));
	    	descriptions.add(desc);
	    	
	    	//Setting Role Definition
	        
//	    	List<RoleDefinition> roleDefinitions = templateDefinition.getRoleDefinitions();
//	        StringTokenizer str = new StringTokenizer(httpRequest.getParameter("roleDefinition"), ",");
//	        while (str.hasMoreTokens())
//	        {
//	        	RoleDefinition roleDefinition = new RoleDefinition();
//	        	ClassDefinition classDtls = getIndividualClassDtls(str.nextToken());
//	        	//specialization.setLabel(classDtls.getNames().get(0).getValue());
//	        	//specialization.setReference(classDtls.getId());
//	        	//specializations.add(specialization);
//	        }

	    	
	    	templateDefinitions.add(templateDefinition);

	    	
	        return true;

    }
    catch (Exception e)
    {
      System.out.println("Error Occured");
      e.printStackTrace();
      return false;
    }
  }
}
