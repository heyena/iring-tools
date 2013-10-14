using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;
using System.Web.SessionState;

namespace org.iringtools.library
{
  public interface IAuthentication
  {
    string Authenticate(HttpSessionState session);
  }
}
