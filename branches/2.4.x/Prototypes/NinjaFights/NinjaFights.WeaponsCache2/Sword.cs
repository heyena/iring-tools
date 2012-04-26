using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjaFights.WeaponsCache2
{
  public class Sword : IWeapon
  {
    public Sword()
    {
    }

    public string Hit(string target)
    {
      return "Slice " + target + " in half";
    }
  }
}
