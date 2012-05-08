package org.iringtools.ldap.tree;

import java.util.ArrayList;
import java.util.List;

public class Node
{
  private String name;
  private String type;
  private String description;  
  private String ldapName;
  private String id;
  private String context;
  private String baseUrl;
  private String assembly;
  private List<Node> children;
  private String securityRole;
  private Node parentNode;
  
  public Node(String name)
  {
  	this.name = name;
  }
  
  public Node()
  {
  	children = new ArrayList<Node>();
  } 

  public List<Node> getChildren()
  {
    if (children == null)
    {
      children = new ArrayList<Node>();
    }
    return this.children;
  }

  public void setChildren(List<Node> children)
  {
    this.children = children;
  }

  public String getName()
  {
    return name;
  } 

  public void setSecurityRole(String value)
  {
    this.securityRole = value;
  }
  
  public String getSecurityRole()
  {
    return securityRole;
  }

  public void setLdapName(String value)
  {
    this.ldapName = value;
  }
  
  public String getLdapName()
  {
    return ldapName;
  }
  
  public void setName(String value)
  {
    this.name = value;
  } 
  
  public String geDescription()
  {
    return description;
  }

  public void setDescription(String value)
  {
    this.description = value;
  }

  public String getType() {
  	return type;
  }

  public void setType(String type) {
  	this.type = type;
	} 
  
  public String getId()
  {
    return id;
  }

  public void setId(String value)
  {
    this.id = value;
  }  

  public void setContext(String value)
  {
    this.context = value;
  }  
 
  public String getContext()
  {
    return context;
  }
  
  public void setBaseUrl(String value)
  {
    this.baseUrl = value;
  }  
 
  public String getBaseUrl()
  {
    return baseUrl;
  }
  
  public void setAssembly(String value)
  {
    this.assembly = value;
  }  
 
  public String getAssembly()
  {
    return assembly;
  }
  
  public void setParentNode(Node node)
  {
    this.parentNode = node;
  }
  
  public Node getParentNode()
  {
    return parentNode;
  }

}
