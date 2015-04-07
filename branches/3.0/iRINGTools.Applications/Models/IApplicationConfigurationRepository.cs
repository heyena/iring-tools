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

        org.iringtools.applicationConfig.Contexts GetContexts(string userName, Guid parentFolderId);

        library.Response AddContext(org.iringtools.applicationConfig.Context newContext);

        library.Response UpdateContext(org.iringtools.applicationConfig.Context updatedContext);

        library.Response DeleteContext(org.iringtools.applicationConfig.Context context);

        Applications GetApplications(string userName, Guid parentContextId);

        library.Response AddApplication(Application newApplication);

        library.Response UpdateApplication(Application updatedApplication);

        library.Response DeleteApplication(Application application);

        library.DataDictionary GetDictionary(Guid applicationId);

        Graphs GetGraphs(string userName, Guid applicationId);

        ValueListMaps GetValueListMaps(string userName, Guid applicationId);
    }
}
