using System;
using System.Collections.Generic;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;
using org.ids_adi.iring;
using org.iringtools.library;

namespace org.iringtools.modulelibrary.layerdal
{
    public interface IReferenceData //: IReferenceDataSilverlight
    {
        event EventHandler<EventArgs> OnDataArrived;

        object Search(string query, object userState);

        object SearchReset(string query, object userState);

        object Find(string query, object userState);

        QMXF GetClass(string id, object userState);

        List<Entity> GetSubClasses(string id, object userState);

        List<Entity> GetSuperClasses(string id, object userState);

        List<Entity> GetClassTemplates(string id, object userState);

        void GetClassLabel(string key, string uri, object userState);

        void GetTemplateLabel(string key, string uri, object userState);

        QMXF GetTemplate(string id, object userState);

        Response PostTemplate(QMXF template);

        Response PostClass(QMXF @class);

        string GetReferenceDataServiceUri { get; }

    }
}
