package org.iringtools.ldap.tree;

import java.util.ArrayList;
import java.util.List;

public class Tree
{
	private List<Node> nodes;
  private Node nodeAddChild;
  private Node nodeAddSibling;

  public List<Node> getNodes()
  {
    if (nodes == null)
    {
      nodes = new ArrayList<Node>();
    }
    return this.nodes;
  }

  public void setNodes(List<Node> nodes)
  {
    this.nodes = nodes;
  } 
  
  public void setNodeAddChild(Node node)
  {
  	this.nodeAddChild = node;
  }
  
  public Node getNodeAddChild()
  {
  	return this.nodeAddChild;
  } 
  
  public void setNodeAddSibling(Node node)
  {
  	this.nodeAddSibling = node;
  }
  
  public Node getNodeAddSibling()
  {
  	return this.nodeAddSibling;
  } 
  
  public Node searchNode(String ldapFullName)
  {
  	Node foundNode = null;
  	if (nodes != null)
	  	if (!nodes.isEmpty())
	  	{
	  		for (Node node : nodes)
		  	{
	  			if (foundNode != null)
	    			break;
	  			
		  		if (node.getLdapName().compareToIgnoreCase(ldapFullName) == 0)
		  			return node;
		  		else
		  		{
		  			foundNode = traverseTree(node, ldapFullName);  		
		  			if (foundNode != null)
		  				return foundNode;
		  		}
		  	}
	  	}  	
  	return foundNode;
  }
  
  private Node traverseTree(Node node, String ldapFullName)
  {
  	Node foundNode = null;
  	
  	for (Node nodeItem : node.getChildren())
  	{
  		if (foundNode != null)
  			break;
  		
  		if (nodeItem.getLdapName().compareToIgnoreCase(ldapFullName) == 0)
  			return nodeItem;
  		else
  		{
  			foundNode = traverseTree(nodeItem, ldapFullName);  
  			if (foundNode != null)
  				return foundNode;
  		}
  	}
  	
  	return foundNode;
  }
}
