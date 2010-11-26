package org.iringtools.controllers;

import org.iringtools.models.FederationModel;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;

public class FederationController {
	
	private FederationModel federation;
	private Tree tree;
	
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

	public String getFederation() {
		federation.populate();
		tree = federation.toTree();
        return Action.SUCCESS;
	}
	
	public String postFederation() {		
		federation.readTree(tree);
		federation.save();
        return Action.SUCCESS;
	}
}
