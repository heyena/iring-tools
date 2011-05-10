package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.apache.struts2.interceptor.ServletRequestAware;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.models.RefDataModel;
import org.iringtools.refdata.response.Response;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.Type;

import com.opensymphony.xwork2.Action;


public class RefDataController implements ServletRequestAware{
	
	private RefDataModel refdata;
	private Tree tree;
	private Response response;
	private Qmxf qmxf;
	public Qmxf getQmxf() {
		return qmxf;
	}

	public void setQmxf(Qmxf qmxf) {
		this.qmxf = qmxf;
	}

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
		//String query=httpRequest.getParameter("query");
		Type type = Type.fromValue(httpRequest.getParameter("type"));

		switch(type){
		case SEARCH:
			tree = refdata.populate(httpRequest);
			break;
		case CLASS:
			tree = refdata.getClass(httpRequest.getParameter("id"));
			break;
		case CLASSIFICATION:
			tree = refdata.getClass(httpRequest.getParameter("id"));
			break;
		case SUPERCLASS:
			tree = refdata.getSubSuperClasses(httpRequest.getParameter("id"),"Super");
			break;
		case SUBCLASS:
			tree = refdata.getSubSuperClasses(httpRequest.getParameter("id"), "Sub");
			break;
		case CLASSTEMPLATE:
			tree = refdata.getTemplates(httpRequest.getParameter("id"));
			break;
		case TEMPLATENODE:
			tree = refdata.getRole(httpRequest.getParameter("id"));
			break;
		}
    	
		return Action.SUCCESS;
    	}
	
	public String getTemplates(){
		String id = httpRequest.getParameter("id");
		tree = refdata.getTemplates(id);
		return Action.SUCCESS;
	}
	/*public String getRoles(){
		String id = httpRequest.getParameter("id");
		qmxf = refdata.getRole(id);
		return Action.SUCCESS;
	}*/
	public String postClass() {		
		System.out.println("Reaching post Class");
		boolean successStatus = refdata.postClass(httpRequest);
        return Action.SUCCESS;
	}
	public String postTemplate() {		
		System.out.println("Reaching post Class");
		boolean successStatus = refdata.postTemplate(httpRequest);
        return Action.SUCCESS;
	}	
}
