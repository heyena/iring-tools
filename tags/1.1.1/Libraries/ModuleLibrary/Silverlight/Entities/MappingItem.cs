using System.Windows.Controls;
using System.Windows;
using InformationModel.UserControls;
using ModuleLibrary.Types;
using org.iringtools.library;

namespace ModuleLibrary.Entities
{
  public class MappingItem : CustomTreeItem
  {
    public NodeType NodeType { get; set; }
    public GraphMap GraphMap { get; set; }
    public ClassMap ClassMap { get; set; }
    public TemplateMap TemplateMap { get; set; }
    public RoleMap RoleMap { get; set; }

    // Need to oveeride this method
    public override void nodeSelectedHandler(object sender, RoutedEventArgs e)
    {
      //TODO may need to do something here
    }
  }
}
