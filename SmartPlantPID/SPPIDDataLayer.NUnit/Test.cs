using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ninject;
using Ninject.Extensions.Xml;

namespace iRINGTools.SDK.SPPIDDataLayer
{
    [TestFixture]
    public class SPPIDDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private IKernel _kernel = null;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private IDataLayer2 _sppidDataLayer;

        public SPPIDDataLayerTest()
        {
            // N inject magic
            var ninjectSettings = new NinjectSettings { LoadExtensions = false };
            _kernel = new StandardKernel(ninjectSettings);

            _kernel.Load(new XmlExtensionModule());

            _kernel.Bind<AdapterSettings>().ToSelf().InSingletonScope();
            _adapterSettings = _kernel.Get<AdapterSettings>();

            // Start with some generic settings
            _settings = new NameValueCollection();

            _settings["XmlPath"] = @".\12345_000\";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "SPPID";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings.AppendSettings(_settings);

            // Add our specific settings
            string appSettingsPath = String.Format("{0}12345_000.SPPID.config",
                _adapterSettings["XmlPath"]);

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            // and run the thing
            string relativePath = String.Format(@"{0}BindingConfiguration.{1}.{2}.xml",
                _settings["XmlPath"],
                _settings["ProjectName"],
                _settings["ApplicationName"]);

            // Ninject Extension requires fully qualified path.
            string bindingConfigurationPath = Path.Combine(
                _settings["BaseDirectoryPath"],
                relativePath);

            _kernel.Load(bindingConfigurationPath);

            _sppidDataLayer = _kernel.Get<IDataLayer2>();
        }

        [Test]
        public void Create()
        {
            IList<string> identifiers = new List<string>() { 
                // "Equip-001", 
                //"Equip-002",
                "Equip-003", 
                "Equip-004"
            };

            Random random = new Random();

            IList<IDataObject> dataObjects = _sppidDataLayer.Create("Equipment", identifiers);

            foreach (IDataObject dataObject in dataObjects)
            {
                dataObject.SetPropertyValue("Adapter_ParentTag", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_DateCreated", DateTime.Today);
                dataObject.SetPropertyValue("Drawing_Description", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_DocumentCategory", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_DocumentType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_DrawingNumber", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_ItemStatus", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Name", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Path", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Revision", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Template", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Title", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Drawing_Version", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Representation_InStockpile", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Symbol_FileName", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Symbol_XCoordinate", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("Symbol_YCoordinate", (double)random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_AbsorbedDuty", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Area", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_EquipmentOrientation", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_RatedDuty", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_ApprovalTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("EquipmentOther_Revision_ApprovedBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_Responsibility", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_RevisionNumber", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_StatusTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("EquipmentOther_Revision_StatusType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_StatusValue", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentOther_Revision_Text", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_AbsorbedDuty", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_AbsorbedDutyUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_CleaningReqmtsTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_CoatingReqmtsTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_CorrosionAllowanceTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_DraftType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_EquipmentOrientation", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_HeatTransferAreaPerUnit", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_HeatTransferRating", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_InsulationSpecTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_MaterialOfConstClassTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_MotorPowerPerFan", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_NumberofBays", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_NumberofBundles", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_NumberofFans", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_NumberofTubes", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_PipingMaterialsClassTube", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_PowerAbsorbedPerFan", random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_RatedDuty", random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_RatedDutyUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_ApprovalTimestamp", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_ApprovedBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_Responsibility", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_RevisionNumber", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_StatusTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("Exchanger_Revision_StatusType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_StatusValue", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_Revision_Text", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_ShellDiameter", random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_ShellDiameterUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_TEMA_Designation", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_TubeLength", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_TypeOfLouvers", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Exchanger_UnitWidth", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_CWPipingPlan", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_DifferentialPressure", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_DifferentialPressureUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_DriverRatedPower", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_DriverRatedPowerUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_ElectricalReqmt", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_MaterialOfConstClassInternal", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_MechRating", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_PowerAbsorbed", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_PowerConsumption", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_RatedCapacity", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_RatedCapacityUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_RatedDischargePressure", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_RatedSuctionPressure", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_ApprovalTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("Mechanical_Revision_ApprovedBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_Responsibility", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_RevisionNumber", DateTime.Today);
                dataObject.SetPropertyValue("Mechanical_Revision_StatusTimestamp", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_StatusType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_StatusValue", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_Revision_Text", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_SealPipingPlan", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Mechanical_TypeOfDriver", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_DiameterInternal", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_EquipmentOrientation", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LengthTanToTan", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LengthTanToTanUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LevelReference", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LevelReferenceUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelHigh", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelHighUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelHighHigh", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelHighHighUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelLow", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelLowUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelLowLow", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelLowLowUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelNormal", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelNormalUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelOverflow", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_LiquidLevelOverflowUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_MaterialOfConstClassInternal", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_ApprovalTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("Vessel_Revision_ApprovedBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_Responsibility", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_RevisionNumber", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_StatusTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("Vessel_Revision_StatusType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_StatusValue", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_Revision_Text", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_VolumeRating", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Vessel_VolumeRatingUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Class", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("CleaningReqmts", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("CoatingReqmts", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ConstructionBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ConstructionStatus", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("CorrosionAllowance", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Description", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("DesignBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ERPAssetNo", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentSubclass", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("EquipmentType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("FabricationCategory", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HTraceMedium", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HTraceMediumTemp", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HTraceReqmt", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Height", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HeightUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_ApprovalTimestamp", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_ApprovedBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_Responsibility", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_RevisionNumber", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_StatusTimestamp", DateTime.Today);
                dataObject.SetPropertyValue("HoldStatus_StatusType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_StatusValue", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_Text", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("HoldStatus_UpdateCount", (Int32)random.Next(2, 10));
                dataObject.SetPropertyValue("InsulDensity", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulPurpose", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulTemp", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulThick", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulThickUOM", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulationSpec", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InsulationThkSource", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("InventoryTag", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("IsBulkItem", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("IsUnchecked", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ItemStatus", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ItemTag", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ItemTypeName", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("MaterialOfConstClass", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("ModelItemType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Name", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("PartOfType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("PipingMaterialsClass", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("PlantItemType", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("RequisitionBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("RequisitionNo", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SP_ID", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("Slope", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SlopeRise", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SlopeRun", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SteamOutPressure", "45");
                dataObject.SetPropertyValue("SteamOutReqmt", "46");
                dataObject.SetPropertyValue("SteamOutTemperature", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("StressReliefReqmt", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("SupplyBy", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("TagPrefix", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("TagReqdFlag", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("TagSequenceNo", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("TagSuffix", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("TrimSpec", "PT-" + random.Next(2, 10));
                dataObject.SetPropertyValue("UpdateCount", random.Next(2, 10));
                dataObject.SetPropertyValue("aabbcc_code", "PT-" + random.Next(2, 10));
               // dataObject.SetPropertyValue("aabbcc_code1", "PT-" + random.Next(2, 10));

            }
            Response actual = _sppidDataLayer.Post(dataObjects);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        //[Test]
        //public void Read()
        //{
        //    IList<string> identifiers = new List<string>() 
        //    { 
        //        "Equip-001", 
        //        "Equip-002", 
        //        "Equip-003", 
        //        "Equip-004" 
        //    };

        //    IList<IDataObject> dataObjects = _sppidDataLayer.Get("Equipment", identifiers);

        //    if (!(dataObjects.Count() > 0))
        //    {
        //        throw new AssertionException("No Rows returned.");
        //    }

        //    foreach (IDataObject dataObject in dataObjects)
        //    {
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
        //    }
        //}

        //[Test]
        //public void ReadWithFilter()
        //{
        //    DataFilter dataFilter = new DataFilter
        //    {
        //        Expressions = new List<Expression>
        //        {
        //            new Expression
        //            {
        //                PropertyName = "PumpDriverType",
        //                RelationalOperator = RelationalOperator.EqualTo,
        //                Values = new Values
        //                {
        //                    "PDT-8",
        //                }
        //            }
        //        }
        //    };

        //    IList<IDataObject> dataObjects = _sppidDataLayer.Get("Equipment", dataFilter, 2, 0);

        //    if (!(dataObjects.Count() > 0))
        //    {
        //        throw new AssertionException("No Rows returned.");
        //    }

        //    Assert.AreEqual(dataObjects.Count(), 2);

        //    foreach (IDataObject dataObject in dataObjects)
        //    {
        //        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        //        Assert.AreEqual(dataObject.GetPropertyValue("PumpDriverType"), "PDT-8");
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        //        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
        //    }
        //}

        //[Test]
        //public void GetDictionary()
        //{
        //    DataDictionary dictionary = _sppidDataLayer.GetDictionary();

        //    Assert.IsNotNull(dictionary);

        //    string dictionaryPath = String.Format("{0}DataDictionary.xml",
        //          _adapterSettings["XmlPath"]
        //        );

        //    Utility.Write<DataDictionary>(dictionary, dictionaryPath, true);
        //}
    }
}
