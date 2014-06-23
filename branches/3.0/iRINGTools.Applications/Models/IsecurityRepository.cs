using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.UserSecurity;
using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;
using System.IO;

namespace iRINGTools.Web.Models
{
    public interface IsecurityRepository
    {
      Users GetAllUsers(string format);

     //   List<User> GetAllUsers(string format);

    }
}