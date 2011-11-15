using System.Windows.Controls;
using System.Windows;
using org.iringtools.informationmodel.usercontrols;
using org.iringtools.modulelibrary.types;
using org.iringtools.library;
using org.iringtools.mapping;

namespace org.iringtools.modulelibrary.entities
{
  public class MappingItem : CustomTreeItem
  {
    public NodeType NodeType { get; set; }
    public GraphMap GraphMap { get; set; }
    public ClassMap ClassMap { get; set; }
    public TemplateMap TemplateMap { get; set; }
    public RoleMap RoleMap { get; set; }
    public ValueMap ValueMap { get; set; }

    // Need to oveeride this method
    public override void nodeSelectedHandler(object sender, RoutedEventArgs e)
    {
    }
  }
}
