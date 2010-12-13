package org.iringtools.controllers;

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
import org.iringtools.utility.HttpClient;

import com.opensymphony.xwork2.ActionSupport;

public class DirectoryTreeController extends ActionSupport
{
  private static final long serialVersionUID = 1L;

  private String directoryURL;
  private Tree directoryTree;

  public String execute() throws Exception
  {
    try
    {
      HttpClient httpClient = new HttpClient();
      Directory directory = httpClient.get(Directory.class, directoryURL);
      directoryTree = toTree(directory);
    }
    catch (Exception ex)
    {
      throw ex;
    }

    return SUCCESS;
  }
  
  public void setDirectoryURL(String directoryURL)
  {
    this.directoryURL = directoryURL;
  }

  public String getDirectoryURL()
  {
    return directoryURL;
  }

  public void setDirectoryTree(Tree directoryTree)
  {
    this.directoryTree = directoryTree;
  }

  public Tree getDirectoryTree()
  {
    return directoryTree;
  }
  
  private Tree toTree(Directory directory)
  {
    Tree tree = new Tree();
    List<Node> scopeNodes = tree.getItems();

    for (Scope scope : directory.getItems())
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

        for (Application app : appData.getItems())
        {
          TreeNode appNode = new TreeNode();
          appNode.setText(app.getName());
          appDataNodeList.add(appNode);

          List<Node> appNodeList = appNode.getChildren();

          for (Graph graph : app.getGraphs().getItems())
          {
            LeafNode graphNode = new LeafNode();
            graphNode.setId(graph.getId());
            graphNode.setText(graph.getName());
            graphNode.setLeaf(true);
            appNodeList.add(graphNode);
            
            List<Property> properties = graphNode.getItems();
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

        for (Commodity commodity : exchangeData.getItems())
        {
          TreeNode commodityNode = new TreeNode();
          commodityNode.setText(commodity.getName());
          exchangeDataNodeList.add(commodityNode);

          List<Node> commodityNodeList = commodityNode.getChildren();

          for (Exchange exchange : commodity.getExchanges().getItems())
          {
            LeafNode exchangeNode = new LeafNode();
            exchangeNode.setId(exchange.getId());
            exchangeNode.setText(exchange.getName());
            exchangeNode.setLeaf(true);
            
            List<Property> properties = exchangeNode.getItems();
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
}
