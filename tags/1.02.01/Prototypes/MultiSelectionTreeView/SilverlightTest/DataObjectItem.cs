using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SilverlightTest
{
  public class DataObjectItem : TreeViewItem
  {
    private List<string> _selectedItems = new List<string>();

    public DataObjectItem() : this(String.Empty) { }

    public DataObjectItem(string text)
    {
      StackPanel itemStackPanel = new StackPanel();
      itemStackPanel.Orientation = Orientation.Horizontal;
      //itemStackPanel.Children.Add(icon);
      TextBlock textBlock = new TextBlock();
      textBlock.Text = text;
      itemStackPanel.Children.Add(textBlock);
      Header = itemStackPanel;
    }

    public string SelectedDataPropertyItems
    {
      get {
        string selectedItems = String.Empty;

        foreach (string selectedItem in _selectedItems)
        {
          if (selectedItems.Length > 0)
          {
            selectedItems += ", ";
          }

          selectedItems += selectedItem;
        }

        return selectedItems;
      }
    }

    public void Update()
    {
      TreeView dataObjectTree = (TreeView)Parent;

      // unselect data property items in other data object items
      foreach (DataObjectItem dataObjectItem in dataObjectTree.Items)
      {
        if (dataObjectItem != this)
        {
          foreach (DataPropertyItem dataPropertyItem in dataObjectItem.Items)
          {
            if (dataPropertyItem.IsSelected)
            {
              dataPropertyItem.IsSelected = false;
              dataPropertyItem.Position = -1;
            }
          }

          dataObjectItem._selectedItems.Clear();
        }
      }

      // refresh selected data property items positions
      foreach (DataObjectItem dataObjectItem in dataObjectTree.Items)
      {
        if (dataObjectItem == this)
        {
          foreach (DataPropertyItem dataPropertyItem in Items)
          {
            if (dataPropertyItem.Position == -1)
            {
              if (dataPropertyItem.IsSelected)
              {
                _selectedItems.Add(dataPropertyItem.Text);
                dataPropertyItem.Position = _selectedItems.Count;
              }
            }
            else if (!dataPropertyItem.IsSelected)
            {
              _selectedItems.RemoveAt(dataPropertyItem.Position - 1);
              dataPropertyItem.IsSelected = false;
              int deletedPosition = dataPropertyItem.Position;
              dataPropertyItem.Position = -1;
              
              // shift selected data property items by one position to the left
              foreach (DataPropertyItem dataPropertyItemToShift in Items)
              {
                if (dataPropertyItemToShift.Position > deletedPosition)
                {
                  dataPropertyItemToShift.Position--;
                }
              }
            }
          }

          break;
        }
      }
    }
  }
}
