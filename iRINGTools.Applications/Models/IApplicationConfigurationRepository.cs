using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.applicationConfig;
using library = org.iringtools.library;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
    internal interface IApplicationConfigurationRepository
    {
        Folders GetFolders(string userName, int siteId, int platformId, Guid parentFolderId);

        library.Response AddFolder(Folder newFolder);

        library.Response DeleteFolder(Folder folder);

        library.Response UpdateFolder(Folder updatedFolder);

        Contexts GetContexts(string userName, Guid parentFolderId);

        library.Response AddContext(Context newContext);

        library.Response UpdateContext(Context updatedContext);

        library.Response DeleteContext(Context context);

        Applications GetApplications(string userName, Guid parentContextId);

        library.Response AddApplication(Application newApplication);

        library.Response UpdateApplication(Application updatedApplication);

        library.Response DeleteApplication(Application application);

        DataDictionary GetDictionary(Guid guid);

        Graphs GetGraphs(string userName, Guid applicationId);

        ValueListMaps GetValueListMaps(string userName, Guid applicationId);
    }
}
