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
using System.Windows.Browser;


namespace org.iringtools.modules.details.detailsregion
{
  public class DetailsPresenter : PresenterBase<IDetailsView>
  {
    private IIMPresentationModel model = null;
    private IEventAggregator aggregator = null;
    //This is to make a change delete it!
    
    /// <summary>
    /// Gets the details grid.
    /// </summary>
    /// <value>The grid details.</value>
    private Grid grdDetails { get { return GridCtrl("dg"); } }
    private DataGrid grdData { get { return DataGridCtrl("dg"); } }
    private Button btnClipboard { get { return ButtonCtrl("btnClipBoard"); } }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDetailPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    public DetailsPresenter(IDetailsView view, IIMPresentationModel model,
      IEventAggregator aggregator)
      : base(view, model)
    {
      this.aggregator = aggregator;
      this.model = model;
      aggregator.GetEvent<NavigationEvent>().Subscribe(NavigationEventHandler);
      //btnClipboard.Click += btnClipBoard_Click;
    }

    //private void btnClipBoard_Click(object sender, System.Windows.RoutedEventArgs e)
    //{
    //  if (grdData.SelectedItem != null)
    //  {
    //    KeyValuePair<string, string> row = (KeyValuePair<string, string>)grdData.SelectedItem;
    //    HtmlPage.Window.Eval("window.clipboardData.setData('Text','" + row.Key + ": " + row.Value + "')");
    //  }
    //}

    public void NavigationEventHandler(NavigationEventArgs e)
    {

    }
  }
}
