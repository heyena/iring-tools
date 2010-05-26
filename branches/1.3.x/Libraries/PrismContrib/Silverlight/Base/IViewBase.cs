
using System.Windows.Threading;
namespace PrismContrib.Base
{
    public interface IViewBase
    {
        IPresentationModel Model { get; set; }
        object DataContext { get; set; }
        Dispatcher Dispatcher { get;  }
        object FindName(string name);
        string Name { get; }
    }
}
