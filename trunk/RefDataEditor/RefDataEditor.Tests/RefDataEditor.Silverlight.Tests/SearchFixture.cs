using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modules.Search.SearchRegion;
using ModuleLibrary.LayerDAL;
using Microsoft.Practices.Unity;
using ModuleLibrary.Events;
using OntologyService.Interface;
using PrismContrib.Loggers;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Events;
using Microsoft.Silverlight.Testing;
using Microsoft.Practices.Composite.Logging;
using OntologyService.Interface.PresentationModels;
using ModuleLibrary.LayerBLL;
using Library.Interface.Configuration;
using Library.Configuration;

namespace RefDataEditor.SilverLight.Tests
{
  [TestClass]
  public class SearchFixture : SilverlightTest
  {
    public IDictionary<string, string> InitParams { get; set; }

    [TestMethod]
    [Asynchronous]
    public void ButtonClickSearchTest()
    {
      string initParamsValue = @"UseAdapterServiceStub#false~AdapterProxy#Services/AdapterProxy.svc~AdapterServiceUri#http://localhost:63997/Service.svc~AdapterCredentialToken#~AdapterProxyCredentialToken#~AdapterProxyHost#~AdapterProxyPort#~AdapterPageSize#20~ReferenceDataProxy#Services/ReferenceDataProxy.svc~ReferenceDataServiceUri#http://localhost:64001/Service.svc~ReferenceDataCredentialToken#~ReferenceDataProxyCredentialToken#~ReferenceDataProxyHost#~ReferenceDataProxyPort#~ReferenceDataPageSize#20~WebServerURL#http://localhost:63993~WebServerPath#C:\iring.svn.codeplex.com\MappingEditor\MappingEditor.Web~WebSiteName#MappingEditor.Web";

      InitParams = new Dictionary<string, string>();
      InitParams.Add("InitParameters", initParamsValue);

      IUnityContainer container = new UnityContainer();
      container.RegisterType<ISearchControl, SearchControl>();
      ISearchControl view = container.Resolve<ISearchControl>();

      container.RegisterType<IEventAggregator, EventAggregator>();
      IEventAggregator aggregator = container.Resolve<IEventAggregator>();

      container.RegisterType<ILoggerFacade, DebugLogger>();

      container.RegisterType<IIMPresentationModel, IMPresentationModel>();
      IMPresentationModel model = container.Resolve<IMPresentationModel>();

      container.RegisterType<IError, Error>();

      container.RegisterType<IServerConfiguration, ServerConfiguration>(
        new ContainerControlledLifetimeManager());

      container.RegisterInstance<IDictionary<string, string>>
                 ("InitParameters", InitParams, new ContainerControlledLifetimeManager());

      container.RegisterType<IReferenceData, ReferenceDataBLL>(
                    new ContainerControlledLifetimeManager()); // Singleton

      container.RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL",
                   new ContainerControlledLifetimeManager()); // Singleton

      IReferenceData referenceDataService = container.Resolve<IReferenceData>();


      SearchControlPresenter target = new SearchControlPresenter(view, (IMPresentationModel)model, aggregator, referenceDataService, container);
      Button button = new Button();
      button.Name = "SearchControlPresenter:btnSearch";
      button.Tag = "pump";
      ButtonEventArgs e = new ButtonEventArgs();
      e.ButtonClicked = button;
      target.ButtonClickHandler(e);
      Assert.Inconclusive("A method that does not return a value cannot be verified.");
    }
  }
}
