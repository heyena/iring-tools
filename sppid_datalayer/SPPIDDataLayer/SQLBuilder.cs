using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using log4net;

namespace org.iringtools.adapter.datalayer.sppid
{
  public class SQLBuilder
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SQLBuilder));
    private Dictionary<SQLPart, string> _queryParts = new Dictionary<SQLPart,string>();
    private DBType _dbType;
    private string _tableName;
    private Dictionary<string, KeyType> _keys = new Dictionary<string, KeyType>();
    private List<DBField> _fields = new List<DBField>();
    private Dictionary<string, string> _schemaMap;
    private Dictionary<string, string> _replacements;

    public SQLBuilder(DBType dbType, XElement queryElement, Dictionary<string, string> schemaMap, 
      Dictionary<string, string> assignments, Dictionary<string, string> replacements)
      : this(dbType, queryElement, schemaMap, assignments, replacements, false) { }

    public SQLBuilder(DBType dbType, XElement queryElement, Dictionary<string, string> schemaMap, 
      Dictionary<string, string> assignments, Dictionary<string, string> replacements, bool withCreate)
    {
      _dbType = dbType;
      _schemaMap = schemaMap;
      _replacements = replacements;

      CreateSQLParts(queryElement, assignments, withCreate);
    }

    public Dictionary<string, KeyType> Keys { get { return _keys; } }
    public List<DBField> Fields { get { return _fields; } }

    public string Build(SQLCommand sqlCmd)
    {
      StringBuilder queryBuilder;

      switch (sqlCmd)
      {
        case SQLCommand.SELECT:
          queryBuilder = BuildSelect();
          break;

        case SQLCommand.CREATE:
          queryBuilder = BuildCreate();
          break;

        default:
          queryBuilder = new StringBuilder(Environment.NewLine);
          break;
      }

      if (queryBuilder == null)
      {
        return string.Empty;
      }
      
      // apply schema maps
      if (_schemaMap != null && _schemaMap.Count > 0)
      {
        foreach (var pair in _schemaMap)
        {
          queryBuilder.Replace(pair.Key + ".", pair.Value + ".");
        }
      }

      // apply text replacements
      foreach (var textReplacement in _replacements)
      {
        queryBuilder.Replace(textReplacement.Key, textReplacement.Value);
      }

      // remove first CR
      queryBuilder.Remove(0, Environment.NewLine.Length);

      return queryBuilder.ToString();
    }

    private void CreateSQLParts(XElement queryElement, Dictionary<string, string> assignments, bool withCreate)
    {
      //
      // Initialize query parts
      //
      string[] partNames = Enum.GetNames(typeof(SQLPart));

      foreach (string partName in partNames)
      {
        _queryParts[(SQLPart)(Enum.Parse(typeof(SQLPart), partName))] = null;
      }

      IEnumerable<XElement> decElts = null;
      StringBuilder selsBuilder = null;
      
      try
      {
        //
        // Get table name
        //
        _tableName = queryElement.Attribute("destination").Value;

        //
        // Get declarations
        //
        XElement decsElt = queryElement.Element("declarations");

        if (decsElt != null)
        {
          decElts = decsElt.Elements("declaration");
        }

        //
        // Get text replacements
        //
        IEnumerable<XElement> replacementElts = queryElement.Element("replacements").Elements("replacement");

        if (replacementElts != null && replacementElts.Count() > 0)
        {
          foreach (XElement replacementElt in replacementElts)
          {
            string placeHolder = replacementElt.Attribute("placeHolder").Value;
            string name = replacementElt.Attribute("name").Value;
            string value = replacementElt.Attribute("value").Value;

            if (placeHolder == string.Empty || name == string.Empty || value == string.Empty)
            {
              continue;
            }

            _replacements[placeHolder[0] + name + placeHolder[1]] = value;
          }
        }

        //
        // Collect SELECT qualifiers
        //
        XElement selsElt = queryElement.Element("selections");

        if (selsElt != null)
        {
          IEnumerable<XElement> selElts = selsElt.Elements("selection");
          selsBuilder = new StringBuilder();

          if (selElts != null && selElts.Count() > 0)
          {
            foreach (XElement selElt in selElts)
            {
              if (selsBuilder.Length > 0)
              {
                selsBuilder.Append(" ");
              }

              selsBuilder.Append(selElt.Value);
            }
          }
        }

        //
        // Collect keys information
        //
        XElement keysElt = queryElement.Element("keys");

        if (keysElt != null)
        {
          string keyDelimiter = keysElt.Attribute("delimiter").Value;

          foreach (XElement keyElt in keysElt.Elements("key"))
          {
            string name = keyElt.Attribute("name").Value;

            KeyType type = (keyElt.Attribute("type") == null || keyElt.Attribute("type").Value.ToLower() == "assigned") 
                ? KeyType.ASSIGNED : KeyType.AUTO;

            _keys[name] = type;
          }
        }

        //
        // Build FROM part
        //
        XElement sourcesElt = queryElement.Element("sources");

        if (sourcesElt != null)
        {
          IEnumerable<XElement> sourceElts = sourcesElt.Elements("source");

          if (sourceElts != null && sourceElts.Count() > 0)
          {
            StringBuilder fromBuilder = new StringBuilder();
            bool firstSource = true;

            foreach (XElement sourceElt in sourceElts)
            {
              string alias = sourceElt.Attribute("alias").Value;

              if (!firstSource)
              {
                fromBuilder.Append(" " + sourceElt.Attribute("joinType").Value + " join ");
              }

              string qualSourceName = string.Format("{0}.{1}", sourceElt.Attribute("schema").Value, sourceElt.Attribute("name").Value);
              fromBuilder.Append(qualSourceName);

              if (!string.IsNullOrEmpty(alias))
              {
                fromBuilder.Append(" " + alias);
              }

              if (!firstSource && sourceElt.Elements("relation").Count<XElement>() > 0)
              {
                fromBuilder.Append(" on ");

                foreach (XElement relation in sourceElt.Elements("relation"))
                {
                  string conjunction = relation.Attribute("conjunction").Value;

                  if (!string.IsNullOrEmpty(conjunction))
                  {
                    fromBuilder.Append(" " + conjunction + " ");
                  }

                  fromBuilder.Append(relation.Attribute("leftSource").Value + "." + relation.Attribute("leftField").Value + relation.Attribute("operator").Value);

                  if (relation.Attribute("joinToText").Value.ToLower() == "true")
                  {
                    fromBuilder.Append("'" + relation.Attribute("rightField").Value + "'");
                  }
                  else
                  {
                    if (string.IsNullOrEmpty(relation.Attribute("rightSource").Value))
                    {
                      fromBuilder.Append(relation.Attribute("rightField").Value);
                    }
                    else
                    {
                      fromBuilder.Append(relation.Attribute("rightSource").Value + "." + relation.Attribute("rightField").Value);
                    }
                  }
                }
              }

              firstSource = false;
            }

            Add(SQLPart.FROM, fromBuilder.ToString());
          }
        }

        //
        // Build CREATE part
        //
        XElement fieldsElt = queryElement.Element("fields");

        if (fieldsElt != null)
        {
          IEnumerable<XElement> fieldElts = fieldsElt.Elements("field");

          if (fieldElts != null && fieldElts.Count() > 0)
          {
            StringBuilder selectBuilder = new StringBuilder();
            StringBuilder createBuilder = new StringBuilder();
            bool hasKey = false;

            foreach (XElement fieldElt in fieldElts)
            {
              string source = fieldElt.Attribute("source").Value;
              string colName = fieldElt.Attribute("name").Value;
              string alias = fieldElt.Attribute("alias").Value;
              string dataType = fieldElt.Attribute("datatype").Value.ToLower();
              bool nullable = true;

              if (string.IsNullOrEmpty(alias))
              {
                alias = colName;
              }

              if (_keys.ContainsKey(alias))
              {
                hasKey = true;
                nullable = false;
              }

              if (!XElement.ReferenceEquals(fieldElt, fieldElts.First<XElement>()))
              {
                selectBuilder.Append(",");
                createBuilder.Append(",");
              }

              string selectItem = string.Empty;

              if (fieldElt.Attribute("expression") != null && fieldElt.Attribute("expression").Value != string.Empty)
              {
                selectItem = fieldElt.Attribute("expression").Value;
              }
              else
              {
                selectItem = source + "." + colName;
              }
              
              selectBuilder.Append(selectItem + " " + alias);

              if (string.IsNullOrEmpty(dataType))
              {
                dataType = "nvarchar(45)";
              }

              // make all fields that are not keys nullable to be safe
              createBuilder.Append(alias + " " + dataType + ((nullable) ? " NULL" : " NOT NULL"));

              // add to field list
              int dataLength = Int16.MaxValue;

              if (dataType.Contains("char("))
              {
                if (!dataType.Contains("max"))
                {
                  int startIndex = dataType.IndexOf("(") + 1;
                  int endIndex = dataType.IndexOf(")");

                  dataLength = int.Parse(dataType.Substring(startIndex, endIndex - startIndex));
                }
              }
              else if (dataType == "date" || dataType == "datetime")
              {
                dataLength = 24;
              }

              _fields.Add(new DBField(alias, dataType, dataLength, nullable));
            }

            if (selsBuilder.Length > 0)
            {
              Add(SQLPart.SELECT, selsBuilder.ToString() + " " + selectBuilder.ToString());
            }
            else
            {
              Add(SQLPart.SELECT, selectBuilder.ToString());
            }

            if (withCreate)
            {
              // if key name is not in the field list, then create a single key with that name and set it as autoincrement
              if (!hasKey)
              {
                string keyName = _keys.First().Key;

                if (_dbType == DBType.ORACLE)
                {
                  createBuilder.Insert(0, keyName + " NUMBER,");

                  string sequenceName = _tableName + Constants.SEQUENCE_SUFFIX;
                  Add(SQLPart.SEQUENCE, sequenceName + " START WITH 1 INCREMENT BY 1");

                  string triggerName = sequenceName + Constants.TRIGGER_SUFFIX;
                  string trigger = string.Format(Constants.ORACLE_SEQUENCE_TRIGGER_TEMPLATE, triggerName, _tableName, sequenceName);
                  Add(SQLPart.TRIGGER, trigger);
                  
                  _fields.Add(new DBField(keyName, "NUMBER", 32, false));
                }
                else if (_dbType == DBType.SQLServer)
                {
                  createBuilder.Insert(0, keyName + " int IDENTITY(1,1),");

                  _fields.Add(new DBField(keyName, "int", 32, false));
                }
              }

              Add(SQLPart.CREATE, createBuilder.ToString());
            }
          }
        }

        //
        // Build WHERE part
        //
        XElement filtersElt = queryElement.Element("filters");

        if (filtersElt != null)
        {
          IEnumerable<XElement> filterElts = filtersElt.Elements("filter");

          if (filterElts != null && filterElts.Count() > 0)
          {
            StringBuilder filtersBuilder = new StringBuilder();

            //TODO: validate filtered column 
            foreach (XElement filterElt in filterElts)
            {
              string filterValue = filterElt.Attribute("value").Value;

              // filter value is variable 
              if (filterValue.StartsWith("@"))
              {
                if (decElts != null)
                {
                  // look for filter value in the declarations
                  foreach (XElement decElt in decElts)
                  {
                    if (decElt.Attribute("name").Value.ToLower() == filterValue.ToLower())
                    {
                      string decValue = decElt.Attribute("value").Value;
                      string decDataType = decElt.Attribute("datatype").Value;

                      // if declaration value is empty, look up project assignments
                      if (string.IsNullOrEmpty(decValue))
                      {
                        if (assignments.ContainsKey(filterValue))
                          filterValue = assignments[filterValue];
                      }
                      else
                      {
                        filterValue = decValue;
                      }

                      if (!filterValue.StartsWith("@") && !Utility.IsNumeric(decDataType))
                      {
                        filterValue = "'" + filterValue + "'";
                      }

                      break;
                    }
                  }
                }

                // filter value is a variable but no substitution occurred, thus invalid
                if (filterValue.StartsWith("@"))
                {
                  _logger.Warn("Filter variable value [" + filterValue + "] is empty or not declared.");
                  continue;
                }
              }
              else
              {
                //TODO: wrap value with quotes for non numeric types only
                filterValue = "'" + filterValue + "'";
              }

              if (filtersBuilder.Length > 0)
              {
                filtersBuilder.Append(" " + filterElt.Attribute("conjunction").Value + " ");
              }

              filtersBuilder.Append(new string('(', int.Parse(filterElt.Attribute("preParenCount").Value)));
              filtersBuilder.Append(filterElt.Attribute("source").Value + "." + filterElt.Attribute("field").Value);
              filtersBuilder.Append(filterElt.Attribute("operator").Value + filterValue);
              filtersBuilder.Append(new string(')', int.Parse(filterElt.Attribute("postParenCount").Value)));
            }

            if (filtersBuilder.Length > 0)
            {
              Add(SQLPart.WHERE, filtersBuilder.ToString());
            }
          }
        }

        //
        // Build SORT part
        //
        XElement sortsElt = queryElement.Element("sorts");

        if (sortsElt != null)
        {
          IEnumerable<XElement> sortElts = sortsElt.Elements("sort");

          if (sortElts != null && sortElts.Count() > 0)
          {
            StringBuilder sortsBuilder = new StringBuilder();

            foreach (XElement sortElt in sortElts)
            {
              if (!XElement.ReferenceEquals(sortElt, sortElts.First<XElement>()))
              {
                sortsBuilder.Append(",");
              }

              sortsBuilder.Append(" " + sortElt.Attribute("source").Value + "." + sortElt.Attribute("field").Value + " " + sortElt.Attribute("direction").Value + " ");
            }

            Add(SQLPart.SORT, sortsBuilder.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        string error = "Error creating query parts: " + ex.Message;
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private StringBuilder BuildSelect()
    {
      StringBuilder queryBuilder = new StringBuilder();

      queryBuilder.Append(_queryParts[SQLPart.DECLARE] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.SET] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.SELECT] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.INTO] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.FROM] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.WHERE] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.GROUPBY] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.HAVING] ?? string.Empty);
      queryBuilder.Append(_queryParts[SQLPart.ORDERBY] ?? string.Empty);

      return queryBuilder;
    }

    //TODO: define/generate primary key
    private StringBuilder BuildCreate()
    {
      StringBuilder queryBuilder = new StringBuilder();

      queryBuilder.Append(_queryParts[SQLPart.CREATE]);

      if (_dbType == DBType.ORACLE)
      {
        queryBuilder.Append(_queryParts[SQLPart.SEQUENCE]);
        queryBuilder.Append(_queryParts[SQLPart.TRIGGER]);
      }

      return queryBuilder;
    }

    private void Add(SQLPart part, string clause)
    {
      switch (part)
      {
        case SQLPart.DECLARE:
          _queryParts[part] = Environment.NewLine + "DECLARE " + clause;
          break;

        case SQLPart.SET:
          _queryParts[part] = clause;
          break;

        case SQLPart.SELECT:
          _queryParts[part] = Environment.NewLine + "SELECT " + clause;
          break;

        case SQLPart.INTO:
          _queryParts[part] = Environment.NewLine + "INTO " + clause;
          break;

        case SQLPart.FROM:
          _queryParts[part] = Environment.NewLine + "FROM " + clause;
          break;

        case SQLPart.WHERE:
          _queryParts[part] = Environment.NewLine + "WHERE " + clause;
          break;

        case SQLPart.GROUPBY:
          _queryParts[part] = Environment.NewLine + "GROUP BY " + clause;
          break;

        case SQLPart.HAVING:
          _queryParts[part] = Environment.NewLine + "HAVING " + clause;
          break;

        case SQLPart.ORDERBY:
          _queryParts[part] = Environment.NewLine + "ORDER BY " + clause;
          break;

        case SQLPart.SEQUENCE:
          _queryParts[part] = Environment.NewLine + "CREATE SEQUENCE " + clause;
          break;

        case SQLPart.TRIGGER:
          _queryParts[part] = Environment.NewLine + "CREATE OR REPLACE TRIGGER " + clause;
          break;

        case SQLPart.DELETE:
          _queryParts[part] = Environment.NewLine + "DELETE TABLE " + clause;
          break;

        case SQLPart.CREATE:
          _queryParts[part] = Environment.NewLine + "CREATE TABLE " + _tableName + " (" + clause + ")";
          break;

        case SQLPart.UPDATE:
          _queryParts[part] = Environment.NewLine + "UPDATE TABLE " + clause;
          break;
      }
    }
  }

  public class DBField
  {
    public DBField() {}

    public DBField(string name, string dataType, int dataLength, bool nullable)
    {
      Name = name;
      DataType = dataType;
      DataLength = dataLength;
      Nullable = nullable;
    }

    public string Name { get; set; }
    public string DataType { get; set; }
    public int DataLength { get; set; }
    public bool Nullable { get; set; }
  }

  public enum DBType
  {
    SQLServer,
    ORACLE
  }

  public enum KeyType
  {
    AUTO,
    ASSIGNED
  }

  public enum SQLCommand  
  {
    SELECT,
    CREATE,
    DELETE,
    UPDATE,
    TRUNCATE,
    PURGE,
    DROP
  }

  public enum SQLPart
  {
    DECLARE,
    SET,
    SELECT,
    INTO,
    FROM,
    WHERE,
    SORT,
    GROUPBY,
    HAVING, 
    ORDERBY,
    SEQUENCE,
    TRIGGER,
    CREATE,
    UPDATE,
    DELETE
  }

  public enum SQLJoin
  {
    INNER,
    LEFT,
    RIGHT,
    FULL,
    SELF
  }
}
