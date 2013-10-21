using System;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Collections;
using System.Web;
using System.Web.SessionState;

namespace org.iringtools.library
{
  public interface IAuthorization
  {
    bool Authorize(HttpSessionState session, string application, string user);
  }
}
