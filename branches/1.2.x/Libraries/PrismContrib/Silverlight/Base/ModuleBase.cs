using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

namespace PrismContrib.Base
{
    public abstract class ModuleBase : IModule
    {
        public string ModuleFullName;
        
        [Dependency]
        public ILoggerFacade Logger { get; set; }

        [Dependency]
        public IUnityContainer Container { get; set; }

        [Dependency]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Initialize()
        {
            ModuleFullName = GetType().FullName;

            Logger.Log(string.Format("{0}.Initialize()", ModuleFullName), Category.Debug, Priority.None);

            RegisterViewsAndServices();
            RegisterTypesForPullBasedComposition();

        }

        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public abstract void RegisterViewsAndServices();
        
        /// <summary>
        /// Registers the types for pull based composition.
        /// </summary>
        public abstract void RegisterTypesForPullBasedComposition();
    }
}
