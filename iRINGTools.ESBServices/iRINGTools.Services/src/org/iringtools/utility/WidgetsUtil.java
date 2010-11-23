package org.iringtools.utility;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;

public class WidgetsUtil {
	
	private static Property createProperty(String name, String value) {
		Property property = new Property();
		property.setName(name);
		property.setValue(value);
		return property;
	}
	
	public static Tree toTree(Directory directory) {
		Tree tree = new Tree();
		List<Node> scopeNodes = tree.getTreeNodes();

		for (Scope scope : directory.getScopes()) {
			TreeNode scopeNode = new TreeNode();
			scopeNode.setText(scope.getName());
			scopeNode.setCls("scope");

			scopeNodes.add(scopeNode);

			List<Node> scopeChildren = scopeNode.getChildren();

			ApplicationData appData = scope.getApplicationData();

			if (appData != null) {
				TreeNode appDataNode = new TreeNode();
				appDataNode.setText("Application Data");
				appDataNode.setCls("folder");

				scopeChildren.add(appDataNode);

				List<Node> applicationNodes = appDataNode.getChildren();

				for (Application app : appData.getApplications()) {
					TreeNode appNode = new TreeNode();
					appNode.setText(app.getName());
					appNode.setCls("application");

					applicationNodes.add(appNode);

					List<Node> graphNodes = appNode.getChildren();

					for (Graph graph : app.getGraphs().getItems()) {
						LeafNode graphNode = new LeafNode();
						graphNode.setId(graph.getId());
						graphNode.setText(graph.getName());
						graphNode.setCls("file-table");
						graphNode.setLeaf(true);

						List<Property> properties = graphNode.getProperties();
						properties.add(createProperty("Name", graph.getName()));
						properties.add(createProperty("Description",
								graph.getDescription()));
						properties.add(createProperty("Commodity",
								graph.getCommodity()));

						graphNodes.add(graphNode);
					}
				}
			}

			DataExchanges exchangeData = scope.getDataExchanges();

			if (exchangeData != null) {
				TreeNode exchangeDataNode = new TreeNode();
				exchangeDataNode.setText("Exchange Data");
				exchangeDataNode.setCls("folder");

				scopeChildren.add(exchangeDataNode);

				List<Node> commodityNodes = exchangeDataNode.getChildren();

				for (Commodity commodity : exchangeData.getCommodities()) {
					TreeNode commodityNode = new TreeNode();
					commodityNode.setText(commodity.getName());
					commodityNode.setCls("commodity");

					commodityNodes.add(commodityNode);

					List<Node> exchangeNodes = commodityNode.getChildren();

					for (Exchange exchange : commodity.getExchanges().getItems()) {
						LeafNode exchangeNode = new LeafNode();
						exchangeNode.setId(exchange.getId());
						exchangeNode.setText(exchange.getName());
						exchangeNode.setCls("file-table-diff");
						exchangeNode.setLeaf(true);

						List<Property> properties = exchangeNode
								.getProperties();
						properties.add(createProperty("Id", exchange.getId()));
						properties.add(createProperty("Name",
								exchange.getName()));
						properties.add(createProperty("Description",
								exchange.getDescription()));

						exchangeNodes.add(exchangeNode);
					}
				}
			}
		}

		return tree;
	}

	// TODO: Review this code, and ask questions.
	// Basically, the TreePanel will handle a generic JSON.
	// So, make a generic JSON and get rid of old way
	// Used above as a guide...
	// SetCls is used to set the CSS class that should be setup.
	// Use the objects... working with references.
	public static Tree federationToTree(Federation federation) {

		Tree tree = new Tree();
		List<Node> treeNodes = tree.getTreeNodes();

		TreeNode generatorsNode = new TreeNode();
		generatorsNode.setText("ID Generators");
		generatorsNode.setCls("folder");
		treeNodes.add(generatorsNode);

		List<Node> generatorNodes = generatorsNode.getChildren();

		for (IDGenerator idgenerator : federation.getIdGenerators().getItems()) {
			LeafNode generatorNode = new LeafNode();
			generatorNode.setId(Integer.toString(idgenerator.getId()));
			generatorNode.setText(idgenerator.getName());
			generatorNode.setCls("generator");
			generatorNode.setLeaf(true);

			List<Property> properties = generatorNode.getProperties();
			properties.add(createProperty("Name", idgenerator.getName()));
			properties.add(createProperty("URI", idgenerator.getUri()));
			properties.add(createProperty("Description",
					idgenerator.getDescription()));

			generatorNodes.add(generatorNode);
		}

		// Namespaces
		TreeNode namespacesNode = new TreeNode();
		namespacesNode.setText("Namespaces");
		namespacesNode.setCls("folder");
		treeNodes.add(namespacesNode);
		
		List<Node> namespaceNodes = namespacesNode.getChildren();

		for (Namespace namespace : federation.getNamespaces().getItems()) {
			LeafNode namespaceNode = new LeafNode();
			namespaceNode.setId(Integer.toString(namespace.getId()));
			namespaceNode.setText(namespace.getAlias());
			namespaceNode.setCls("namespace");
			namespaceNode.setLeaf(true);

			List<Property> properties = namespaceNode.getProperties();
			properties.add(createProperty("Alias", namespace.getAlias()));
			properties.add(createProperty("URI", namespace.getUri()));
			properties.add(createProperty("Description",
					namespace.getDescription()));
			properties.add(createProperty("Writable",
					String.valueOf(namespace.isIsWritable())));
			properties.add(createProperty("Id",
					String.valueOf(namespace.getIdGenerator())));

			namespaceNodes.add(namespaceNode);
		}

		// Repositories
		//TODO: Complete this...
		
//		TreeNode repositoriesNode = new TreeNode();
//		repositoriesNode.setText("Repositories");
//		treeNodes.add(repositoriesNode);
//
//		node = new TreeNode();
//		node.setId("Repositories");
//		node.setText("Repositories");
//		node.setIcon("");
//		children = new ArrayList<Node>();
//
//		for (Repository repository : federation.getRepositories()
//				.getRepositories()) {
//			LeafNode childNode = new LeafNode();
//			childNode.setId(repository.getName());
//			childNode.setText(repository.getName());
//			childNode.setIcon("");
//			childNode.setLeaf(true);
//			List<Property> properties = new ArrayList<Property>();
//			properties.add(wdu.setProperty("URI", repository.getUri()));
//			properties.add(wdu.setProperty("Description",
//					repository.getDescription()));
//			properties.add(wdu.setProperty("Read Only",
//					String.valueOf(repository.isIsReadOnly())));
//			properties.add(wdu.setProperty("Repository Type",
//					repository.getRepositoryType()));
//			properties.add(wdu.setProperty("Update URI",
//					repository.getUpdateUri()));
//
//			childNode.setProperties(properties);
//
//			children.add(childNode);
//		}
//		node.setChildren(children);
//		treeNodes.add(node);

		return tree;

	}
}
