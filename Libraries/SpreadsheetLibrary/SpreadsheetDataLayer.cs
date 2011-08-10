﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using log4net;
using Ninject;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using org.iringtools.adapter.datalayer;

namespace org.iringtools.adapter.datalayer
{


  public class SpreadsheetDataLayer : BaseDataLayer
  {
    private SpreadsheetProvider _provider = null;
    private List<IDataObject> _dataObjects = null;
    private ILog _logger = LogManager.GetLogger(typeof(SpreadsheetDataLayer));

    [Inject]
    public SpreadsheetDataLayer(AdapterSettings settings)
      : base(settings)
    {      
      _provider = new SpreadsheetProvider(settings);     
    }

    public override DataDictionary GetDictionary()
    {
      try
      {
        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()
        };

        foreach (SpreadsheetTable table in _provider.GetConfiguration().Tables)
        {
          DataObject dataObject = new DataObject()
          {
            objectName = table.Label,
            tableName = table.Name,
            dataProperties = new List<DataProperty>()
          };

          dataDictionary.dataObjects.Add(dataObject);

          foreach (SpreadsheetColumn column in table.Columns)
          {
            DataProperty dataProperty = new DataProperty()
            {
              propertyName = column.Label,
              columnName = column.Name,
              dataType = column.DataType
            };

            if (table.Identifier.Equals(column.Label))
            {
              dataObject.addKeyProperty(dataProperty);
            }
            else
            {
              dataObject.dataProperties.Add(dataProperty);
            }
          }
        }

        return dataDictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
      finally
      {
        _provider.Dispose();
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      try
      {
        LoadDataDictionary(objectType);

        IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

        var expressions = FormMultipleKeysPredicate(identifiers);

        if (expressions != null)
        {
          _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
        }

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      try
      {
        LoadDataDictionary(objectType);

        IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

        // Apply filter
        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          var predicate = filter.ToPredicate(_dataObjectDefinition);

          if (predicate != null)
          {
            _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
          }
        }

        if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
        {
          throw new NotImplementedException("OrderExpressions are not supported by the CSV DataLayer.");
        }

        //Page and Sort The Data
        if (pageSize > _dataObjects.Count())
          pageSize = _dataObjects.Count();
        _dataObjects = _dataObjects.GetRange(startIndex, pageSize);

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);

        throw new Exception(
          "Error while getting a list of data objects of type [" + objectType + "].",
          ex
        );
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        return dataObjects.Count();
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a count of type [" + objectType + "].",
          ex
        );
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();

        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a list of identifiers of type [" + objectType + "].",
          ex
        );
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      string objectType = String.Empty;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;

        LoadDataDictionary(objectType);

        //IList<IDataObject> existingDataObjects = LoadDataObjects(objectType);

        //foreach (IDataObject dataObject in dataObjects)
        //{
        //  IDataObject existingDataObject = null;

        //  string identifier = GetIdentifier(dataObject);
        //  var predicate = FormKeyPredicate(identifier);

        //  if (predicate != null)
        //  {
        //    existingDataObject = existingDataObjects.AsQueryable().Where(predicate).FirstOrDefault();
        //  }

        //  if (existingDataObject != null)
        //  {
        //    existingDataObjects.Remove(existingDataObject);
        //  }

        //  //TODO: Should this be per property?  Will it matter?
        //  existingDataObjects.Add(dataObject);
        //}

        //response = SaveDataObjects(objectType, existingDataObjects);

        response = SaveDataObjects(objectType, dataObjects);

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);

        throw new Exception(
          "Error while posting dataObjects of type [" + objectType + "].",
          ex
        );
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }

      SpreadsheetTable cftable = _provider.GetConfigurationTable(objectType);
      SpreadsheetReference tableReference = cftable.GetReference();

      WorksheetPart worksheetPart = _provider.GetWorksheetPart(tableReference.SheetName);
      SpreadsheetColumn column = cftable.Columns.First<SpreadsheetColumn>(c => cftable.Identifier.Equals(c.Name));

      IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>();

      foreach (string identifier in identifiers)
      {
        Status status = new Status();
        status.Identifier = identifier;

        try
        {
          foreach (Row row in rows)
          {
            Cell cell = row.Descendants<Cell>().First(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(column.ColumnIdx));

            if (_provider.GetValue(cell).Equals(identifier))
            {
              row.Remove();

              string message = String.Format(
                "DataObject [{0}] deleted successfully.",
                identifier
              );

              status.Messages.Add(message);
            }
          }

        }
        catch (Exception ex)
        {
          _logger.Error("Error in Delete: " + ex);

          status.Level = StatusLevel.Error;

          string message = String.Format(
            "Error while deleting dataObject [{0}]. {1}",
            identifier,
            ex
          );

          status.Messages.Add(message);
        }

        response.Append(status);
      }

      rows = worksheetPart.Worksheet.Descendants<Row>().OrderBy(r => r.RowIndex.Value);

      uint i = 1;
      foreach (Row row in rows)
      {
        row.RowIndex.Value = i++;
      }

      tableReference.EndRow = --i;
            
      worksheetPart.Worksheet.SheetDimension.Reference = tableReference.GetReference(false);
      cftable.Reference = tableReference.GetReference(true);
      worksheetPart.Worksheet.Save();
      
      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        IList<string> identifiers = new List<string>();

        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);

        throw new Exception(
          "Error while deleting data objects of type [" + objectType + "].",
          ex
        );
      }
    }

    private IList<IDataObject> LoadDataObjects(string objectType)
    {
      try
      {
        IList<IDataObject> dataObjects = new List<IDataObject>();

        SpreadsheetTable cfTable = _provider.GetConfigurationTable(objectType);
        SpreadsheetReference tableReference = cfTable.GetReference();

        WorksheetPart worksheetPart = _provider.GetWorksheetPart(tableReference.SheetName);

        IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex > tableReference.StartRow && r.RowIndex <= tableReference.EndRow);

        foreach(Row row in rows)
        {
          IDataObject dataObject = new GenericDataObject
          {
            ObjectType = objectType,
          };

          foreach(Cell col in row.ChildElements) {

            string columnIdx = SpreadsheetReference.GetColumnName(col.CellReference);
            SpreadsheetColumn column = cfTable.Columns.First<SpreadsheetColumn>(c => columnIdx.Equals(c.ColumnIdx));

            if (column != null)
            {
              dataObject.SetPropertyValue(column.Name, _provider.GetValue(col));
            }
          }

          dataObjects.Add(dataObject);
        }
        
        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in LoadDataObjects: " + ex);
        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
      }
    }

    private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
    {
      try
      {
        Response response = new Response();

        SpreadsheetTable cfTable = _provider.GetConfigurationTable(objectType);
        SpreadsheetReference tableReference = cfTable.GetReference();
        WorksheetPart worksheetPart = _provider.GetWorksheetPart(cfTable);

        foreach (IDataObject dataObject in dataObjects)
        {
          Status status = new Status();

          try
          {
            string identifier = GetIdentifier(dataObject);
            status.Identifier = identifier;

            SpreadsheetColumn column = cfTable.Columns.First<SpreadsheetColumn>(c => c.Name.Equals(cfTable.Identifier));
            Cell cell = worksheetPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(column.ColumnIdx) && _provider.GetValue(c).Equals(identifier));

            if (cell != null)
            {
              Row existingRow = cell.Ancestors<Row>().First();

              foreach (SpreadsheetColumn col in cfTable.Columns)
              {
                Cell existingCell = existingRow.Descendants<Cell>().First(c => SpreadsheetReference.GetColumnName(c.CellReference).Equals(col.ColumnIdx));
                existingCell.DataType = SpreadsheetProvider.GetCellValue(col.DataType);
                existingCell.CellValue.Text = Convert.ToString(dataObject.GetPropertyValue(col.Name));              
              }
            }
            else
            {
              tableReference.EndRow++;

              Row newRow = new Row
              {
                RowIndex = (UInt32Value)tableReference.EndRow,
                Spans = new ListValue<StringValue> 
                { 
                  InnerText = string.Format("1:{0}", cfTable.Columns.Count)
                }
              };

              foreach (SpreadsheetColumn col in cfTable.Columns)
              {
                Cell newCell = new Cell
                {
                  CellReference = string.Format("{0}{1}", col.ColumnIdx, newRow.RowIndex),
                  DataType = SpreadsheetProvider.GetCellValue(col.DataType),
                  CellValue = new CellValue(Convert.ToString(dataObject.GetPropertyValue(col.Name)))
                };

                newRow.Append(newCell);
              }

              SheetData sheetData = (SheetData)worksheetPart.Worksheet.Descendants<SheetData>().First();
              sheetData.Append(newRow);
            }

            worksheetPart.Worksheet.SheetDimension.Reference = tableReference.GetReference(false);
            cfTable.Reference = tableReference.GetReference(true);

            worksheetPart.Worksheet.Save();

            status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
          }
          catch (Exception ex)
          {
            status.Level = StatusLevel.Error;

            string message = String.Format(
              "Error while posting dataObject [{0}]. {1}",
              dataObject.GetPropertyValue("Tag"),
              ex.ToString()
            );

            status.Messages.Add(message);
          }

          response.Append(status);
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in LoadDataObjects: " + ex);
        throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
      }
    }    
  }
}
