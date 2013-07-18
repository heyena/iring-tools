using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.ontologyservice.presentation;

using org.ids_adi.qmxf;

namespace org.iringtools.modules.popup.popupregion
{
    public class PopupPresenter : PresenterBase<IPopupView>
    {
        private IIMPresentationModel model = null;
        private IEventAggregator aggregator = null;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupPresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        public PopupPresenter(IPopupView view, IIMPresentationModel model,
          IEventAggregator aggregator)
            : base(view, model)
        {
            this.aggregator = aggregator;
            this.model = model;
        }
        
    }
}
