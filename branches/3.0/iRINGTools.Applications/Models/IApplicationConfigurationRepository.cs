﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.applicationConfig;

namespace iRINGTools.Web.Models
{
    internal interface IApplicationConfigurationRepository
    {
        Folders GetFolders(string userName, Guid parentFolderId, int siteId);

        string AddFolder(string userName, Folder newFolder);

        string DeleteFolder(Folder folder);

        string UpdateFolder(string userName, Folder updatedFolder);

        Contexts GetContexts(string userName, Guid parentFolderId, int siteId);

        string AddContext(string userName, Context newContext);

        string UpdateContext(string userName, Context updatedContext);

        string DeleteContext(Context context);

        Applications GetApplications(string userName, Guid parentContextId, int siteId);

        string AddApplication(string userName, Application newApplication);

        string UpdateApplication(string userName, Application updatedApplication);

        string DeleteApplication(Application application);
    }
}
