using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;


namespace org.iringtools.modules.edits.editsregion
{
    public class EditsPresenter : PresenterBase<IEditsView>
    {
        private IIMPresentationModel model = null;
        private IEventAggregator aggregator = null;

        private Grid grdDetails { get { return GridCtrl("editg"); } }


        public EditsPresenter(IEditsView view, IIMPresentationModel model,
      IEventAggregator aggregator)
            : base(view, model)
        {
            this.aggregator = aggregator;
            this.model = model;
            aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);
        }

        public void NavigationEventHandler(NavigationEventArgs e)
        {
        }
    }

}
