package org.iringtools.federationmanager.controllers;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.federationmanager.ext.ParentTreeNode;
import org.iringtools.federationmanager.ext.TreeNode;
import org.iringtools.federationmanager.models.FederationModel;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.utility.WidgetsUtil;

import com.opensymphony.xwork2.Action;

public class FederationController {
	
	private FederationModel federation;
	//TODO: Change this to a Tree
	private List<Node> federationTree;
	
	public FederationController()
	{
		federation = new FederationModel();
	}

	//TODO: Change this to a Tree
	public void setFederationTree(List<Node> federationTree) {
		this.federationTree = federationTree;
	}

	//TODO: Change this to a Tree
	public List<Node> getFederationTree() {
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
	
	private void generateFederationTree(int i)
	{
		//TODO: Add another method to WidgetsUtil to convert Federation to Tree
		
		List<ParentTreeNode> tree = new ArrayList<ParentTreeNode>();
		TreeNode node = new TreeNode();
		//IDGenerators
		ParentTreeNode idGenTreeNode = new ParentTreeNode();
		idGenTreeNode.setId("IDGenerators");
		idGenTreeNode.setText("ID Generators");
		idGenTreeNode.setIcon("");
		idGenTreeNode.setExpanded(true);

		List<TreeNode> children = new ArrayList<TreeNode>();

		for (IDGenerator idgenerator : federation.getIdGenerators().getIdGenerators())
		{
			TreeNode childNode;
			childNode = node.setIdGenDetails(idgenerator);
			
			children.add(childNode);
		}
		idGenTreeNode.setChildren(children);
		
		//Namespaces
		ParentTreeNode namespaceTreeNode = new ParentTreeNode();
		namespaceTreeNode.setId("Namespaces");
		namespaceTreeNode.setText("Namespaces");
		namespaceTreeNode.setIcon("");
		namespaceTreeNode.setExpanded(true);
		

		children = new ArrayList<TreeNode>();

		for (Namespace namespace : federation.getNamespaces().getNamespaces())
		{
			TreeNode childNode;
			childNode = node.setNameSpaceDet(namespace);
			children.add(childNode);
		}
		namespaceTreeNode.setChildren(children);
		
		//Repositories
		ParentTreeNode repoTreeNode = new ParentTreeNode();
		repoTreeNode.setId("Repositories");
		repoTreeNode.setText("Repositories");
		repoTreeNode.setIcon("");
		repoTreeNode.setExpanded(true);

		children = new ArrayList<TreeNode>();

		for (Repository repository : federation.getRepositories().getRepositories())
		{
			TreeNode childNode;
			childNode = node.setRepositoryDetails(repository);
			children.add(childNode);
		}
		repoTreeNode.setChildren(children);
		
		//Final Tree
		tree.add(idGenTreeNode);
		tree.add(namespaceTreeNode);
		tree.add(repoTreeNode);
		
		//setFederationTree(tree);		
	}

	private void readFederationTree()
	{
		//TODO: Add another method (ToFederation) to WidgetsUtil to convert Tree to Federation
	}

}
