using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using org.iringtools.sdk.objects.widgets;

namespace WidgetService
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class WidgetService
    {
        private WidgetProvider _widgetProvider = null;
        public WidgetService()
        {
            _widgetProvider = new WidgetProvider();

        }

        // TODO: Implement the collection resource that will contain the SampleItem instances

        //[WebGet(UriTemplate = "/Generate")]
        //public List<Sample> Generate()
        //{
        //    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
        //    context.ContentType = "application/xml";
        //    return _widgetProvider.Generate();
           
        //}

        [WebInvoke(UriTemplate = "", Method = "POST")]
        public Widget Create(Widget instance)
        {
            // TODO: Add the new instance of SampleItem to the collection
            throw new NotImplementedException();
        }

        [WebGet(UriTemplate = "")]
        public Widgets GetAll()
        {
            Widgets widgets = _widgetProvider.ReadWidgets(null);

            return widgets;
        }

        [WebGet(UriTemplate = "{identifier}")]
        public Widget Get(string identifier)
        {
            int id = Int32.Parse(identifier);
            Widget widget = _widgetProvider.ReadWidget(id);

            return widget;
        }

        [WebInvoke(UriTemplate = "{id}", Method = "PUT")]
        public Widget Update(string id, Widget instance)
        {
            // TODO: Update the given instance of SampleItem in the collection
            throw new NotImplementedException();
        }

        [WebInvoke(UriTemplate = "{id}", Method = "DELETE")]
        public void Delete(string id)
        {
            // TODO: Remove the instance of SampleItem with the given id from the collection
            throw new NotImplementedException();
        }

    }
}
