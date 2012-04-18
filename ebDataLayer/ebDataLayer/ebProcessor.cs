using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eB.Data;
using org.iringtools.library;
using org.iringtools.adaper.datalayer.eb.config;
using System.Data;
using eB.Common.Enum;

namespace org.iringtools.adaper.datalayer.eb
{
  public class ebProcessor
  {
    private Session _session;
    private IDataObject _dataObject;
    private List<Map> _mappings;
    private Rules _rules;

    public ebProcessor(Session session, List<Map> mappings, Rules rules)
    {
      _session = session;
      _mappings = mappings;
      _rules = rules;
    }

    public Status ProcessTag(IDataObject dataObject, int id, string key)
    {
      _dataObject = dataObject;
      Status status = new Status();

      try
      {
        StringBuilder messages = new StringBuilder();
        Tag tag = new Tag(_session, id);

        tag.Retrieve("Header;Attributes");
        tag.Code = key;

        if (!SetName(ref tag, key))
        {
          messages.Append("Failed to set Name\n");
        }

        if (!SetTitle(ref tag, key))
        {
          messages.Append("Failed to set Title\n");
        }

        messages.Append(SetAttributes(ref tag));
        SetRelationships(tag);
        tag.Save();
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    public Status ProcessDocument(IDataObject dataObject, int id, string key)
    {
      _dataObject = dataObject;
      Status status = new Status();

      try
      {
        Document doc = new Document(_session, id);
        doc.Retrieve("Header;Attributes");
        
        status.Messages.Add(SetAttributes(ref doc));
        status.Messages.Add(SetRelationships(doc));
        status.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    protected string SetAttributes(ref Tag tag)
    {
      StringBuilder messages = new StringBuilder();

      try
      {
        var attrs = from m in _mappings
                    where m.Type == PropertyType.Attribute
                    select m;

        foreach (Map m in attrs)
        {
          try
          {
            object value = _dataObject.GetPropertyValue(m.Property);

            if (value != null)
            {
              eB.Data.Attribute ebAtr = tag.Attributes.Where(atr => ((atr.Name == m.Property))).Select(atr => atr).FirstOrDefault();

              if ((ebAtr.Value == null) || (ebAtr.Value.ToString() != value.ToString()))
              {
                _session.Writer.ChgCharData(tag.Id, ebAtr.AttributeDef.Id, value);
              }
            }
          }
          catch (Exception ex)
          {
            messages.Append(String.Format("Attribute {0} update failed due to error {1}", m.Property, ex.Message));
          }
        }
      }
      catch (Exception ex) { return ex.Message; }

      return messages.ToString();
    }

    protected string SetAttributes(ref Document doc)
    {
      StringBuilder messages = new StringBuilder();

      try
      {
        var attrs = from k in _mappings
                    where k.Type == PropertyType.Attribute
                    select k;

        foreach (Map m in attrs)
        {
          try
          {
            object value = _dataObject.GetPropertyValue(m.Property);

            if (value != null)
            {
              eB.Data.Attribute ebAtr = doc.Attributes.Where(atr => ((atr.Name == m.Property))).Select(atr => atr).FirstOrDefault();

              if (ebAtr.Value.ToString() != (string)value)
              {
                _session.Writer.ChgCharData(doc.Id, ebAtr.AttributeDef.Id, value);
              }
            }
          }
          catch (Exception ex)
          {
            messages.Append(String.Format("Attribute {0} update failed due to error {1}", m.Property, ex.Message));
          }
        }
      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    public bool RequiredParametersPresent(Rule rule)
    {
      foreach (var p in rule.Parameters)
      {
        if (_dataObject.GetPropertyValue(p.Value) == null)
        {
          return false;
        }
      }

      return true;
    }

    private Rules GetRules(List<RuleRef> ruleRefs)
    {
      Rules rules = new Rules();

      ruleRefs.Sort(new RuleRefComparer());

      foreach (RuleRef ruleRef in ruleRefs)
      {
        Rule rule = _rules.Find(x => x.Id == ruleRef.Value);
        if (rule != null) rules.Add(rule);
      }

      return rules;
    }

    private string GetValue(Parameter param, string p)
    {
      try
      {
        return p.Split(new String[] {param.Seperator}, StringSplitOptions.RemoveEmptyEntries)[param.Position];
      }

      catch { return p; }
    }

    private String ParseEQL(Rule rule)
    {
      if ((rule.Parameters == null) || (rule.Parameters.Count() == 0))
      {
        return rule.Eql;
      }

      string[] parameters = new string[rule.Parameters.Count];
      for (int i = 0; i < rule.Parameters.Count; i++)
      {
        parameters[i] = GetValue(rule.Parameters[i], (string) _dataObject.GetPropertyValue(rule.Parameters[i].Value));
      }

      return String.Format(rule.Eql, parameters);
    }

    private bool SelfchecksPassed(Rule rule)
    {
      foreach (var c in rule.SelfChecks)
      {
        if (!Passed(c))
        {
          return false;
        }
      }

      return true;
    }

    private bool Passed(SelfCheck c)
    {
      try
      {
        string value = (string)_dataObject.GetPropertyValue(c.Column);

        switch (c.Operator.ToLower())
        {
          case "=": return value == c.Value;
          case "<>": return value != c.Value;
          case "contains": return value.Contains(c.Value);
          case "startswith": return value.StartsWith(c.Value);
          case "endswith": return value.EndsWith(c.Value);
          default: return false;
        }
      }

      catch { return false; }
    }

    protected string SetRelationships(Tag o)
    {
      StringBuilder messages = new StringBuilder();
      EqlClient eqlClient = new EqlClient(_session);

      try
      {
        foreach (Map m in _mappings.Where(m => m.RuleRefs.Count > 0).Select(m => m))
        {
          List<Rule> rules = GetRules(m.RuleRefs);

          foreach (Rule r in rules)
          {
            if (RequiredParametersPresent(r))
            {
              if (SelfchecksPassed(r))
              {
                int reltemplateId = eqlClient.GetTemplateId(r.RelationshipTemplate);
                if (reltemplateId > 0)
                {
                  DataTable relatedObjects = eqlClient.GetObjectIds(ParseEQL(r));
                  foreach (DataRow dr in relatedObjects.Rows)
                  {
                    int relatedObjectId = int.Parse(dr[0].ToString());
                    if (relatedObjectId > 0)
                    {
                      try
                      {
                        bool add = false;
                        int existingrelId = eqlClient.GetExistingRelationship(reltemplateId, o.Id, relatedObjectId, ref add);

                        if (existingrelId > 0)
                        {
                          _session.Writer.DelRelationship(existingrelId);
                        }

                        if (add)
                        {
                          _session.Writer.CreateRelFromTemplate(reltemplateId, o.Id, (int)ObjectType.Tag, relatedObjectId, r.RelatedObjectType);
                        }
                      }
                      catch (Exception ex)
                      {
                        messages.Append(String.Format("Relationship failed due to error {0}.", ex.Message));
                      }
                    }
                  }
                }
                else
                {
                  messages.Append(String.Format("Relationship template {0} is not defined.", r.RelationshipTemplate));
                }
              }
            }
          }
        }
      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    protected string SetRelationships(Document o)
    {
      StringBuilder messages = new StringBuilder();
      EqlClient eqlClient = new EqlClient(_session);

      try
      {
        foreach (Map m in _mappings.Where(m => m.RuleRefs.Count > 0).Select(m => m))
        {
          List<Rule> rules = GetRules(m.RuleRefs);

          foreach (Rule r in rules)
          {
            if (RequiredParametersPresent(r))
            {
              if (SelfchecksPassed(r))
              {
                int reltemplateId = eqlClient.GetTemplateId(r.RelationshipTemplate);
                if (reltemplateId > 0)
                {
                  DataTable relatedObjects = eqlClient.GetObjectIds(ParseEQL(r));
                  foreach (DataRow dr in relatedObjects.Rows)
                  {
                    int relatedObjectId = int.Parse(dr[0].ToString());
                    if (relatedObjectId > 0)
                    {
                      try
                      {
                        bool add = false;
                        int existingrelId = eqlClient.GetExistingRelationship(reltemplateId, o.Id, relatedObjectId, ref add);
                        if (existingrelId > 0)
                        {
                          _session.Writer.DelRelationship(existingrelId);
                        }
                        if (add)
                        {
                          _session.Writer.CreateRelFromTemplate(reltemplateId, o.Id, (int)ObjectType.Document, relatedObjectId, (int)r.RelatedObjectType);
                        }
                      }
                      catch (Exception ex)
                      {
                        messages.Append(String.Format("Relationship failed due to error {0}.", ex.Message));
                      }
                    }
                  }
                }
                else
                {
                  messages.Append(String.Format("Relationship template {0} is not defined.", r.RelationshipTemplate));
                }
              }
            }
          }
        }
      }
      catch (Exception ex) { return ex.Message; }

      return messages.ToString();
    }

    protected bool SetName(ref Tag tag, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == PropertyType.Name
                     select k).FirstOrDefault();

        if (n != null)
        {
          string name = (string)_dataObject.GetPropertyValue(n.Property);

          if (tag.Name != name)
          {
            tag.Name = name;
          }
        }
        else
        {
          tag.Name = key;
        }
      }
      catch { return false; }
      return true;
    }

    protected bool SetName(ref Document doc, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == PropertyType.Name
                     select k).FirstOrDefault();
        if (n != null)
        {
          string name = (string)_dataObject.GetPropertyValue(n.Property);

          if (doc.Name != name)
          {
            doc.Name = name;
          }
        }
        else
        {
          doc.Name = key;
        }
      }
      catch { return false; }
      return true;
    }

    protected bool SetTitle(ref Tag tag, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == PropertyType.Title
                     select k).FirstOrDefault();
        if (n != null)
        {
          string name = (string)_dataObject.GetPropertyValue(n.Property);
          if (tag.Name != name)
          {
            tag.Description = name;
          }
        }
        else
        {
          tag.Description = key;
        }
      }
      catch { return false; }
      return true;
    }
  }
}
