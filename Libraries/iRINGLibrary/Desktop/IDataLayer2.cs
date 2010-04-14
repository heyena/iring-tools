// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.ids_adi.qxf;
using org.w3.sparql_results;

namespace org.iringtools.library
{
  public interface IDataObject
  {
    object GetPropertyValue(string propertyName);

    void SetPropertyValue(string propertyName, object value);
  }

  public interface IDataLayer2
  {
    object Create(string objectType, string identifier);

    IList<object> CreateList(string objectType, List<string> identifiers);

    object Get(string objectType, string identifier);

    IList<object> GetList(string objectType, DataFilter filter, int pageSize, int pageNumber);

    Response Post(object dataObject);

    Response PostList(List<object> dataObjects);

    Response Delete(string objectType, string identifier);

    Response DeleteList(string objectType, DataFilter filter);

    DataDictionary GetDictionary();
  }
}

public class ValveDO : Valve, org.iringtools.library.IDataObject
{
  public object GetPropertyValue(string propertyName)
  {
    switch (propertyName)
    {
      case "Tag":
        return this.Tag;
      
      case "LineTag":
        return this.LineTag;
        
      default:
        throw new Exception("Property " + propertyName + " does not exist in the DataObject.");
    }
  }

  public void SetPropertyValue(string propertyName, object value)
  {
    switch (propertyName)
    {
      case "Tag":
        this.Tag = value.ToString();
        break;

      case "LineTag":
        this.LineTag = value.ToString();
        break;

      default:
        throw new Exception("Property " + propertyName + " does not exist in the DataObject.");
    }
  }
}

public class Valve
{
  public string Tag { get; set; }
  public string LineTag { get; set; }
}
