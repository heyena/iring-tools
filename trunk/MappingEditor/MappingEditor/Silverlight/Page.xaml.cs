﻿using System.Windows.Controls;
using System.Windows.Browser;
using System.Windows;

namespace MappingEditor
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            LayoutRoot.SizeChanged += new SizeChangedEventHandler(LayoutRoot_SizeChanged);
        }

        void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          Control c = (Control)MainItem.Items[0];
          c.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
          c.Width = LayoutRoot.ColumnDefinitions[1].ActualWidth;          
        }
    }
}
