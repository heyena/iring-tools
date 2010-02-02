using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using InformationModel.Events;
using org.iringtools.library;


using org.iringtools.library.configuration;

namespace org.iringtools.modules.projectapplicationregion
{
    public class ProjectApplicationPresenter : PresenterBase<ProjectApplicationView>
    {
        private IEventAggregator aggregator = null;
        private IAdapter adapterProxy = null;
        IIMPresentationModel model = null;

        private ComboBox prjCB { get { return ComboBoxCtrl("ProjectCombo"); } }

        private ComboBox appCB { get { return ComboBoxCtrl("AppCombo"); } }
  

        public ProjectApplicationPresenter(IProjectApplicationView view, IIMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy,
      IUnityContainer container)
            : base(view, model)
        {
           
            prjCB.SelectionChanged += new SelectionChangedEventHandler(prjCB_SelectionChanged);
            appCB.SelectionChanged += new SelectionChangedEventHandler(appCB_SelectionChanged);
            adapterProxy.OnDataArrived += new EventHandler<EventArgs>(adapterProxy_OnDataArrived);
            
        }

        void adapterProxy_OnDataArrived(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }



        void appCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (prjCB.SelectedIndex != -1)
            {
              
            }
        }

        void prjCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
