package org.iringtools.federationmanager.controllers;

import org.iringtools.federationmanager.models.FederationModel;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.utility.WidgetsUtil;
import com.opensymphony.xwork2.Action;

public class FederationController {
	
	private FederationModel federation;
	private Tree federationTree;
	
	public FederationController()
	{
		federation = new FederationModel();
	}

	public void setFederationTree(Tree federationTree) {
		this.federationTree = federationTree;
	}

	public Tree getFederationTree() {
		return federationTree;
	}

	public String getFederation() {
		federation.populate();
		generateFederationTree();
        return Action.SUCCESS;
	}
	
	public String postFederation() {		
		readFederationTree();
		federation.save();
        return Action.SUCCESS;
	}

	private void generateFederationTree()
	{
		federationTree = WidgetsUtil.federationToTree(federation.getFederation());
	}
	
	private void readFederationTree()
	{
		//TODO: Add another method (treeToFederation) to WidgetsUtil to convert Tree to Federation
	}

}
