using System.Windows.Controls;
using PrismContrib.Base;

namespace InformationModel.Views.ClassDetails
{
    public partial class DetailView : UserControl, IDetailView
    {
        public DetailView()
        {
            InitializeComponent();
        }

        public IPresentationModel Model { get; set; }

    }
}
