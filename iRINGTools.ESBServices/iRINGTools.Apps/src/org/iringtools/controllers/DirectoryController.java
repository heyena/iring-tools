package org.iringtools.controllers;

import java.io.IOException;
import java.util.Map;

import javax.xml.bind.JAXBException;

import org.apache.struts2.interceptor.SessionAware;
import org.iringtools.models.DirectoryModel;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;
import com.opensymphony.xwork2.ActionSupport;

public class DirectoryController extends ActionSupport implements SessionAware{
	
	private static final long serialVersionUID = 1L;
	
	private Map<String, Object> session;
	
	private DirectoryModel directory;
	private Tree tree;
	
	public DirectoryController()
	{
		directory = new DirectoryModel();
	}

	public void setTree(Tree tree) {
		this.tree = tree;
	}

	public Tree getTree() {
		return tree;
	}

	public String getDirectory() {
		directory.populate();
		tree = directory.toTree();
		session.clear();
        return Action.SUCCESS;
	}
	
	public String postDirectory() throws IOException, JAXBException {		
		directory.readTree(tree);
		directory.save();
        return Action.SUCCESS;
	}
	
	@Override
	public void setSession(Map<String, Object> session) {
		this.session = session;		
	}
}
