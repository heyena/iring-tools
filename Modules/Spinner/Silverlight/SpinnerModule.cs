using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;

using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;

using org.ids_adi.iring.referenceData;

namespace Modules.Spinner
{
    public class SpinnerModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public override void RegisterViewsAndServices()
        {
            Container
                .RegisterType<IWorkingSpinner, WorkingSpinner>(new ContainerControlledLifetimeManager());
            ;
        }

        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("SpinnerRegion", () => Container.Resolve<WorkingSpinner>());
        }
    }
}
