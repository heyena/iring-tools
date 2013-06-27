using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Logging;
namespace PrismContrib.Base
{
    public interface IPresenterBase<TView>
     where TView : IViewBase
    {
        IUnityContainer Container { get; set; }
        ILoggerFacade Logger { get; set; }
        IPresentationModel Model { get; set; }
        string ModuleFullName { get; set; }
        void OnModelSet();
        void OnViewSet();
        TView View { get; set; }
    }
}
