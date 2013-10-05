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
using org.iringtools.library;
using System.Text;


namespace ApplicationEditor
{
  public partial class Relations : ChildWindow
  {
    internal ObservableCollection<String> _existingRelations;
    internal ObservableCollection<String> _relatedObjects;
    internal ObservableCollection<String> _relationType;
    internal ObservableCollection<String> _selectedObjectProperties;
    internal ObservableCollection<String> _targetObjectProperties;
    internal ObservableCollection<String> _relatedObjectProperties;
    internal DataRelationship _dataRelationship;
    public Relations()
    {
      InitializeComponent();
      _existingRelations = new ObservableCollection<string>();
      _relatedObjects = new ObservableCollection<string>();
      _relationType = new ObservableCollection<string>();
      _selectedObjectProperties = new ObservableCollection<string>();
      _targetObjectProperties = new ObservableCollection<string>();
      _relatedObjectProperties = new ObservableCollection<string>();
      _dataRelationship = (DataRelationship)tblPrimaryObject.Tag;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
    }

    private void btnAddRelation_Click(object sender, RoutedEventArgs e)
    {
      if (cbSourceProps.SelectedIndex == -1)
      {
        MessageBox.Show("Please select a source property", "CREATE RELATION", MessageBoxButton.OK);
        cbSourceProps.Focus();
        return;
      }
      else if (cbRelated.SelectedIndex == -1)
      {
        MessageBox.Show("Please select a related data object", "CREATE RELATION", MessageBoxButton.OK);
        cbRelated.Focus();
        return;
      }
      else if (cbRelatedProps.SelectedIndex == -1)
      {
        MessageBox.Show("Please select a related data property", "CREATE RELATION", MessageBoxButton.OK);
        cbRelatedProps.Focus();
        return;
      }
      else if (cbRelationType.SelectedIndex == -1)
      {
        MessageBox.Show("Please select a related data property", "CREATE RELATION", MessageBoxButton.OK);
        cbRelationType.Focus();
        return;
      }
      else if (string.IsNullOrEmpty(tblRelationshipName.Text))
      {
        MessageBox.Show("Please Please enter relationship name", "CREATE RELATION", MessageBoxButton.OK);
        tblRelationshipName.Focus();
        return;
      }
      else
      {

        StringBuilder sb = new StringBuilder();
        //sb.Append(tblPrimaryObject.Text);
        //sb.Append(".");
        sb.Append(cbSourceProps.SelectedItem.ToString());
        sb.Append(".");
        //sb.Append(cbRelationType.SelectedItem.ToString());
        //sb.Append(".");
        //sb.Append(cbRelated.SelectedItem.ToString());
        //sb.Append(".");
        sb.Append(cbRelatedProps.SelectedItem.ToString());
        if (!lbRelatedProps.Items.Contains(sb.ToString()))
        {
          lbRelatedProps.Items.Add(sb.ToString());
        }
        else
        {
          MessageBox.Show("Property map exists already", "EDIT RELATION", MessageBoxButton.OK);
          return;
        }
      }
    }

    private void btnDelRelation_Click(object sender, RoutedEventArgs e)
    {
      lbRelatedProps.Items.Remove(lbRelatedProps.SelectedItem);
    }

    private void cbRelated_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      DatabaseDictionary dbdict = (DatabaseDictionary)cbRelated.Tag;
      org.iringtools.library.DataObject dataObject;

      string selectedObject = e.AddedItems[0].ToString();
      dataObject = dbdict.dataObjects.First(c => c.objectName == selectedObject);
      foreach (DataProperty prop in dataObject.dataProperties)
      {
        _relatedObjectProperties.Add(prop.propertyName);
      }
      cbRelatedProps.ItemsSource = _relatedObjectProperties;
    }

    private void cbExisting_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      lbRelatedProps.Items.Clear();
      DatabaseDictionary dbdict = (DatabaseDictionary)cbRelated.Tag;
      string selectedrelationship = e.AddedItems[0].ToString();
      org.iringtools.library.DataObject selectedDataObject = tblPrimaryObject.Tag as org.iringtools.library.DataObject;
      DataRelationship dataRelationship = selectedDataObject.dataRelationships.First(c => c.relationshipName == e.AddedItems[0].ToString());
      tblRelationshipName.Text = dataRelationship.relationshipName;
      cbRelated.SelectedValue = dataRelationship.relatedObjectName;
      cbRelationType.SelectedValue = dataRelationship.relationshipType.ToString();
      foreach (PropertyMap map in dataRelationship.propertyMaps)
      {
        lbRelatedProps.Items.Add(map.dataPropertyName + "." + map.relatedPropertyName);
      }
    }

  }
}

