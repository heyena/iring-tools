using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.layerbll;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.modules.edits.editsregion;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

namespace org.iringtools.modules.edits
{
    public class EditsModule : ModuleBase
    {
        public override void RegisterViewsAndServices()
        {

            Container.RegisterType<IEditsView, EditsView>() ;

        }
        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("EditsRegion", () => Container.Resolve<EditsPresenter>().View);
        }
    }
}
