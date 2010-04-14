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

namespace SilverlightTest
{
  public class DataPropertyItem : TreeViewItem
  {
    private CheckBox _checkBox;
    private TextBlock _positionText;

    public DataPropertyItem() : this(String.Empty) {}

    public DataPropertyItem(string itemText)
    {
      StackPanel itemStackPanel = new StackPanel();
      itemStackPanel.Orientation = Orientation.Horizontal;
      _checkBox = new CheckBox();
      _checkBox.Content = itemText;
      itemStackPanel.Children.Add(_checkBox);
      _positionText = new TextBlock();
      itemStackPanel.Children.Add(_positionText);
      Header = itemStackPanel;
      _checkBox.Click += new RoutedEventHandler(_checkBox_Click);
    }

    void _checkBox_Click(object sender, RoutedEventArgs e)
    {
      ((DataObjectItem)Parent).Update();
    }

    public string Text
    {
      get { return Convert.ToString(_checkBox.Content); }
      set { _checkBox.Content = value; }
    }

    public int Position 
    {
      get
      {
        if (!String.IsNullOrEmpty(_positionText.Text))
        {
          return Convert.ToInt16(_positionText.Text.Substring(1, _positionText.Text.Length - 2));
        }

        return -1;
      }

      set
      {
        if (value == -1)
        {
          _positionText.Text = String.Empty;
        }
        else
        {
          _positionText.Text = "[" + value + "]";
        }
      }
    }

    public new bool IsSelected
    {
      get { return Convert.ToBoolean(_checkBox.IsChecked); }
      set { _checkBox.IsChecked = value; }
    }
  }
}
