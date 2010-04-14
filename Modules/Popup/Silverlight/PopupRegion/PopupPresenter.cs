using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;

using InformationModel.Events;
using ModuleLibrary.Events;

using OntologyService.Interface.PresentationModels;
using OntologyService.Interface;

using org.ids_adi.iring;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;

namespace Modules.Popup.PopupRegion
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
