package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.apache.struts2.interceptor.ServletRequestAware;
import org.iringtools.models.RefDataModel;
import org.iringtools.refdata.response.Response;
import org.iringtools.widgets.tree.Tree;

import com.opensymphony.xwork2.Action;


public class RefDataController implements ServletRequestAware{
	
	private RefDataModel refdata;
	private Tree tree;
	private Response response;
	public Response getResponse() {
		return response;
	}

	public void setResponse(Response response) {
		this.response = response;
	}

	private HttpServletRequest httpRequest = null;
	
	public RefDataController()
	{
		refdata = new RefDataModel();
	}

	public void setTree(Tree tree) {
		this.tree = tree;
	}

	public Tree getTree() {
		return tree;
	}
	
	public void setServletRequest(HttpServletRequest request) {
		this.httpRequest = request;  
		} 
	
	public String searchPage() {
		//System.out.println("callled.........");
		String query=httpRequest.getParameter("query");
    	if(query!=null){
    		response = refdata.populate(httpRequest);
    		//	tree = refdata.toTree();
    	       
    	}else{
    		//System.out.println('{"items":null,"message":null,"success":false,"total":0,"errors":null}');
    	}
    	//System.out.println("Finished........."+response.getEntities().getItems().size());
    	 return Action.SUCCESS;
    	}
	
}
