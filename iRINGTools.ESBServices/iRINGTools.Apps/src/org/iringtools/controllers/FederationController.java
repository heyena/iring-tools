package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.models.FederationModel;
import org.iringtools.models.Result;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;
import org.apache.struts2.interceptor.ServletRequestAware;


public class FederationController  implements ServletRequestAware{
	
	private FederationModel federation;
	private Tree tree;
	private HttpServletRequest httpRequest = null;
	private Result result=new Result();
	
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
	
	public Result getResult() {
		return result;
	}

	public void setResult(Result result) {
		this.result = result;
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
		boolean successStatus = federation.readTree(httpRequest);
		result.setSuccess(successStatus);
		//result.setMessage("Details Successfully saved!");
		//federation.save();
        return Action.SUCCESS;
	}
}
