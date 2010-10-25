using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;

using org.iringtools.modules.spinner;
using org.iringtools.modules.search;
using org.iringtools.modules.status;
using org.iringtools.modules.details;
using org.iringtools.modules;
using org.iringtools.modules.menu;

using org.iringtools.library.configuration;
using org.iringtools.library.presentation.configuration;

using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;

using PrismContrib.Errors;
using PrismContrib.Events;
using PrismContrib.Loggers;

namespace MappingEditor
{
    public class Bootstrapper : UnityBootstrapper
    {
      public IDictionary<string, string> InitParams { get; set; }

        protected override IModuleCatalog GetModuleCatalog()
        {
          ModuleCatalog catalog = new ModuleCatalog();

          catalog.AddModule(typeof(SpinnerModule));
          catalog.AddModule(typeof(StatusModule));
          catalog.AddModule(typeof(SearchModule));
          catalog.AddModule(typeof(DetailsModule));
          catalog.AddModule(typeof(MenuModule));
          catalog.AddModule(typeof(MappingModule),
                           new String[] { "SpinnerModule", "StatusModule", "SearchModule", "DetailsModule", "MenuModule" });
              
          return catalog;
        }

        protected override DependencyObject CreateShell()
        {
            if (InitParams == null)
              InitParams = new Dictionary<string, string>();

            Container.RegisterInstance<IDictionary<string, string>>
              ("InitParameters", InitParams, new ContainerControlledLifetimeManager());
            Container.RegisterType<IError, Error>();
            Container.RegisterType<IServerConfiguration, ServerConfiguration>(
              new ContainerControlledLifetimeManager());

            Page shell = Container.Resolve<Page>();

#if SILVERLIGHT
            Application.Current.RootVisual = shell;
#else
            Application.Current.MainWindow = shell;
            shell.Show();
#endif

            return shell;
        }
    }
}


