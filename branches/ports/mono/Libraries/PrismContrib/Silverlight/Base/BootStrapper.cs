using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using System.Collections.Generic;
using System;
using PrismContrib.Events;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using PrismContrib.Interfaces;

namespace PrismContrib.Base
{
    /// <summary>
    /// Bootstrapper - see the following link for Sequence Diagram
    /// http://global-webnet.net/UML/prism.htm
    /// </summary>
    public class Bootstrapper<TShell> : UnityBootstrapper
#if SILVERLIGHT
        where TShell : UIElement
#else
        where TShell : Window
#endif
    {
        public event EventHandler<BootStrapperEventArgs> OnInitializeModules;
        public event EventHandler<BootStrapperEventArgs> OnConfigureContainer;

        public IDictionary<string, string> InitParams { get; set; }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject CreateShell()
        {
            if (InitParams == null)
                InitParams = new Dictionary<string, string>();

            Container.RegisterInstance<IDictionary<string,string>>
                ("InitParameters", InitParams, new ContainerControlledLifetimeManager());

            TShell shell = this.Container.Resolve<TShell>();
            

#if SILVERLIGHT
            Application.Current.RootVisual = shell;
#else
            if (!(shell is IUnitTest))
                shell.Show();
#endif
            return shell;
        }

        protected override void ConfigureContainer()
        {
            BootStrapperEventArgs args = 
                new BootStrapperEventArgs(Container);
            
            // Give the application an opportunity to 
            // configure the container
            if (OnConfigureContainer != null)
                OnConfigureContainer(this, args);

            base.ConfigureContainer();
                    
        }


        /// <summary>
        /// Initializes the modules.
        /// </summary>
        protected override void InitializeModules()
        {
            // Give the application an opportunity to
            // initialize modules
            if (OnInitializeModules != null)
                OnInitializeModules(this, new BootStrapperEventArgs(Container));
        }
    }
}
