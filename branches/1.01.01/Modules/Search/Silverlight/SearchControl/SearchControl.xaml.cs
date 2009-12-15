﻿using System.Windows.Controls;

using PrismContrib.Base;

using Library.Interface.Events;
using Microsoft.Practices.Composite.Events;

using Microsoft.Practices.Composite.Regions;

namespace Modules.Search.SearchRegion
{
    public partial class SearchControl : UserControl, ISearchControl
    {
      public SearchControl(IEventAggregator aggregator)
        {
          InitializeComponent();
          
          itcModelBrowser.SizeChanged += new System.Windows.SizeChangedEventHandler(itcModelBrowser_SizeChanged);
          itcModelBrowser.Loaded += new System.Windows.RoutedEventHandler(itcModelBrowser_SizeChanged);
        }

        void itcModelBrowser_SizeChanged(object sender, System.Windows.RoutedEventArgs e)
        {
          if (itcModelBrowser.Items.Count > 0)
          {
            System.Windows.Controls.Control userControl = (System.Windows.Controls.Control)itcModelBrowser.Items[0];
            userControl.Height = itcModelBrowser.ActualHeight;
            //userControl.Width = itcModelBrowser.ActualWidth;
          }
        }

        public IPresentationModel Model { get; set; }

    }
}
