using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class RefDataEntities
  {
    public RefDataEntities()
    {
      Entities = new SortedList<string, Entity>();
    }

    [DataMember]
    public SortedList<string, Entity> Entities { get; set; }

    [DataMember]
    public int Total { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/library", Name = "entity")]
  public class Entity
  {
    [DataMember(Name = "uri")]
    public string Uri { get; set; }

    [DataMember(Name = "label")]
    public string Label { get; set; }

    [DataMember(Name = "repository")]
    public string Repository { get; set; }

    public static IComparer<Entity> sortAscending()
    {
      return (IComparer<Entity>)new sortAscendingHelper();
    }

    private class sortAscendingHelper : IComparer<Entity>
    {
      int IComparer<Entity>.Compare(Entity e1, Entity e2)
      {
        return string.Compare(e1.Label, e2.Label);
      }
    }
  }
}
