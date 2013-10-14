using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjaFights.WeaponsCache3
{
  public class Shuriken : IWeapon
  {
    public Shuriken()
    {
    }

    public string Hit(string target)
    {
      return "Pierced " + target + "'s armor";
    }
  }
}
