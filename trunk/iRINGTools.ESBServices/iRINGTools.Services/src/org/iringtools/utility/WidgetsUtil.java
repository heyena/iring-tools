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

public class WidgetsUtil
{
  public static Tree toTree(Directory directory)
  {
    Tree tree = new Tree();
    List<Node> scopeNodes = tree.getTreeNodes();

    for (Scope scope : directory.getScopes())
    {
      TreeNode scopeNode = new TreeNode();
      scopeNode.setText(scope.getName());
      scopeNodes.add(scopeNode);

      List<Node> scopeNodeList = scopeNode.getChildren();
      ApplicationData appData = scope.getApplicationData();

      if (appData != null)
      {
        TreeNode appDataNode = new TreeNode();
        appDataNode.setText("Application Data");
        scopeNodeList.add(appDataNode);

        List<Node> appDataNodeList = appDataNode.getChildren();

        for (Application app : appData.getApplications())
        {
          TreeNode appNode = new TreeNode();
          appNode.setText(app.getName());
          appDataNodeList.add(appNode);

          List<Node> appNodeList = appNode.getChildren();

          for (Graph graph : app.getGraphs().getGraphs())
          {
            LeafNode graphNode = new LeafNode();
            graphNode.setId(graph.getId());
            graphNode.setText(graph.getName());
            graphNode.setLeaf(true);
            appNodeList.add(graphNode);
            
            List<Property> properties = graphNode.getProperties();
            Property prop1 = new Property();
            prop1.setName("Name");
            prop1.setValue(graph.getName());
            properties.add(prop1);
            Property prop2 = new Property();
            prop2.setName("Description");
            prop2.setValue(graph.getDescription());
            properties.add(prop2);
            Property prop3 = new Property();
            prop3.setName("Commodity");
            prop3.setValue(graph.getCommodity());
            properties.add(prop3);
          }
        }
      }

      DataExchanges exchangeData = scope.getDataExchanges();

      if (exchangeData != null)
      {
        TreeNode exchangeDataNode = new TreeNode();
        exchangeDataNode.setText("Exchange Data");
        scopeNodeList.add(exchangeDataNode);

        List<Node> exchangeDataNodeList = exchangeDataNode.getChildren();

        for (Commodity commodity : exchangeData.getCommodities())
        {
          TreeNode commodityNode = new TreeNode();
          commodityNode.setText(commodity.getName());
          exchangeDataNodeList.add(commodityNode);

          List<Node> commodityNodeList = commodityNode.getChildren();

          for (Exchange exchange : commodity.getExchanges().getExchanges())
          {
            LeafNode exchangeNode = new LeafNode();
            exchangeNode.setId(exchange.getId());
            exchangeNode.setText(exchange.getName());
            exchangeNode.setLeaf(true);
            
            List<Property> properties = exchangeNode.getProperties();
            Property prop1 = new Property();
            prop1.setName("Id");
            prop1.setValue(exchange.getId());
            properties.add(prop1);
            Property prop2 = new Property();
            prop2.setName("Name");
            prop2.setValue(exchange.getName());
            properties.add(prop2);
            Property prop3 = new Property();
            prop3.setName("Description");
            prop3.setValue(exchange.getDescription());
            properties.add(prop3);
            commodityNodeList.add(exchangeNode);
          }
        }
      }
    }
    
    return tree;
  }

  public static List<Node> federationToTree(Federation federation){
	  
	  Tree tree = new Tree();
	  //List<Tree> tree = new ArrayList<Tree>();
	  List<Node> treeNodes = tree.getTreeNodes();
	  WidgetsUtil wdu = new WidgetsUtil();
	  
	  TreeNode node = new TreeNode();
		//IDGenerators
		node.setId("IDGenerators");
		node.setText("ID Generators");
		node.setIcon("");
		List<Node> children = new ArrayList<Node>();
		for (IDGenerator idgenerator : federation.getIdGenerators().getIdGenerators())
		{
			LeafNode childNode = new LeafNode();
			childNode.setId(Integer.toString(idgenerator.getId()));
			childNode.setText(idgenerator.getName());
			childNode.setIcon("");
			childNode.setLeaf(true);
			List<Property> properties = new ArrayList<Property>();
			properties.add(wdu.setProperty("Name", idgenerator.getName()));
			properties.add(wdu.setProperty("URI", idgenerator.getUri()));
			properties.add(wdu.setProperty("Description", idgenerator.getDescription()));
			
			childNode.setProperties(properties);
						
			children.add(childNode);
		}
		node.setChildren(children);
		treeNodes.add(node);
		
		//Namespaces
		node = new TreeNode();
		node.setId("Namespaces");
		node.setText("Namespaces");
		node.setIcon("");
		children = new ArrayList<Node>();
		for (Namespace namespace : federation.getNamespaces().getNamespaces())
		{
			LeafNode childNode = new LeafNode();
			childNode.setId(namespace.getAlias());
			childNode.setText(namespace.getAlias());
			childNode.setIcon("");
			childNode.setLeaf(true);
			List<Property> properties = new ArrayList<Property>();
			properties.add(wdu.setProperty("Alias", namespace.getAlias()));
			properties.add(wdu.setProperty("URI", namespace.getUri()));
			properties.add(wdu.setProperty("Description", namespace.getDescription()));
			properties.add(wdu.setProperty("Writable", String.valueOf(namespace.isIsWritable())));
			properties.add(wdu.setProperty("Id", String.valueOf(namespace.getIdGenerator())));
						
			childNode.setProperties(properties);
						
			children.add(childNode);
		}
		node.setChildren(children);
		treeNodes.add(node);
		
		//Repositories
		node = new TreeNode();
		node.setId("Repositories");
		node.setText("Repositories");
		node.setIcon("");
		children = new ArrayList<Node>();

		for (Repository repository : federation.getRepositories().getRepositories())
		{
			LeafNode childNode = new LeafNode();
			childNode.setId(repository.getName());
			childNode.setText(repository.getName());
			childNode.setIcon("");
			childNode.setLeaf(true);
			List<Property> properties = new ArrayList<Property>();
			properties.add(wdu.setProperty("URI", repository.getUri()));
			properties.add(wdu.setProperty("Description", repository.getDescription()));
			properties.add(wdu.setProperty("Read Only", String.valueOf(repository.isIsReadOnly())));
			properties.add(wdu.setProperty("Repository Type", repository.getRepositoryType()));
			properties.add(wdu.setProperty("Update URI", repository.getUpdateUri()));
			
			childNode.setProperties(properties);
						
			children.add(childNode);
		}
		node.setChildren(children);
		treeNodes.add(node);
		
		return treeNodes;
	  //return tree;
	  
  }
  public Property setProperty(String name, String value){
		Property prop = new Property();
		prop.setName(name);
		prop.setValue(value);
		return prop;
	}

}
