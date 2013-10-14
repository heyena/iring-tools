using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjaFights.WeaponsCache1
{
  public class Dagger : IWeapon
  {
    public Dagger()
    {
    }

    public string Hit(string target)
    {
      return "Stab " + target + " to death";
    }
  }
}
