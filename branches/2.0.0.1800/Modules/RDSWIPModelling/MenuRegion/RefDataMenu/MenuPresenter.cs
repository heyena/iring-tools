using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;

using org.iringtools.library.presentation.events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using org.iringtools.utility;

namespace org.iringtools.menu.views.menuregion
{
    public class MenuPresenter : PresenterBase<IMenuView>
    {

        #region btnMappingEditor
        private Button btnMappingEditor
        {
            get { return ButtonCtrl("btnMappingEditor1"); }
        }

        #endregion
        #region btnRefDataEditor
        private Button btnRefDataEditor
        {
            get { return ButtonCtrl("btnRefDataEditor1"); }
        }
        #endregion

        private IEventAggregator aggregator;
        private IRegionManager regionManager;

        private IMPresentationModel model;
        private IReferenceData referenceDataBLL;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        /// <param name="regionManager">The region manager.</param>
        public MenuPresenter(IMenuView view, IIMPresentationModel model,
          IRegionManager regionManager,
          IEventAggregator aggregator,
          IReferenceData referenceDataBLL)
            : base(view, model)
        {
            try
            {
                this.model = (IMPresentationModel)model;
                this.regionManager = regionManager;
                this.aggregator = aggregator;
                this.referenceDataBLL = referenceDataBLL;

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        void adapterService_OnDataArrived(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Enables the buttons.
        /// </summary>
        private void EnableButtons()
        {
            //btnRefDataEditor.IsEnabled = true;
            //btnMappingEditor.IsEnabled = true;
        }

        //void OnClassDataArrivedHandler(object sender, System.EventArgs e)
        //{
        //  try
        //  {
        //    CompletedEventArgs args = (CompletedEventArgs)e;
        //    QMXF qmxf = (QMXF)args.Data;


        //    ShowView();
        //  }
        //  catch (Exception ex)
        //  {
        //    Error.SetError(ex);
        //  }
        //}
    }
}
