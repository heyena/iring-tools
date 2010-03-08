using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace CSVService
{
  [ServiceContract]
  public interface ICSVService
  {
    [OperationContract]
    [WebGet(UriTemplate = "/save?fileName={fileName}&content={content}")]
    string Save(string fileName, string content);
  }
}
