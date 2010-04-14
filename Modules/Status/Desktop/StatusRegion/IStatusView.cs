using PrismContrib.Base;

namespace Modules.Status.StatusRegion
{
  public interface IStatusView : IViewBase
  {
    string stsLeftMessage { get; set; }
    string stsRightMessage { get; set; }
    string stsMiddleMessage { get; set; }
  }
}
