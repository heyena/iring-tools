package org.iringtools.federationmanager.controllers;

import java.util.List;

import org.iringtools.federationmanager.ext.ParentTreeNode;
import org.iringtools.federationmanager.models.FederationModel;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;

import com.opensymphony.xwork2.Action;

public class FederationController {
	
	private FederationModel model;
	
	private Federation federation;
	private Repositories repositories;
	private Namespaces namespaces;
	private IDGenerators idGenerators;
	private List<ParentTreeNode> federationTree;
	
	
	public FederationController()
	{
		model = new FederationModel();
		federation = model.getFederation();
		federationTree = model.getFederationTree();
		
		repositories = federation.getRepositories();
		namespaces = federation.getNamespaces();
		idGenerators = federation.getIdGenerators();
	}
	
	public String execute() {
        return Action.SUCCESS;
	}

	public Federation getFederation() {
		return federation;
	}

	public void setFederation(Federation federation) {
		this.federation = federation;
	}

	public Repositories getRepositories() {
		return repositories;
	}

	public void setRepositories(Repositories repositories) {
		this.repositories = repositories;
	}

	public Namespaces getNamespaces() {
		return namespaces;
	}

	public void setNamespaces(Namespaces namespaces) {
		this.namespaces = namespaces;
	}

	public IDGenerators getIdGenerators() {
		return idGenerators;
	}

	public void setIdGenerators(IDGenerators idGenerators) {
		this.idGenerators = idGenerators;
	}

	public List<ParentTreeNode> getFederationTree() {
		return federationTree;
	}

	public void setFederationTree(List<ParentTreeNode> federationTree) {
		this.federationTree = federationTree;
	}
}
