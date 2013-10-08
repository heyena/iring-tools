using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{
  public interface IAuthHeaders
  {
    IDictionary<string, string> Get(string user);
  }
}
