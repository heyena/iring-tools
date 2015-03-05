using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.applicationConfig;
using library = org.iringtools.library;

namespace iRINGTools.Web.Models
{
    internal interface IApplicationConfigurationRepository
    {
        Folders GetFolders(string userName, Guid parentFolderId, int siteId);

        library.Response AddFolder(string userName, Folder newFolder);

        library.Response DeleteFolder(Folder folder);

        library.Response UpdateFolder(string userName, bool IsFolderNameChanged, Folder updatedFolder);

        Contexts GetContexts(string userName, Guid parentFolderId, int siteId);

        library.Response AddContext(string userName, Context newContext);

        library.Response UpdateContext(string userName, Context updatedContext);

        library.Response DeleteContext(Context context);

        Applications GetApplications(string userName, Guid parentContextId, int siteId);

        string AddApplication(string userName, Application newApplication);

        string UpdateApplication(string userName, Application updatedApplication);

        string DeleteApplication(Application application);
    }
}
