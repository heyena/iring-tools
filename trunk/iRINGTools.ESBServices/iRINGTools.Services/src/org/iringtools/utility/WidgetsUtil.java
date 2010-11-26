package org.iringtools.utility;

import java.util.List;

import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;

public class WidgetsUtil {
	
	public static Property createProperty(String name, String value) {
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

}
