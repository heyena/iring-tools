using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public enum RDFRepositoryType
  {
    RDSWIP,
    Camelot,
    Part8,
  }

  public class RDFEntity
  {
    public string Uri { get; set; }
    public string Label { get; set; }
    public string Lang { get; set; }
    public string Repository { get; set; }

    public RDFEntity()
    {
    }
  }

  public class RDFRepository
  {
    public string Name { get; set; }
    public string Uri { get; set; }
    public string UpdateUri { get; set; }
    public string Description { get; set; }
    public string EncryptedCredentials { get; set; }
    public bool IsReadOnly { get; set; }
    public RDFRepositoryType RepositoryType { get; set; }

    public RDFQuerySet QuerySet { get; set; }

    public RDFRepository()
    {
    }
  }

  public class RDFQuerySet
  {
    public QMXF GetClass(string id)
    {
      throw new NotImplementedException();
    }
  }
}
