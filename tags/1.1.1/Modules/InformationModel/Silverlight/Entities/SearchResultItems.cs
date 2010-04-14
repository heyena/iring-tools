using System.Windows.Controls;
using InformationModel.Types;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;
using org.ids_adi.iring.utility;

namespace InformationModel.Entities
{
    public class SearchResultItem : TreeViewItem
    {
        public bool IsProcessed { get; set; }
        public SPARQLPrefix.ObjectType NodeType { get; set; }
        public QMXF QMXF { get; set; }
        public Entity Entity { get; set; }
    }
}
