﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;

namespace org.iringtools.library
{
  public class GenericContentObject : GenericDataObject, IContentObject
  {
    public GenericContentObject() : base() {}

    public GenericContentObject(IDataObject dataObject)
    {
      if (typeof(GenericDataObject).IsAssignableFrom(dataObject.GetType()))
        _dictionary = ((GenericDataObject)dataObject).Dictionary;
    }

    [IgnoreDataMember]
    [XmlIgnore]
    public Stream Content { get; set; }

    public string ContentType { get; set; }

    public string HashValue { get; set; }

    public string HashType { get; set; }

    public string URL { get; set; }

    public string Identifier { get; set; }
  }
}