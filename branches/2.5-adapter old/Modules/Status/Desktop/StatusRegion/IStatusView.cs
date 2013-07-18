using PrismContrib.Base;

namespace org.iringtools.modules.status.statusregion
{
  public interface IStatusView : IViewBase
  {
    string stsLeftMessage { get; set; }
    string stsRightMessage { get; set; }
  }
}
