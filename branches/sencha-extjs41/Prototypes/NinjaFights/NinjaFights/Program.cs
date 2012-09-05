using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Syntax;
using Ninject.Modules;
using System.IO;

namespace NinjaFights
{
  public class Program
  {
    public class Samurai
    {      
      readonly IWeapon _weapon;

      public Samurai(IWeapon weapon)
      {
        _weapon = weapon;
      }

      public void Attack(string target)
      {
        Console.WriteLine(_weapon.Hit(target));
      }
    }

    public class Warrior
    {
      readonly IEnumerable<IWeapon> _weapons;

      public Warrior(IEnumerable<IWeapon> weapons)
      {
        _weapons = weapons;
      }
      public void Attack(string victim)
      {
        foreach (var weapon in _weapons)
          Console.WriteLine(weapon.Hit(victim));
      }
    }

    public static void Main()
    {
      string weaponsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Weapons\*.dll");

      IKernel kernel = new StandardKernel();
      kernel.Load(weaponsPath);

      string choice = string.Empty;

      while (!choice.Equals("Exit"))
      {
        choice = Console.ReadLine();

        try
        {
          var weapon = kernel.Get<IWeapon>(choice);
          Console.WriteLine(weapon.Hit("Koos"));
        }
        catch
        {
          Console.WriteLine("Wrong Choice");
        }

        //Warrior warior = kernel.Get<Warrior>();
        //warior.Attack("your enemy");              
      }

      Console.ReadKey();
    }
  }
}
