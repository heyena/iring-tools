using System;
using Microsoft.Practices.Unity;

namespace PrismContrib.Events
{
    public class BootStrapperEventArgs : EventArgs
    {
        public IUnityContainer Container { get; set; }
        public BootStrapperEventArgs(IUnityContainer container)
        {
            this.Container = container;
        }
    }
}
