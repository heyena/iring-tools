package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.library.directory.DirectoryProvider;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

public class DirectoryModel 
{
  private DirectoryProvider provider;
  private Map<String, Object> settings;
  
  public DirectoryModel(Map<String, Object> settings)
  {
    this.settings = settings;
    provider = new DirectoryProvider(settings);
  }
  
  public Directory getDirectory() throws Exception
  {
    return provider.getDirectory();
  }
  
  public Tree directoryToTree(Directory directory)
  {
    Tree tree = new Tree();
    HashMap<String, String> properties = new HashMap<String, String>();
    List<Node> scopeNodes = tree.getNodes();

    for (Scope scope : directory.getScope())
    {
      TreeNode scopeNode = new TreeNode();
      scopeNode.setText(scope.getName());
      scopeNode.setIconCls("scope");
      scopeNodes.add(scopeNode);
      
      properties = scopeNode.getProperties();
      properties.put("Name", scope.getName());

      List<Node> scopeNodeList = scopeNode.getChildren();
      ApplicationData appData = scope.getApplicationData();

      if (appData != null)
      {
        TreeNode appDataNode = new TreeNode();
        appDataNode.setText("Application Data");
        appDataNode.setIconCls("folder");
        scopeNodeList.add(appDataNode);

        List<Node> appDataNodeList = appDataNode.getChildren();

        for (Application app : appData.getApplication())
        {
          TreeNode appNode = new TreeNode();
          appNode.setText(app.getDisplayName());
          appNode.setIconCls("application");
          appDataNodeList.add(appNode);
          
          properties = appNode.getProperties();
          properties.put("Context", app.getContext());
          properties.put("Name", app.getDisplayName());
          properties.put("Description", app.getDescription());
          properties.put("Base URI", app.getBaseUri());
          properties.put("Internal Name", app.getName());

          String context = (app.getContext() != null && app.getContext().length() > 0)
              ? app.getContext() : scope.getName();
          app.setContext(context);

          List<Node> appNodeList = appNode.getChildren();          
          
          for (Graph graph : app.getGraph())
          {
            LeafNode graphNode = new LeafNode();
            graphNode.setText(graph.getName());
            graphNode.setIconCls("graph");
            graphNode.setLeaf(true);
            appNodeList.add(graphNode);
            
            properties = graphNode.getProperties();
            properties.put("Context", context);
            properties.put("Name", graph.getName());
            properties.put("Description", graph.getDescription());
            properties.put("Base URI", app.getBaseUri());
            properties.put("Commodity", graph.getCommodity());
          }
        }
      }

      DataExchanges dataExchanges = scope.getDataExchanges();

      if (dataExchanges != null)
      {
        TreeNode exchangeDataNode = new TreeNode();
        exchangeDataNode.setText("Data Exchanges");
        exchangeDataNode.setIconCls("folder");
        scopeNodeList.add(exchangeDataNode);

        List<Node> exchangeDataNodeList = exchangeDataNode.getChildren();

        for (Commodity commodity : dataExchanges.getCommodity())
        {
          TreeNode commodityNode = new TreeNode();
          commodityNode.setText(commodity.getName());
          commodityNode.setIconCls("commodity");
          exchangeDataNodeList.add(commodityNode);
          
          properties = commodityNode.getProperties();
          properties.put("Name", commodity.getName());


          List<Node> commodityNodeList = commodityNode.getChildren();

          for (Exchange exchange : commodity.getExchange())
          {
            LeafNode exchangeNode = new LeafNode();
            exchangeNode.setIdentifier(exchange.getId());
            exchangeNode.setText(exchange.getName());
            exchangeNode.setIconCls("exchange");
            exchangeNode.setLeaf(true);
            commodityNodeList.add(exchangeNode);
            
            properties = exchangeNode.getProperties();
            properties.put("Id", exchange.getId());
            properties.put("Name", exchange.getName());
            properties.put("Description", exchange.getDescription());
          }
        }
      }
    }
    
    return tree;
  } 
  
  public Response refreshCache(String dxfrUri, String scope, String app, String graph, String timeout)
  {
    Response response = null;
    
    try
    {
      HttpClient client = new HttpClient(dxfrUri);
      HttpUtils.addHttpHeaders(settings, client);
      client.setTimeout(Integer.parseInt(timeout));
      response = client.get(Response.class, scope + "/" + app + "/" + graph + "/refresh");
    }
    catch (Exception e)
    {
      e.printStackTrace();
      response = new Response();
      response.setLevel(Level.ERROR);
      response.getMessages().getItems().add(e.getMessage());
    }
    
    return response;
  }
  
  public Response importCache(String dxfrUri, String scope, String app, String graph, String cacheUri, String timeout)
  {
    Response response = null;
    
    try
    {
      HttpClient client = new HttpClient(dxfrUri);
      HttpUtils.addHttpHeaders(settings, client);
      client.setTimeout(Integer.parseInt(timeout));
      response = client.get(Response.class, scope + "/" + app + "/" + graph + "/import?baseUri=" + cacheUri);
    }
    catch (Exception e)
    {
      e.printStackTrace();
      response = new Response();
      response.setLevel(Level.ERROR);
      response.getMessages().getItems().add(e.getMessage());
    }
    
    return response;
  }
}
