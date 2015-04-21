using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.applicationConfig;
using org.iringtools.mapping;
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
    internal interface IApplicationConfigurationRepository
    {
        Folders GetFolders(string userName, int siteId, int platformId, Guid parentFolderId);

        Response AddFolder(Folder newFolder);

        Response DeleteFolder(Folder folder);

        Response UpdateFolder(Folder updatedFolder);

        org.iringtools.applicationConfig.Contexts GetContexts(string userName, Guid parentFolderId);

        Response AddContext(org.iringtools.applicationConfig.Context newContext);

        Response UpdateContext(org.iringtools.applicationConfig.Context updatedContext);

        Response DeleteContext(org.iringtools.applicationConfig.Context context);

        Applications GetApplications(string userName, Guid parentContextId);

        Response AddApplication(Application newApplication);

        Response UpdateApplication(Application updatedApplication);

        Response DeleteApplication(Application application);

        org.iringtools.library.DataDictionary GetDictionary(Guid applicationId);

        Graphs GetGraphs(string userName, Guid applicationId);

        ValueListMaps GetValueListMaps(string userName, Guid applicationId);

        DataLayers GetDataLayers(int siteId, int platformId);

        DataFilter GetDataFilter(Guid DataObjectOrGraphId);
    }
}
