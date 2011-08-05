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

    //private Expression<Func<Widget, bool>> ToPredicate(List<Filter> filters)
    //{
    //  Expression<Func<Widget, bool>> predicate = null;

    //  try
    //  {
    //    foreach (Filter filter in filters)
    //    {
    //      LogicalOperators logicalOperator = LogicalOperators.none;
    //      Enum.TryParse<LogicalOperators>(filter.Logical, out logicalOperator);

    //      switch (logicalOperator)
    //      {
    //        case LogicalOperators.and:
    //        case LogicalOperators.none:
    //          if (predicate == null)
    //            predicate = PredicateBuilder.True<Widget>();
    //          predicate = predicate.And(ResolvePredicate(filter));
    //          break;

    //        case LogicalOperator.AndNot:
    //        case LogicalOperator.Not:
    //          if (predicate == null)
    //            predicate = PredicateBuilder.True<IDataObject>();
    //          predicate = predicate.And(ResolvePredicate(expression));
    //          predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
    //          break;

    //        case LogicalOperator.Or:
    //          if (predicate == null)
    //            predicate = PredicateBuilder.False<IDataObject>();
    //          predicate = predicate.Or(ResolvePredicate(filter));
    //          break;

    //        case LogicalOperator.OrNot:
    //          if (predicate == null)
    //            predicate = PredicateBuilder.False<IDataObject>();
    //          predicate = predicate.Or(ResolvePredicate(expression));
    //          predicate = LINQ.Expression.Lambda<Func<IDataObject, bool>>(LINQ.Expression.Not(predicate.Body), predicate.Parameters[0]);
    //          break;
    //      }

    //      if (groupLevel > 0)
    //      {
    //        predicate = predicate.And(ToPredicate(groupLevel));
    //      }
    //    }

    //    if (predicate == null)
    //      predicate = PredicateBuilder.True<IDataObject>();

    //    return predicate;
    //  }
    //  catch (Exception ex)
    //  {
    //    throw new Exception("Error while generating Predicate.", ex);
    //  }
    //}


    //private Expression<Func<Widget, bool>> ResolvePredicate(Filter filter)
    //{
    //   Expression<Func<Widget, bool>> predicate = null;

    //  string propertyName = filter.AttributeName;

    //  RelationalOperators relationalOperator = RelationalOperators.equalTo;

    //  Enum.TryParse<RelationalOperators>(filter.RelationalOperator, out relationalOperator);

    //  string attributeName = filter.AttributeName;

    //  switch (filter.AttributeName.ToUpper())
    //  {
    //    case "NAME":
    //      predicate = ResolveStringAttribute("Name", relationalOperator, filter.Value);

    //    case "DESCRIPTION":
    //      predicate = ResolveStringAttribute("Description", relationalOperator, filter.Value);

    //    case "LENGTH":
    //      predicate = ResolveNumericalAttribute("Length", relationalOperator, filter.Value);

    //    case "WIDTH":
    //      predicate = ResolveNumericalAttribute("Width", relationalOperator, filter.Value);

    //    case "HEIGHT":
    //      predicate = ResolveNumericalAttribute("Height", relationalOperator, filter.Value);

    //    case "WEIGHT":
    //      predicate = ResolveNumericalAttribute("Weight", relationalOperator, filter.Value);

    //    case "UOMLENGTH":
    //      predicate = ResolveUomLenthAttribute("LengthUOM", relationalOperator, filter.Value);

    //    case "UOMWEIGHT":
    //      predicate = ResolveUomLenthAttribute("WeightUOM", relationalOperator, filter.Value);

    //    case "MATERIAL":
    //      predicate = ResolveStringAttribute("Material", relationalOperator, filter.Value);

    //    case "COLOR":
    //      predicate = ResolveColorAttribute("Color", relationalOperator, filter.Value);

    //  }

    //  return predicate;
    //}

    //private Expression<Func<Widget, bool>> ResolveStringAttribute(string attributeName, RelationalOperators relationalOperator, string value)
    //{
    //  Expression<Func<Widget, bool>> predicate = null;

    //  switch (relationalOperator)
    //  {
    //    case RelationalOperators.contains:
    //      return o => o.Name.ToUpper().Contains(value.ToUpper());

    //    case RelationalOperators.@in:

    //      string[] values = value.Split(", ");

    //      return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString());

    //    case RelationalOperator.EqualTo:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
    //      }
    //      else
    //      {
    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
    //      }

    //    case RelationalOperator.NotEqualTo:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => !o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
    //      }
    //      else
    //      {
    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => !comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
    //      }

    //    case RelationalOperator.GreaterThan:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1;
    //      }

    //    case RelationalOperator.GreaterThanOrEqual:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1 ||
    //                    o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1 ||
    //                    comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
    //      }

    //    case RelationalOperator.LesserThan:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("LesserThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1;
    //      }

    //    case RelationalOperator.LesserThanOrEqual:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1 ||
    //                    o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1 ||
    //                    comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
    //      }

    //    default:
    //      throw new Exception("Relational operator does not exist.");
    //  }

    //  return predicate;
    //}
    //  //

    //  switch (relationalOperator)
    //  {
    //    case RelationalOperators.contains:
    //      if (!isString) throw new Exception("Contains operator used with non-string property");


    //      return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().Contains(expression.Values.FirstOrDefault().ToUpper());


    //    case RelationalOperator.EndsWith:
    //      if (!isString) throw new Exception("EndsWith operator used with non-string property");

    //      if (expression.IsCaseSensitive)
    //      {
    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().EndsWith(expression.Values.FirstOrDefault());
    //      }
    //      else
    //      {
    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().ToUpper().EndsWith(expression.Values.FirstOrDefault().ToUpper());
    //      }

    //    case RelationalOperator.In:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString());
    //      }
    //      else
    //      {
    //        return o => expression.Values.Contains(o.GetPropertyValue(dataProperty.propertyName).ToString(), new GenericDataComparer(propertyType));
    //      }

    //    case RelationalOperator.EqualTo:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
    //      }
    //      else
    //      {
    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
    //      }

    //    case RelationalOperator.NotEqualTo:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => !o.GetPropertyValue(dataProperty.propertyName).ToString().Equals(expression.Values.FirstOrDefault());
    //      }
    //      else
    //      {
    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => !comparer.Equals(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault());
    //      }

    //    case RelationalOperator.GreaterThan:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1;
    //      }

    //    case RelationalOperator.GreaterThanOrEqual:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 1 ||
    //                    o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 1 ||
    //                    comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
    //      }

    //    case RelationalOperator.LesserThan:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("LesserThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1;
    //      }

    //    case RelationalOperator.LesserThanOrEqual:
    //      if (expression.IsCaseSensitive)
    //      {
    //        if (!isString) throw new Exception("Case Sensitivity is not available with this operator and propertyType.");

    //        return o => o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == -1 ||
    //                    o.GetPropertyValue(dataProperty.propertyName).ToString().CompareTo(expression.Values.FirstOrDefault()) == 0;
    //      }
    //      else
    //      {
    //        if (!isBoolean) throw new Exception("GreaterThan operator cannot be used with Boolean property");

    //        GenericDataComparer comparer = new GenericDataComparer(propertyType);
    //        return o => comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == -1 ||
    //                    comparer.Compare(o.GetPropertyValue(dataProperty.propertyName).ToString(), expression.Values.FirstOrDefault()) == 0;
    //      }

    //    default:
    //      throw new Exception("Relational operator does not exist.");
    //  }
    //}
  }
}
