using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;

namespace NinjaFights.WeaponsCache1
{
  public class WeaponsCache : NinjectModule, INinjectModule
  {
    public WeaponsCache()
    {
    }

    public override void Load()
    {
      Bind<IWeapon>().To<Dagger>().Named("Dagger");
    }
  }
}
