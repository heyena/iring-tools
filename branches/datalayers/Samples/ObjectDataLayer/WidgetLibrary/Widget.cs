using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.sdk.objects.widgets
{
  public class Widget
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Weight { get; set; }

    public LengthUOM LengthUOM { get; set; }

    public WeightUOM WeightUOM { get; set; }

    public string Material { get; set; }

    public Color Color { get; set; }
  }

  public enum LengthUOM
  {
    meter,
    inch,
    feet,
    milimeter,
  }

  public enum WeightUOM
  {
    pounds,
    grams,
    tons,
    kilograms,
    metricTons,
  }

  public enum Color
  {
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Violet,
    Black,
    White,
    Gray,
  }
}
