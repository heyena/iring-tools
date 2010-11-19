package org.iringtools.federationmanager.models;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.federationmanager.ext.ParentTreeNode;
import org.iringtools.federationmanager.ext.TreeNode;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;

public class FederationModel {
	
	public Federation getFederation()
	  {
		String URI="http://10.206.120.188:8080/services/refdata";
		Federation federation = null;
		try{
			WebClient webclient = new WebClient(URI);
			federation = webclient.get(Federation.class, "/federation");			
		}catch(WebClientException wce)
		{
			System.out.println("WebClientException :"+wce);
		}catch(Exception e)
		{
			System.out.println("Exception :"+e);
		}
		return federation;
	  }
	
	public List<ParentTreeNode> getFederationTree()
	{
		Federation federation = getFederation();
		
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
		
		return tree;		
	}

}
