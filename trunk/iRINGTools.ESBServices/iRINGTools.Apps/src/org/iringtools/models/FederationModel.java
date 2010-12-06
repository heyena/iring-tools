package org.iringtools.models;

import java.util.List;

import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;
import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;
import org.iringtools.utility.WidgetsUtil;

import com.opensymphony.xwork2.ActionContext;

public class FederationModel {

	private Federation federation = null;
	private String URI;

	public FederationModel() {
		try {
			URI = ActionContext.getContext().getApplication()
					.get("RefDataServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in RefDataServiceUri :" + e);
		}
		federation = null;
	}

	public void populate() {

		try {
			WebClient webclient = new WebClient(URI);
			federation = webclient.get(Federation.class, "/federation");
		} catch (WebClientException wce) {
			System.out.println("WebClientException :" + wce);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void save() {

		try {
			WebClient webclient = new WebClient(URI);
			Response response = webclient.post(Response.class, "/federation",
					federation);

			if (response.getLevel().value().equalsIgnoreCase("Error")) {
				throw new WebClientException("Response Error");
			}else if (response.getLevel().value().equalsIgnoreCase("Warning")) {
				throw new WebClientException("Response Warning");
			}
			System.out.println("response.getLevel().value() :"+response.getLevel().value());

		} catch (WebClientException wce) {
			System.out.println("WebClientException :" + wce);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public IDGenerators getIdGenerators() {
		return federation.getIdGenerators();
	}

	public Namespaces getNamespaces() {
		return federation.getNamespaces();
	}

	public Repositories getRepositories() {
		return federation.getRepositories();
	}

	public Federation getFederation() {
		return federation;
	}

	public Tree toTree() {

		Tree tree = new Tree();
		List<Node> treeNodes = tree.getTreeNodes();

		TreeNode generatorsNode = new TreeNode();
		generatorsNode.setText("ID Generators");
		generatorsNode.setIconCls("folder");
		treeNodes.add(generatorsNode);

		List<Node> generatorNodes = generatorsNode.getChildren();

		for (IDGenerator idgenerator : federation.getIdGenerators().getItems()) {
			LeafNode generatorNode = new LeafNode();
			generatorNode.setId(idgenerator.getName());
			generatorNode.setText(idgenerator.getName());
			generatorNode.setIconCls("generator");
			generatorNode.setLeaf(true);

			List<Property> properties = generatorNode.getProperties();
			properties.add(WidgetsUtil.createProperty("Name",
					idgenerator.getName()));
			properties.add(WidgetsUtil.createProperty("URI",
					idgenerator.getUri()));
			properties.add(WidgetsUtil.createProperty("Description",
					idgenerator.getDescription()));

			generatorNodes.add(generatorNode);
		}

		// Namespaces
		TreeNode namespacesNode = new TreeNode();
		namespacesNode.setText("Namespaces");
		namespacesNode.setIconCls("folder");
		treeNodes.add(namespacesNode);

		List<Node> namespaceNodes = namespacesNode.getChildren();

		for (Namespace namespace : federation.getNamespaces().getItems()) {
			LeafNode namespaceNode = new LeafNode();
			namespaceNode.setId(namespace.getAlias());
			namespaceNode.setText(namespace.getAlias());
			namespaceNode.setIconCls("namespace");
			namespaceNode.setLeaf(true);

			List<Property> properties = namespaceNode.getProperties();
			properties.add(WidgetsUtil.createProperty("Alias",
					namespace.getAlias()));
			properties
					.add(WidgetsUtil.createProperty("URI", namespace.getUri()));
			properties.add(WidgetsUtil.createProperty("Description",
					namespace.getDescription()));
			properties.add(WidgetsUtil.createProperty("Writable",
					String.valueOf(namespace.isIsWritable())));

			namespaceNodes.add(namespaceNode);
		}

		// Repositories

		TreeNode repositoriesNode = new TreeNode();
		repositoriesNode.setText("Repositories");
		repositoriesNode.setIconCls("folder");
		treeNodes.add(repositoriesNode);

		List<Node> repositoryNodes = repositoriesNode.getChildren();

		for (Repository repository : federation.getRepositories().getItems()) {
			LeafNode repositoryNode = new LeafNode();
			repositoryNode.setId(repository.getName());
			repositoryNode.setText(repository.getName());
			repositoryNode.setIconCls("repository");
			repositoryNode.setLeaf(true);

			List<Property> properties = repositoryNode.getProperties();
			properties.add(WidgetsUtil.createProperty("URI",
					repository.getUri()));
			properties.add(WidgetsUtil.createProperty("Description",
					repository.getDescription()));
			properties.add(WidgetsUtil.createProperty("Read Only",
					String.valueOf(repository.isIsReadOnly())));
			properties.add(WidgetsUtil.createProperty("Repository Type",
					repository.getRepositoryType()));
			properties.add(WidgetsUtil.createProperty("Update URI",
					repository.getUpdateUri()));

			repositoryNodes.add(repositoryNode);
		}

		return tree;

	}
	
	public void readTree(Tree tree) {
		//TODO: Read the tree!
	}
}
