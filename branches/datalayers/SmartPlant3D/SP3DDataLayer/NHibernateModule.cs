using Ninject.Modules;

namespace iringtools.sdk.sp3ddatalayer
{
  public class NHibernateModule : NinjectModule
  {
    public override void Load()
    {
      Bind<NHibernateSettings>().ToSelf().InSingletonScope();
    }
  }
}
