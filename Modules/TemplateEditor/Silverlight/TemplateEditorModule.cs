using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;

using Modules.TemplateEditor.EditorRegion;

using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;

using org.ids_adi.iring.referenceData;

namespace Modules.TemplateEditor
{
    public class TemplateEditorModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public override void RegisterViewsAndServices()
        {
            Container
                .RegisterType<ILoggerFacade, DebugLogger>()
                .RegisterType<IIMPresentationModel, IMPresentationModel>(new ContainerControlledLifetimeManager())
                .RegisterType<IAdapter, AdapterBLL>(new ContainerControlledLifetimeManager()) // Singleton
                .RegisterType<IAdapter, AdapterDAL>("AdapterProxyDAL", new ContainerControlledLifetimeManager()) // Singleton
                .RegisterType<IReferenceData, ReferenceDataBLL>(new ContainerControlledLifetimeManager()) // Singleton
                .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL", new ContainerControlledLifetimeManager())  // Singleton                
                .RegisterType<ITemplateEditorView, TemplateEditorView>()
            ;
        }

        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("TemplateEditorRegion", () => Container.Resolve<TemplateEditorPresenter>().View);
        }
    }
}
