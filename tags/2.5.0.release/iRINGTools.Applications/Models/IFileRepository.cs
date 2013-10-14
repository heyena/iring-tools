using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.IO;

namespace iRINGTools.Web.Models
{
  public interface IFileRepository
  {
       

    Response PostFile(string scope, string application, Stream inputFile, string fileName);
    byte[] getFile(string scope, string application, string fileName, string ext);
      

  }
}