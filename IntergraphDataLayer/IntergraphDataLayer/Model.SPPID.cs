﻿using System;
using System.Xml.Linq;

namespace Bechtel.IntergraphDataLayer
{
  public abstract class IntergraphObject
  {
    public string CommodityName { get; set; }

    public abstract string ItemTag { get; set; }

    public string GetKeysXml()
    {
      XElement keysXml = new XElement("root",
        new XElement("attr",
          new XAttribute("name", "ItemTag"),
            new XAttribute("value", this.ItemTag)));
      return keysXml.ToString();
    }

    public abstract string GetUpdatesXml();
  }
}

namespace Bechtel.IntergraphDataLayer.SPPID
{  
  public class Equipment : IntergraphObject
  {
    public Equipment()
    {
      CommodityName = "Equipment";
    }

    public override string ItemTag { get; set; }

    public string Description { get; set; }

    public string InsulType { get; set; }

    public string InsulPurpose { get; set; }

    public string InsulThick { get; set; }

    public override string GetUpdatesXml()
    {
      XElement updatesXml = new XElement("root");

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "ItemTag"),
          new XAttribute("value", this.ItemTag)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "Description"),
          new XAttribute("value", this.Description)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulType"),
          new XAttribute("value", this.InsulType)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulPurpose"),
          new XAttribute("value", this.InsulPurpose)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulThick"),
          new XAttribute("value", this.InsulThick)));

      return updatesXml.ToString();
    }
  }

  public class Instrument : IntergraphObject
  {
    public Instrument()
    {
      CommodityName = "Instrument";
    }

    public override string ItemTag { get; set; }

    public string NominalDiameter { get; set; }

    public string Description { get; set; }

    public string InsulType { get; set; }

    public string InsulPurpose { get; set; }

    public string InsulThick { get; set; }

    public override string GetUpdatesXml()
    {
      XElement updatesXml = new XElement("root");

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "ItemTag"),
          new XAttribute("value", this.ItemTag)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "NominalDiameter"),
          new XAttribute("value", this.NominalDiameter)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "Description"),
          new XAttribute("value", this.Description)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulType"),
          new XAttribute("value", this.InsulType)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulPurpose"),
          new XAttribute("value", this.InsulPurpose)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulThick"),
          new XAttribute("value", this.InsulThick)));

      return updatesXml.ToString();
    }
  }

  public class PipeRun : IntergraphObject
  {
    public PipeRun()
    {
      CommodityName = "PipeRun";
    }

    public override string ItemTag { get; set; }

    public string Description { get; set; }

    public string NominalDiameter { get; set; }

    public string InsulType { get; set; }

    public string InsulPurpose { get; set; }

    public string InsulThick { get; set; }

    public string OperFluidCode { get; set; }

    public string PipingMaterialsClass { get; set; }

    public override string GetUpdatesXml()
    {
      String xml = String.Empty;

      if (this.ItemTag != null)
      {
        XElement updatesXml = new XElement("root");

        updatesXml.Add(new XElement("attr",
          new XAttribute("name", "ItemTag"),
            new XAttribute("value", this.ItemTag)));

        if (this.Description != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "Description"),
              new XAttribute("value", (this.Description))));
        }

        if (this.NominalDiameter != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "NominalDiameter"),
              new XAttribute("value", this.NominalDiameter)));
        }

        if (this.InsulType != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "InsulType"),
              new XAttribute("value", (this.InsulType))));
        }

        if (this.InsulPurpose != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "InsulPurpose"),
              new XAttribute("value", this.InsulPurpose)));
        }

        if (this.InsulThick != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "InsulThick"),
              new XAttribute("value", this.InsulThick)));
        }

        if (this.OperFluidCode != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "OperFluidCode"),
              new XAttribute("value", this.OperFluidCode)));
        }

        if (this.PipingMaterialsClass != null)
        {
          updatesXml.Add(new XElement("attr",
            new XAttribute("name", "PipingMaterialsClass"),
              new XAttribute("value", this.PipingMaterialsClass)));
        }

        xml = updatesXml.ToString();
      }

      return xml;
    }
  }

  public class PipingComp : IntergraphObject
  {
    public PipingComp()
    {
      CommodityName = "PipingComp";
    }

    public override string ItemTag { get; set; }

    public string NominalDiameter { get; set; }

    public string Description { get; set; }

    public string InsulType { get; set; }

    public string InsulPurpose { get; set; }

    public string InsulThick { get; set; }

    public override string GetUpdatesXml()
    {
      XElement updatesXml = new XElement("root");

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "ItemTag"),
          new XAttribute("value", this.ItemTag)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "NominalDiameter"),
          new XAttribute("value", this.NominalDiameter)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "Description"),
          new XAttribute("value", this.Description)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulType"),
          new XAttribute("value", this.InsulType)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulPurpose"),
          new XAttribute("value", this.InsulPurpose)));

      updatesXml.Add(new XElement("attr",
        new XAttribute("name", "InsulThick"),
          new XAttribute("value", this.InsulThick)));

      return updatesXml.ToString();
    }
  }
}
