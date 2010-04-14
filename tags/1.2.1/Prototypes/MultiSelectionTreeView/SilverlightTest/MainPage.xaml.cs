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

namespace SilverlightTest
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();

      DataObjectItem dataObjectItem = null;
      DataPropertyItem dataPropertyItem = null;

      //////////////////////////////////////

      dataObjectItem = new DataObjectItem();
      dataObjectItem.Header = "Lines";
      dataPropertyItem = new DataPropertyItem("Tag");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataPropertyItem = new DataPropertyItem("Description");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataPropertyItem = new DataPropertyItem("Diameter");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataObjectTree.Items.Add(dataObjectItem);

      //////////////////////////////////////

      dataObjectItem = new DataObjectItem();
      dataObjectItem.Header = "Valves"; 
      dataPropertyItem = new DataPropertyItem("Tag");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataPropertyItem = new DataPropertyItem("Description");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataPropertyItem = new DataPropertyItem("System");
      dataObjectItem.Items.Add(dataPropertyItem);
      dataObjectTree.Items.Add(dataObjectItem);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (dataObjectTree.SelectedItem == null) return;

      DataObjectItem dataObjectItem = null;
      if (dataObjectTree.SelectedItem is DataObjectItem)
      {
        dataObjectItem = (DataObjectItem)dataObjectTree.SelectedItem;
      }
      else // dataObjectTree.SelectedItem is DataPropertyItem
      {
        dataObjectItem = (DataObjectItem)((DataPropertyItem)dataObjectTree.SelectedItem).Parent;
      }
      MessageBox.Show(dataObjectItem.SelectedDataPropertyItems);
    }
  }
}
