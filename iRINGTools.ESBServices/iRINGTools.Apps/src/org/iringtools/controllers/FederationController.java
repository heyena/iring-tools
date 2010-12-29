package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.models.FederationModel;
import org.iringtools.models.Result;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;
import org.apache.struts2.interceptor.ServletRequestAware;


public class FederationController  implements ServletRequestAware{
	
	private FederationModel federation;
	private Tree tree;
	private HttpServletRequest httpRequest = null;
	private Result result=new Result();
	private String[] repositoryTypes;
	
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
	
	public String[] getRepositoryTypes() {
		return repositoryTypes;
	}

	public void setRepositoryTypes(String[] repositoryTypes) {
		this.repositoryTypes = repositoryTypes;
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
	public String deleteNode() {		
		System.out.println("Reaching deleteNode");
		boolean successStatus = federation.deleteNode(httpRequest);
		result.setSuccess(successStatus);
        return Action.SUCCESS;
	}
	
	public String getRepoTypes() {
		RepositoryType[] repoTypes = RepositoryType.values();
		String returnArray[] = new String[(repoTypes.length)];
		int i = 0;
		for (RepositoryType repoType : repoTypes)
		{
			returnArray[i] = repoType.value();
			i++;
		}
		repositoryTypes = returnArray;
		return Action.SUCCESS;
	}
}
