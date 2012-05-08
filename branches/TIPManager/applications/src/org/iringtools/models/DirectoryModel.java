package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Endpoints;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Exchanges;
import org.iringtools.directory.Folder;
import org.iringtools.directory.NodeIconCls;
import org.iringtools.directory.SourceType;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.library.DataDictionary;
import org.iringtools.library.DataObject;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.widgets.tree.LeafNode;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

public class DirectoryModel
{
  private static final Logger logger = Logger.getLogger(DirectoryModel.class);
  
  private Map<String, Object> session;
  private String endpoint, context, baseUri;
  private String directoryServiceUri;
  private Directory directory = null;
  protected static Map<String, Object> nodeTypeMap;
  static
  {
    nodeTypeMap = new HashMap<String, Object>();
    nodeTypeMap.put("root", 0);
    nodeTypeMap.put("dataObj", 1);
  }

  protected static Map<String, NodeIconCls> nodeIconClsMap;
  static
  {
    nodeIconClsMap = new HashMap<String, NodeIconCls>();
    nodeIconClsMap.put("folder", NodeIconCls.FOLDER);
    nodeIconClsMap.put("project", NodeIconCls.PROJECT);
    nodeIconClsMap.put("scope", NodeIconCls.PROJECT);
    nodeIconClsMap.put("proj", NodeIconCls.PROJECT);
    nodeIconClsMap.put("application", NodeIconCls.APPLICATION);
    nodeIconClsMap.put("app", NodeIconCls.APPLICATION);
    nodeIconClsMap.put("resource", NodeIconCls.RESOURCE);
    nodeIconClsMap.put("resrc", NodeIconCls.RESOURCE);
    nodeIconClsMap.put("exchange", NodeIconCls.EXCHANGE);
    nodeIconClsMap.put("xch", NodeIconCls.EXCHANGE);
  }

  protected static Map<String, SourceType> sourceTypeMap;
  static
  {
    sourceTypeMap = new HashMap<String, SourceType>();
    sourceTypeMap.put("dxfr", SourceType.DXFR);
    sourceTypeMap.put("data", SourceType.DATA);
  }
  
  public DirectoryModel(Map<String, Object> session, String directoryServiceUri)
  {
    this.session = session;
    this.directoryServiceUri = directoryServiceUri;
  }

  public void postDirectoryTree(String directoryUrl)
  {
    HttpClient httpClient = new HttpClient(directoryServiceUri);
    HttpUtils.addHttpHeaders(session, httpClient);

    try
    {
      Directory dir = httpClient.post(Directory.class, "/directory", directory);
      logger.debug(JaxbUtils.toXml(dir, false));
    }
    catch (Exception ex)
    {
      logger.error(ex.getMessage());
    }
  }

  public Tree getDirectoryTree(String directoryUrl, String type, String baseUri, String context, String endpoint) throws HttpClientException
  {
    if (type != null && type.compareTo("application") == 0)
    {
    	this.baseUri = baseUri;
    	this.context = context;
    	this.endpoint = endpoint;
    	return dataObjectToTree();
    }
    else
    {
      HttpClient httpClient = new HttpClient(directoryServiceUri);
      HttpUtils.addHttpHeaders(session, httpClient);

      try
      {
        directory = httpClient.get(Directory.class, "/directory");
      }
      catch (Exception ex)
      {
        logger.error(ex);
      }
      
      return directoryToTree(directory);
    }
  }

  private Tree dataObjectToTree()
  {
    Tree tree = new Tree();
    List<Node> folderNodes = tree.getNodes();
    combineHttp(folderNodes);
    return tree;
  }

  private Tree directoryToTree(Directory directory)
  {
    Tree tree = new Tree();
    List<Node> folderNodes = tree.getNodes();

    for (Folder folder : directory.getFolderList())
    {
      TreeNode folderNode = new TreeNode();
      folderNode.setText(folder.getName());
      folderNode.setIconCls(getNodeIconCls(folder.getType()));
      folderNode.setType("folder");
      folderNodes.add(folderNode);
      traverseDirectory(folderNode, folder, folderNodes);
    }

    return tree;
  }

  private void traverseDirectory(TreeNode folderNode, Folder folder, List<Node> folderNodes)
  {
    List<Node> folderNodeList = folderNode.getChildren();
    Endpoints endpoints = folder.getEndpoints();
    Exchanges exchanges = folder.getExchanges();

    if (endpoints != null)
    {
      for (Endpoint endpoint : endpoints.getItems())
      {
        LeafNode endPointNode = new LeafNode();
        endPointNode.setText(endpoint.getName());
        endPointNode.setIconCls("application");
        endPointNode.setType("application");
        endPointNode.setLeaf(true);
        endPointNode.setNodeType("async");
        folderNodeList.add(endPointNode);

        HashMap<String, String> properties = endPointNode.getProperties();
        properties.put("Name", endpoint.getName());
        properties.put("Description", endpoint.getDescription());
        baseUri = endpoint.getBaseUrl();
        properties.put("BaseURI", baseUri);     
        context = endpoint.getContext();
        properties.put("Context", context);

      }
    }

    if (exchanges != null)
    {
      for (Exchange exchange : exchanges.getItems())
      {
        LeafNode exchangeNode = new LeafNode();
        exchangeNode.setText(exchange.getName());
        exchangeNode.setIconCls("exchange");
        exchangeNode.setType("exchange");
        exchangeNode.setLeaf(true);
        folderNodeList.add(exchangeNode);

        HashMap<String, String> properties = exchangeNode.getProperties();
        properties.put("Name", exchange.getName());
        properties.put("Description", exchange.getDescription());
        properties.put("Id", exchange.getId());
      }
    }

    if (folder.getFolders() == null)
      return;
    else
    {
      for (Folder subFolder : folder.getFolders().getItems())
      {
        TreeNode subFolderNode = new TreeNode();
        subFolderNode.setText(subFolder.getName());
        subFolderNode.setIconCls(getNodeIconCls(subFolder.getType()));
        subFolderNode.setType("folder");
        folderNodeList.add(subFolderNode);
        traverseDirectory(subFolderNode, subFolder, folderNodes);
      }
    }
  }

  private void combineHttp(List<Node> folderNodes)
  {
    graphsToTree(folderNodes, baseUri + "/dxfr/");
    dataObjectsToTree(folderNodes, baseUri + "/data/");
  }

  private void dataObjectsToTree(List<Node> dataObjNodeList, String uri)
  {
    HttpClient httpClient = new HttpClient(uri);
    HttpUtils.addHttpHeaders(session, httpClient);

    try
    {
      DataDictionary dictionary = httpClient.get(DataDictionary.class, endpoint + "/" + context + "/dictionary");
      String dataObjName = "";
      TreeNode subFolderNode = new TreeNode();
      List<Node> folderNodeList = subFolderNode.getChildren();
      dataObjNodeList.add(subFolderNode);
      subFolderNode.setText("Raw Data Objects");
      subFolderNode.setIconCls(getNodeIconCls("folder"));
      subFolderNode.setType("folder");
      subFolderNode.setExpanded(false);
      
      for (DataObject dataObject : dictionary.getDataObjects().getItems())
      {
        dataObjName = dataObject.getObjectName();
        LeafNode dataObjectNode = new LeafNode();
        dataObjectNode.setType("Data Object");
        dataObjectNode.setIconCls("resource");
        dataObjectNode.setIdentifier(context + "/" + endpoint + "/" + dataObjName);
        dataObjectNode.setText(dataObjName);
        dataObjectNode.setLeaf(true);

        HashMap<String, String> properties = dataObjectNode.getProperties();
        properties.put("Name", dataObjName);
        properties.put("Description", "Data Object");
        properties.put("ObjectNameSpace", dataObject.getObjectNamespace());
        properties.put("Context", context);
        properties.put("Data Source", "data");      

        folderNodeList.add(dataObjectNode);
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }
  }

  private void graphsToTree(List<Node> dataObjNodeList, String uri)
  {
    HttpClient httpClient = new HttpClient(uri);
    HttpUtils.addHttpHeaders(session, httpClient);

    try
    {
      Manifest manifest = httpClient.get(Manifest.class, context + "/" + endpoint + "/manifest");
      TreeNode subFolderNode = new TreeNode();
      List<Node> folderNodeList = subFolderNode.getChildren();
      dataObjNodeList.add(subFolderNode);
      subFolderNode.setText("Mapped Data Objects");
      subFolderNode.setIconCls(getNodeIconCls("folder"));
      subFolderNode.setType("folder");      
      subFolderNode.setExpanded(true);      
      String dataObjName = "";
      
      for (Graph graph : manifest.getGraphs().getItems())
      {
        dataObjName = graph.getName();
        LeafNode dataObjectNode = new LeafNode();
        dataObjectNode.setType("Data Object");
        dataObjectNode.setIconCls("resource");
        dataObjectNode.setIdentifier(context + "/" + endpoint + "/" + dataObjName);
        dataObjectNode.setText(dataObjName);
        dataObjectNode.setLeaf(true);

        HashMap<String, String> properties = dataObjectNode.getProperties();
        properties.put("Name", dataObjName);
        properties.put("Description", "Data Object");
        // properties.put("ObjectNameSpace", graph.getObjectNamespace());
        properties.put("Context", context);
        properties.put("Data Source", "dxfr");
        folderNodeList.add(dataObjectNode);
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
    }
  }
  
  

  private String getNodeIconCls(String type)
  {
    try
    {
      switch (nodeIconClsMap.get(type.toLowerCase()))
      {
	      case FOLDER:
	        return "folder";
	      case PROJECT:
	        return "project";
	      case APPLICATION:
	        return "application";
	      case RESOURCE:
	        return "resource";
	      case EXCHANGE:
	        return "exchange";
	      default:
	        return "folder";
      }
    }
    catch (Exception ex)
    {
      return "folder";
    }
  }

}
