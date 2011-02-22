using System.Collections.Generic;
using System.Xml.Serialization;


namespace org.iringtools.adapter.datalayer 
{
  [XmlTypeAttribute()]
  [XmlRootAttribute(IsNullable = false)]
  public class MSSQLConfiguration : List<SqlObject>
  {
  }

  [XmlTypeAttribute()]
  [XmlRootAttribute(IsNullable = false)]
  public class SqlObject
  {

    public SqlObject()
    {
      SecondaryObjects = new List<SecondaryObject>();
    }

    public string ObjectName { get; set; }

    public string ObjectTypeName { get; set; }

    public string ConnectionString { get; set; }

    public KeyReferenceObject KeyReferenceObject { get; set; }

    public string IdentifierProperty { get; set; }

    public string IdentifierMap { get; set; }

    public string IdentifierMapSeperator { get; set; }

    public IdType IdentifierType { get; set; }

    public string KeyProperties { get; set; }

    public string ListSqlWhere { get; set; }

    public string SelectSqlJoin { get; set; }

    public string StatusProperty { get; set; }

    public string CreateStatus { get; set; }

    public string DeleteQuery { get; set; }

    public string MinimumProperties { get; set; }

    [XmlArrayItemAttribute("SecondaryObject")]
    public List<SecondaryObject> SecondaryObjects { get; set; }

  }

  [XmlTypeAttribute()]
  public class KeyReferenceObject
  {

    public string ReferenceObjectName { get; set; }

    public string ReturnProperty { get; set; }

    public string WhereClause { get; set; }

  }
  [XmlTypeAttribute()]
  public class SecondaryObject
  {

    public string CreateStatus { get; set; }

    public string StatusProperty { get; set; }

    public string DeleteQuery { get; set; }

    public string IdentifierProperty { get; set; }

    public string IdentifierMap { get; set; }

    public string IdentifierMapSeperator { get; set; }

    public string KeyProperty { get; set; }

    public string MinimumProperties { get; set; }

    public string ObjectName { get; set; }

    public string SelectQuery { get; set; }

    public string WhereClause { get; set; }

    [XmlArrayItemAttribute("SecondaryObject")]
    public List<SecondaryObject> SecondaryObjects { get; set; }
  }

  public enum IdType
  {
    Local,
    Foreign
  }
}
