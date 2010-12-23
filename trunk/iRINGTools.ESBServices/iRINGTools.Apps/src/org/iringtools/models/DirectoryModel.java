package org.iringtools.models;

import java.util.List;

import org.iringtools.common.response.Response;
import org.iringtools.directory.*;
import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;
import org.iringtools.utility.HttpClient;

import com.opensymphony.xwork2.ActionContext;

public class DirectoryModel
{
  private Directory directory = null;
  private HttpClient httpClient = null;

  public DirectoryModel()
  {
    try
    {
      String uri = ActionContext.getContext().getApplication().get("DirectoryServiceUri").toString();
      httpClient = new HttpClient(uri);
    }
    catch (Exception e)
    {
      System.out.println("Exception in DirectoryServiceUri :" + e);
    }
    directory = null;
  }

  public void populate()
  {

    try
    {
      directory = httpClient.get(Directory.class, "/directory");
    }
    catch (Exception e)
    {
      System.out.println("Exception :" + e);
    }
  }

  public void save()
  {
    try
    {
      Response response = httpClient.post(Response.class, "/directory", directory);
      System.out.println("response.getLevel().value() :" + response.getLevel().value());
    }
    catch (Exception e)
    {
      System.out.println("Exception :" + e);
    }
  }

  public List<Scope> getScopes()
  {
    return directory.getItems();
  }

  public Directory getDirectory()
  {
    return directory;
  }

  public Tree toTree()
  {

    Tree tree = new Tree();
    List<Node> scopeNodes = tree.getNodes();

    int ci = 1, si = 1, ei = 1, adi = 1;

    for (Scope scope : directory.getItems())
    {
      TreeNode scopeNode = new TreeNode();
      scopeNode.setText(scope.getName());
      scopeNode.setIconCls("scope");
      scopeNode.setId("scope" + si);
      si++;
      scopeNodes.add(scopeNode);

      List<Node> scopeNodeList = scopeNode.getChildren();
      ApplicationData appData = scope.getApplicationData();

      if (appData != null)
      {
        TreeNode appDataNode = new TreeNode();
        appDataNode.setText("Application Data");
        appDataNode.setIconCls("folder");
        appDataNode.setId("application data" + adi);
        adi++;
        scopeNodeList.add(appDataNode);

        List<Node> appDataNodeList = appDataNode.getChildren();

        for (Application app : appData.getItems())
        {
          TreeNode appNode = new TreeNode();
          appNode.setText(app.getName());
          appNode.setId(app.getName() + app.getId());
          appNode.setIconCls("application");
          appDataNodeList.add(appNode);

          List<Node> appNodeList = appNode.getChildren();

          for (Graph graph : app.getGraphs().getItems())
          {
            LeafNode graphNode = new LeafNode();
            graphNode.setId(graph.getName() + graph.getId());
            graphNode.setText(graph.getName());
            graphNode.setIconCls("graph");
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
        exchangeDataNode.setText("Data Exchanges");
        exchangeDataNode.setId("data exchanges" + ei);
        ei++;
        exchangeDataNode.setIconCls("folder");
        scopeNodeList.add(exchangeDataNode);

        List<Node> exchangeDataNodeList = exchangeDataNode.getChildren();

        for (Commodity commodity : exchangeData.getItems())
        {
          TreeNode commodityNode = new TreeNode();
          commodityNode.setText(commodity.getName());
          commodityNode.setId("commodity" + ci);
          ci++;
          commodityNode.setIconCls("commodity");
          exchangeDataNodeList.add(commodityNode);

          List<Node> commodityNodeList = commodityNode.getChildren();

          for (Exchange exchange : commodity.getExchanges().getItems())
          {
            LeafNode exchangeNode = new LeafNode();
            exchangeNode.setId(exchange.getName() + exchange.getId());
            exchangeNode.setText(exchange.getName());
            exchangeNode.setIconCls("exchange");
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

  public void readTree(Tree tree)
  {
    // TODO: Read the tree!
  }
}
