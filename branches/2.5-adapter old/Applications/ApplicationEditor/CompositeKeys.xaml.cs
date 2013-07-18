using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace ApplicationEditor
{
  public partial class CompositeKeys : ChildWindow
  {
    internal ObservableCollection<String> _keyItems;
    internal ObservableCollection<String> _dataItems;
    public CompositeKeys()
    {
      InitializeComponent();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }

    private void btnAddCompKey_Click(object sender, RoutedEventArgs e)
    {
      String lbItem = lbSourceProperties.SelectedItem as String;
      int index = lbSourceProperties.SelectedIndex;
      if (lbItem != null)
      {
        if (!_keyItems.Contains(lbItem))
        {
          _dataItems.RemoveAt(index);
          _keyItems.Add(lbItem);
        }
      }
    }


    private void btnDelCompKey_Click(object sender, RoutedEventArgs e)
    {
      String lbItem = (String)lbKeys.SelectedItem;
      int index = lbKeys.SelectedIndex;
      if (lbItem != null)
      {
        if (!_dataItems.Contains(lbItem))
        {
          _keyItems.RemoveAt(index);
          _dataItems.Add(lbItem);
        }
      }
    }

    private void ChildWindow_Closed(object sender, EventArgs e)
    {

    }
  }
}




