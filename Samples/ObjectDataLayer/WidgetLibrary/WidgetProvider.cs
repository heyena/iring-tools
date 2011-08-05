using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using org.iringtools.utility;
using System.Reflection;

namespace org.iringtools.sdk.objects.widgets
{
  public class WidgetProvider
  {
    private List<Widget> _repository = null;
    private string _fileName = "WidgetsStore.xml";

    public WidgetProvider()
    {
      _repository = Initialize();
    }

    private void CreateWidget(Widget widget)
    {
      var ids = from w in _repository
                  select w.Id;

      int newId = ids.Max() + 1;

      widget.Id = newId;

      _repository.Add(widget);
    }

    public Widget ReadWidget(int identifier)
    {
      var widgets = from widget in _repository
                    where widget.Id == identifier
                    select widget;

      return widgets.FirstOrDefault();
    }

    public List<Widget> ReadWidgets(List<Filter> filters)
    {
      //TODO: Need to implement filtering in Provider.
      return _repository;
    }

    public int UpdateWidgets(List<Widget> widgets)
    {
      try
      {
        foreach (Widget widget in widgets)
        {
          Widget existingWidget = (from w in _repository
                                   where w.Id == widget.Id
                                   select w).FirstOrDefault();

          if (existingWidget != null)
          {
            _repository.Remove(existingWidget);
            _repository.Add(widget);
          }
          else
          {
            CreateWidget(widget);
          }
        }

        Save();

        return 0;
      }
      catch (Exception ex)
      {
        return 1;
      }
    }

    public int DeleteWidgets(int identifier)
    {
      try
      {
        Widget existingWidget = (from w in _repository
                                 where w.Id == identifier
                                 select w).FirstOrDefault();

        _repository.Remove(existingWidget);

        Save();

        return 0;
      }
      catch (Exception ex)
      {
        return 1;
      }
    }

    public int DeleteWidgets(Filter filter)
    {
      return 0;
    }

    private List<Widget> Initialize()
    {
      List<Widget> widgets = new List<Widget>();

      if (File.Exists(_fileName))
      {
        widgets = Utility.Read<List<Widget>>(_fileName, true);
      }
      else
      {
        widgets = new List<Widget>
        {
          new Widget
          {
            Id = 1,
            Name = "Thing1",
            Description = "Sample Object 1",
            Color = Color.Orange,
            Material = "Polyoxymethylene",
            Length = 3.14,
            Height = 4.0,
            Width = 5.25,
            LengthUOM = LengthUOM.inch,
            Weight = 10,
            WeightUOM = WeightUOM.pounds
          },
          new Widget
          {
            Id = 2,
            Name = "Thing2",
            Description = "Sample Object 2",
            Color = Color.Blue,
            Material = "Polyoxymethylene",
            Length = 6.14,
            Height = 10.0,
            Width = 19.25,
            LengthUOM = LengthUOM.milimeter,
            Weight = 15,
            WeightUOM = WeightUOM.kilograms
          },
          new Widget
          {
            Id = 2,
            Name = "Thing3",
            Description = "Sample Object 3",
            Color = Color.Red,
            Material = "Polyoxymethylene",
            Length = 8.14,
            Height = 12.0,
            Width = 25.25,
            LengthUOM = LengthUOM.feet,
            Weight = 150,
            WeightUOM = WeightUOM.pounds
          },
        };

        Utility.Write<List<Widget>>(widgets, _fileName, true);
      }

      return widgets;
    }

    private void Save()
    {
      Utility.Write<List<Widget>>(_repository, _fileName, true);
    }
  }
}
