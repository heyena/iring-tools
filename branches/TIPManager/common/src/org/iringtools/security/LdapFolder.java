package org.iringtools.security;

import java.util.Hashtable;

import javax.naming.Context;
import javax.naming.Name;
import javax.naming.NameNotFoundException;
import javax.naming.NameParser;
import javax.naming.NamingEnumeration;
import javax.naming.NamingException;
import javax.naming.OperationNotSupportedException;
import javax.naming.directory.Attribute;
import javax.naming.directory.Attributes;
import javax.naming.directory.BasicAttribute;
import javax.naming.directory.BasicAttributes;
import javax.naming.directory.DirContext;
import javax.naming.directory.ModificationItem;
import javax.naming.directory.SearchControls;

public class LdapFolder implements DirContext
{
  Attributes myAttrs = new BasicAttributes(true);
  Attribute oc = new BasicAttribute("objectClass");
  String name;
  String type;
  String context;
  String description;
  String member;

  public LdapFolder(String name, String description, String type, String context, String member)
  {
    this.name = name;
    this.type = type;
    this.context = context;
    this.description = description;
    this.member = member;
    oc.add("folder");
    myAttrs.put(oc);
    myAttrs.put("member", member);
    myAttrs.put("name", name);
    myAttrs.put("type", type);
    myAttrs.put("context", context);
    myAttrs.put("description", description);
  }  
  
  public String getName()
  {
    return name;
  }

  public String getDescription()
  {
    return description;
  }

  public String getType()
  {
    return type;
  }

  public String getContext()
  {
    return context;
  }

  public void setType(String type)
  {
    this.type = type;
  }

  public void setDescription(String description)
  {
    this.description = description;
  }

  public void setName(String name)
  {
    this.name = name;
  }

  public void setContext(String context)
  {
    this.context = context;
  }

  public Attributes getAttributes(String name) throws NamingException
  {
    if (!name.equals(""))
    {
      throw new NameNotFoundException();
    }
    return (Attributes) myAttrs.clone();
  }

  public Attributes getAttributes(Name name) throws NamingException
  {
    return getAttributes(name.toString());
  }

  public Attributes getAttributes(String name, String[] ids) throws NamingException
  {
    if (!name.equals(""))
    {
      throw new NameNotFoundException();
    }

    Attributes answer = new BasicAttributes(true);
    Attribute target;
    for (int i = 0; i < ids.length; i++)
    {
      target = myAttrs.get(ids[i]);
      if (target != null)
      {
        answer.put(target);
      }
    }
    return answer;
  }

  public Attributes getAttributes(Name name, String[] ids) throws NamingException
  {
    return getAttributes(name.toString(), ids);
  }

  public Object lookup(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Object lookup(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void bind(Name name, Object obj) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void bind(String name, Object obj) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rebind(Name name, Object obj) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rebind(String name, Object obj) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void unbind(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void unbind(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rename(Name oldName, Name newName) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rename(String oldName, String newName) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration list(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration list(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration listBindings(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration listBindings(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void destroySubcontext(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void destroySubcontext(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Context createSubcontext(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Context createSubcontext(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Object lookupLink(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Object lookupLink(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NameParser getNameParser(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NameParser getNameParser(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public String composeName(String name, String prefix) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Name composeName(Name name, Name prefix) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Object addToEnvironment(String propName, Object propVal) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Object removeFromEnvironment(String propName) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public Hashtable getEnvironment() throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void close() throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  // -- DirContext
  public void modifyAttributes(Name name, int mod_op, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void modifyAttributes(String name, int mod_op, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void modifyAttributes(Name name, ModificationItem[] mods) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void modifyAttributes(String name, ModificationItem[] mods) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void bind(Name name, Object obj, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void bind(String name, Object obj, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rebind(Name name, Object obj, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public void rebind(String name, Object obj, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext createSubcontext(Name name, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext createSubcontext(String name, Attributes attrs) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext getSchema(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext getSchema(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext getSchemaClassDefinition(Name name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public DirContext getSchemaClassDefinition(String name) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(Name name, Attributes matchingAttributes, String[] attributesToReturn)
      throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(String name, Attributes matchingAttributes, String[] attributesToReturn)
      throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(Name name, Attributes matchingAttributes) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(String name, Attributes matchingAttributes) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(Name name, String filter, SearchControls cons) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(String name, String filter, SearchControls cons) throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(Name name, String filterExpr, Object[] filterArgs, SearchControls cons)
      throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public NamingEnumeration search(String name, String filterExpr, Object[] filterArgs, SearchControls cons)
      throws NamingException
  {
    throw new OperationNotSupportedException();
  }

  public String getNameInNamespace() throws NamingException
  {
    throw new OperationNotSupportedException();
  }
}
