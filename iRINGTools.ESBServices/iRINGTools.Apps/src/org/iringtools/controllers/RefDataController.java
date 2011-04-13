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
		String query=httpRequest.getParameter("query");
		Type type = Type.fromValue(httpRequest.getParameter("type"));
		switch(type){
		case SEARCH:
			tree = refdata.populate(httpRequest);
			break;
		case CLASS:
			tree = refdata.getClass(httpRequest.getParameter("id"));
			break;
		case SUPERCLASS:
			response = refdata.getSuperClasses(httpRequest.getParameter("id"));
			break;
		case SUBCLASS:
			response = refdata.getSubClasses(httpRequest.getParameter("id"));
			break;
		case CLASSTEMPLATE:
			response = refdata.getTemplates(httpRequest.getParameter("id"));
			break;
		}
    	
		return Action.SUCCESS;
    	}
	
	/*public String getClassifications(){
		String id = httpRequest.getParameter("id");
		tree = refdata.getClass("R19192462550");
		return Action.SUCCESS;
	}*/
	public String getSuperClasses(){
		String id = httpRequest.getParameter("id");
		response = refdata.getSuperClasses(id);
		return Action.SUCCESS;
	}
	
	public String getSubClasses(){
		String id = httpRequest.getParameter("id");
		response = refdata.getSubClasses(id);
		return Action.SUCCESS;
	}
	
	public String getTemplates(){
		String id = httpRequest.getParameter("id");
		response = refdata.getTemplates(id);
		return Action.SUCCESS;
	}
	
}
