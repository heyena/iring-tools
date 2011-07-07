using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.adapter.security
{
  interface IAuthorizationLayer
  {
    bool IsAuthorized(IDictionary<string, string> claims);
  }
}
