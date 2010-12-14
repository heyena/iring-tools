package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.models.FederationModel;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;
import org.apache.struts2.interceptor.ServletRequestAware;


public class FederationController  implements ServletRequestAware{
	
	private FederationModel federation;
	private Tree tree;
	private HttpServletRequest httpRequest = null;
	private String success;
	
	public FederationController()
	{
		federation = new FederationModel();
	}

	public void setTree(Tree tree) {
		this.tree = tree;
	}

	public Tree getTree() {
		return tree;
	}
	
	public String getSuccess() {
		return success;
	}

	public void setSuccess(String success) {
		this.success = success;
	}

	public void setServletRequest(HttpServletRequest request) {
		this.httpRequest = request;  
		} 

	public String getFederation() {
		federation.populate();
		tree = federation.toTree();
        return Action.SUCCESS;
	}
	
	public String postFederation() {		
		System.out.println("Reaching post Federation");
		federation.readTree(httpRequest);
		//federation.save();
        return Action.SUCCESS;
	}
}
