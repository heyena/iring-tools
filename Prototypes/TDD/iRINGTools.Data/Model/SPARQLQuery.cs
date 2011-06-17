using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class SPARQLQuery
  {
    public string Key { get; set; }
    public string Query { get; set; }
    public LazyList<SPARQLQueryBinding> Bindings { get; set; }
  }

  public class SPARQLQueryBinding
  {
    public string Name { get; set; }
    public SPARQLBindingType Type { get; set; }
  }
  
  public class SPARQLResults
  {
    public SPARQLHead Head { get; set; }
    public LazyList<SPARQLResult> Results { get; set; }

    public SPARQLResults()
    {
    }
  }

  public class SPARQLHead
  {
    public LazyList<SPARQLVariable> Variables { get; set; }

    public SPARQLHead()
    {
    }
  }

  public class SPARQLVariable
  {
    public string Name { get; set; }
  }

  public class SPARQLResult
  {
    public LazyList<SPARQLBinding> Bindings { get; set; }

    public SPARQLResult()
    {
    }
  }

  public class SPARQLBinding
  {
    public string Name { get; set; }
    public string Bnode { get; set; }
    public string Uri { get; set; }
    public SPARQLLiteral Literal { get; set; }
  }

  public enum SPARQLBindingType
  {
    Uri,
    Literal,
  }

  public class SPARQLLiteral
  {
    public string Lang { get; set; }
    public string Datatype { get; set; }
    public string Value { get; set; }
  }
}
