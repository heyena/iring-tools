using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.adapter.security
{
  public interface IAuthorizationLayer
  {
    void Init(IDictionary<string, string> settings);
    bool IsAuthorized(IDictionary<string, string> claims);
  }
}
