package org.iringtools.models;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Folder;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.HttpUtils;
import org.iringtools.widgets.tree.Node;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.TreeNode;

public class DirectoryModel 
{
  private Map<String, Object> session;
  
  public void setSession(Map<String, Object> session)
  {
    this.session = session;
  }
  
  public Tree getDirectoryTree(String directoryUrl) throws HttpClientException
  {
    HttpClient httpClient = new HttpClient(directoryUrl);
    HttpUtils.addOAuthHeaders(session, httpClient);
    
    Directory directory = httpClient.get(Directory.class);
    return directoryToTree(directory);
  }
  
  private void traverseDirectory(TreeNode folderNode, Folder folder, List<Node> folderNodes)
  {
  	List<Node> folderNodeList = folderNode.getChildren();
  	List<Endpoint> endpoints = folder.getEndpoints();
    List<Exchange> exchanges = folder.getExchanges();    
    
    if (folder.getAnies() == null)
    	return;
    else
    {    	
    	for (Object any : folder.getAnies())
    	{
    		Folder subFolder = (Folder)any;
    		TreeNode subFolderNode = new TreeNode();
    		subFolderNode.setText(subFolder.getName());
    		subFolderNode.setIconCls("folder");
    		folderNodeList.add(subFolderNode); 
    		traverseDirectory(subFolderNode, subFolder, folderNodes);
    	}
    }   
    
  	if (endpoints != null)
    {
    	for (Endpoint endpoint : endpoints) 
    	{
        TreeNode endPointNode = new TreeNode();
        endPointNode.setText(endpoint.getName());
        endPointNode.setIconCls("application");
        folderNodeList.add(endPointNode);
        
        HashMap<String, String> properties = endPointNode.getProperties();
        properties.put("Name", endpoint.getName());
        properties.put("Description", endpoint.getDescription());
        properties.put("Base URI", endpoint.getBaseUri());
    	}
    }
    
    if (exchanges != null)
    {
    	for (Exchange exchange : exchanges)
    	{
    		TreeNode exchangeNode = new TreeNode();
    		exchangeNode.setText(exchange.getName());
    		exchangeNode.setIconCls("exchange");
        folderNodeList.add(exchangeNode);
        
        HashMap<String, String> properties = exchangeNode.getProperties();
        properties.put("Name", exchange.getName());
        properties.put("Description", exchange.getDescription());
        properties.put("Base URI", exchange.getBaseUri());
        properties.put("Id", exchange.getId());
    	}
    }
  }
  
  private Tree directoryToTree(Directory directory)
  {
    Tree tree = new Tree();
    List<Node> folderNodes = tree.getNodes();

    for (Folder folder : directory.getFolders())
    {
      TreeNode folderNode = new TreeNode();  
      folderNode.setText(folder.getName());
      folderNode.setIconCls("folder");
      folderNodes.add(folderNode);
      traverseDirectory(folderNode, folder, folderNodes);
    }
      
    return tree;
  } 
}
