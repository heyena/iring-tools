package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

public class DirectoryModel 
{
  private Map<String, Object> settings;
  
  public DirectoryModel(Map<String, Object> settings)
  {
    this.settings = settings;
  }
  
  public Tree getDirectoryTree(String directoryUrl) throws HttpClientException
  {
    HttpClient httpClient = new HttpClient(directoryUrl);
    HttpUtils.addHttpHeaders(settings, httpClient);
    
    Directory directory = httpClient.get(Directory.class);
    return directoryToTree(directory);
  }
  
  private Tree directoryToTree(Directory directory)
  {
    Tree tree = new Tree();
    List<Node> scopeNodes = tree.getNodes();

    for (Scope scope : directory.getItems())
    {
      TreeNode scopeNode = new TreeNode();
      scopeNode.setText(scope.getName());
      scopeNode.setIconCls("scope");
      scopeNodes.add(scopeNode);

      List<Node> scopeNodeList = scopeNode.getChildren();
      ApplicationData appData = scope.getApplicationData();

      if (appData != null)
      {
        TreeNode appDataNode = new TreeNode();
        appDataNode.setText("Application Data");
        appDataNode.setIconCls("folder");
        scopeNodeList.add(appDataNode);

        List<Node> appDataNodeList = appDataNode.getChildren();

        for (Application app : appData.getItems())
        {
          TreeNode appNode = new TreeNode();
          appNode.setText(app.getName());
          appNode.setIconCls("application");
          appDataNodeList.add(appNode);

          String context = (app.getContext() != null && app.getContext().length() > 0)
              ? app.getContext() : scope.getName();
          app.setContext(context);

          List<Node> appNodeList = appNode.getChildren();          
          
          for (Graph graph : app.getGraphs().getItems())
          {
            LeafNode graphNode = new LeafNode();
            graphNode.setText(graph.getName());
            graphNode.setIconCls("graph");
            graphNode.setLeaf(true);
            appNodeList.add(graphNode);
            
            HashMap<String, String> properties = graphNode.getProperties();
            properties.put("Context", context);
            properties.put("Name", graph.getName());
            properties.put("Description", graph.getDescription());
            properties.put("Base URI", app.getBaseUri());
            properties.put("Commodity", graph.getCommodity());
          }
        }
      }

      DataExchanges exchangeData = scope.getDataExchanges();

      if (exchangeData != null)
      {
        TreeNode exchangeDataNode = new TreeNode();
        exchangeDataNode.setText("Data Exchanges");
        exchangeDataNode.setIconCls("folder");
        scopeNodeList.add(exchangeDataNode);

        List<Node> exchangeDataNodeList = exchangeDataNode.getChildren();

        for (Commodity commodity : exchangeData.getItems())
        {
          TreeNode commodityNode = new TreeNode();
          commodityNode.setText(commodity.getName());
          commodityNode.setIconCls("commodity");
          exchangeDataNodeList.add(commodityNode);

          List<Node> commodityNodeList = commodityNode.getChildren();

          for (Exchange exchange : commodity.getExchanges().getItems())
          {
            LeafNode exchangeNode = new LeafNode();
            exchangeNode.setText(exchange.getName());
            exchangeNode.setIconCls("exchange");
            exchangeNode.setLeaf(true);
            commodityNodeList.add(exchangeNode);
            
            HashMap<String, String> properties = exchangeNode.getProperties();            
            properties.put("Id", exchange.getId());
            properties.put("Name", exchange.getName());
            properties.put("Description", exchange.getDescription());
          }
        }
      }
    }
    
    return tree;
  } 
}
