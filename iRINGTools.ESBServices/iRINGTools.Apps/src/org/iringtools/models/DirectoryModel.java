package org.iringtools.models;



import java.util.List;

import org.iringtools.common.response.Response;
import org.iringtools.directory.*;

import org.iringtools.ui.widgets.tree.LeafNode;
import org.iringtools.ui.widgets.tree.Node;
import org.iringtools.ui.widgets.tree.Property;
import org.iringtools.ui.widgets.tree.Tree;
import org.iringtools.ui.widgets.tree.TreeNode;

import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;


import com.opensymphony.xwork2.ActionContext;

public class DirectoryModel {

	private Directory directory = null;
	private String URI;

	public DirectoryModel() {
		try {
			URI = ActionContext.getContext().getApplication()
					.get("DirectoryServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in DirectoryServiceUri :" + e);
		}
		directory = null;
	}

	public void populate() {

		try {
			WebClient webclient = new WebClient(URI);
			directory = webclient.get(Directory.class, "/directory");
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void save() {

		try {
			WebClient webclient = new WebClient(URI);
			Response response = webclient.post(Response.class, "/directory",
					directory);

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

	public List<Scope> getScopes() {
		return directory.getScopes();
	}

	


	public Directory getDirectory() {
		return directory;
	}



	public Tree toTree() {
		
		Tree tree = new Tree();
	    List<Node> scopeNodes = tree.getTreeNodes();

	    int ci=1, si=1, ei=1, adi=1;
	    
	    for (Scope scope : directory.getScopes())
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
	        appDataNode.setText("application data");
	        appDataNode.setIconCls("folder");
	        appDataNode.setId("application data" + adi);
	        adi++;
	        scopeNodeList.add(appDataNode);

	        List<Node> appDataNodeList = appDataNode.getChildren();

	        for (Application app : appData.getApplications())
	        {
	          TreeNode appNode = new TreeNode();
	          appNode.setText(app.getName());
	          appNode.setId(app.getName()+ app.getId());
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
	        exchangeDataNode.setText("data exchanges");
	        exchangeDataNode.setId("data exchanges" + ei);
	        ei++;
	        exchangeDataNode.setIconCls("folder");
	        scopeNodeList.add(exchangeDataNode);

	        
	        List<Node> exchangeDataNodeList = exchangeDataNode.getChildren();

	        for (Commodity commodity : exchangeData.getCommodities())
	        {	        	
	          TreeNode commodityNode = new TreeNode();
	          commodityNode.setText(commodity.getName());
	          commodityNode.setId("commodity"+ci);
	          ci++;
	          commodityNode.setIconCls("commodity");
	          exchangeDataNodeList.add(commodityNode);

	          List<Node> commodityNodeList = commodityNode.getChildren();

	          
	          for (Exchange exchange : commodity.getExchanges().getItems())
	          {
	            LeafNode exchangeNode = new LeafNode();
	            exchangeNode.setId(exchange.getName()+ exchange.getId());
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
	
	public void readTree(Tree tree) {
		//TODO: Read the tree!
	}
}
