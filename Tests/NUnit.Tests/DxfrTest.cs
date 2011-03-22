using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using log4net;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.dxfr.manifest;
using org.iringtools.library;
using org.iringtools.utility;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.adapter.identity;
using org.iringtools.nhibernate;

namespace NUnit.Tests
{
    [TestFixture]
    public class DxfrTest
    {
	    private static readonly ILog _logger = LogManager.GetLogger(typeof(DxfrTest));      
      private AdapterSettings _settings = null;
      private string _baseDirectory = string.Empty;
      private DataTranferProvider _dxfrProvider = null;
			private IDataLayer _dataLayer = null;
			private IList<IDataObject> dataObjects = null;			

      public DxfrTest()
      {
				setSettings();
				_baseDirectory = Directory.GetCurrentDirectory();
				_baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
				_settings["BaseDirectoryPath"] = _baseDirectory;
				Directory.SetCurrentDirectory(_baseDirectory);
        _dxfrProvider = new DataTranferProvider(ConfigurationManager.AppSettings);
      }

			private void setSettings()
			{
				_settings = new AdapterSettings();
				_settings.AppendSettings(ConfigurationManager.AppSettings);

				_settings["ProjectName"] = "12345_000";
				_settings["ApplicationName"] = "ABC";
				_settings["GraphName"] = "Lines";
				_settings["Identifier"] = "90002-RV";
				_settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles
				_settings["ExecutingAssemblyName"] = "NUnit.Tests";
				_settings["GraphBaseUri"] = "http://www.example.com/";
			}

			private XDocument ToXml<T>(T dataList)
			{
				XDocument xDocument = null;

				try
				{
					string xml = Utility.SerializeDataContract<T>(dataList);
					XElement xElement = XElement.Parse(xml);
					xDocument = new XDocument(xElement);
				}
				catch (Exception ex)
				{
					_logger.Error("Error transferring data list to xml." + ex);
				}

				return xDocument;
			}

			private void deleteHashValue(ref DataTransferIndices dtiList)
			{
				foreach (DataTransferIndex dti in dtiList.DataTransferIndexList)
				{
					dti.HashValue = null;
				}
			}			

      [Test]
      public void GetDataTransferIndices()
      {
        XDocument benchmark = null;
        DataFilter filter = new DataFilter();
        filter.Expressions = null;
        filter.OrderExpressions = null;
        DataTransferIndices dtiList = null;

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithFilter(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], null, filter);

				deleteHashValue(ref dtiList);

        string path = String.Format(
          "{0}DxfrGetDataTransferIndices.xml",
            _settings["XmlPath"]
            );

        benchmark = XDocument.Load(path);
				String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
				String benchmarkString = benchmark.ToString();

				Assert.AreEqual(dtiListString, benchmarkString);
      }

      [Test]
      public void GetDataTransferIndicesWithFilter()
      {
        XDocument benchmark = null;
        DataFilter filter = new DataFilter();
        filter.Expressions.Add(
          new Expression
          {
            PropertyName = "PipingNetworkSystem.NominalDiameter.valValue",
            Values = new Values
            {
              "40"
            },
            RelationalOperator = RelationalOperator.EqualTo
          }
        );

        filter.OrderExpressions.Add(
          new OrderExpression
          {
            PropertyName = "PipingNetworkSystem.IdentificationByTag.valIdentifier",
            SortOrder = SortOrder.Asc
          }
        );

        DataTransferIndices dtiList = null;

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithFilter(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], null, filter);

				deleteHashValue(ref dtiList);

        string path = String.Format(
          "{0}DxfrGetDataTransferIndicesWithFilter.xml",
            _settings["XmlPath"]
            );

        benchmark = XDocument.Load(path);
				String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(dtiListString, benchmarkString);
      }

      [Test]
      public void GetManifest()
      {
        XDocument benchmark = null;           
        Manifest manifest = null;

				manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);
            
        string path = String.Format(
          "{0}DxfrGetManifest.xml",
            _settings["XmlPath"]
            );

				String manifestString = ToXml(manifest).ToString();
        benchmark = XDocument.Load(path);				
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(manifestString, benchmarkString);
      }


      [Test]
      public void GetDataTransferIndicesWithManifest()
      {
				XDocument benchmark = null;
        DataFilter filter = new DataFilter();
        filter.Expressions = null;
        filter.OrderExpressions = null;
        DataTransferIndices dtiList = null;
        Manifest manifest = null;

				manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithManifest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", manifest);

				deleteHashValue(ref dtiList);

        string path = String.Format(
          "{0}DxfrGetDataTransferIndicesWithManifest.xml",
            _settings["XmlPath"]
            );

        benchmark = XDocument.Load(path);

				String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(dtiListString, benchmarkString);
      }

      [Test]
      public void GetDataTransferIndicesWithDxiRequest()
      {
        XDocument benchmark = null;
        DxiRequest dxiRequest = new DxiRequest();
        dxiRequest.DataFilter = new DataFilter();          
        DataTransferIndices dtiList = null;       

        dxiRequest.DataFilter.Expressions.Add(
          new Expression
            {
              PropertyName = "PipingNetworkSystem.NominalDiameter.valValue",
              Values = new Values
                {
                  "80"
                },
              RelationalOperator = RelationalOperator.EqualTo
            }
            );

        dxiRequest.DataFilter.OrderExpressions.Add(
          new OrderExpression
          {
            PropertyName = "PipingNetworkSystem.IdentificationByTag.valIdentifier",
            SortOrder = SortOrder.Asc
          }
        );

				dxiRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesByRequest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", dxiRequest);
				
				deleteHashValue(ref dtiList);

        string path = String.Format(
            "{0}DxfrGetDataTransferIndicesByRequest.xml",
            _settings["XmlPath"]
          );

        benchmark = XDocument.Load(path);
				String dtiListString = ToXml(dtiList.DataTransferIndexList).ToString();
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(dtiListString, benchmarkString);
      }

      [Test]
      public void GetDataTransferObjects()
      {
        XDocument benchmark = null;
        DataTransferIndices dtiList = null, dtiPage = new DataTransferIndices();
        DataTransferObjects dtos = null;
        Manifest manifest = null;          
        int page = 25;

				manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithManifest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", manifest);

        dtiPage.DataTransferIndexList = dtiList.DataTransferIndexList.GetRange(0, page);


        dtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], dtiPage);

        string path = String.Format(
            "{0}DxfrGetDataTransferObjects.xml",
            _settings["XmlPath"]
          );

        benchmark = XDocument.Load(path);
				String dtosString = ToXml(dtos.DataTransferObjectList).ToString();
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(dtosString, benchmarkString);
      }

      [Test]
      public void GetDataTransferObjectsWithDxoRequest()
      {				
        XDocument benchmark = null;
        DataTransferIndices dtiList = null;
        DataTransferObjects dtos = null;
            
        DxoRequest dxoRequest = new DxoRequest();
				dxoRequest.DataTransferIndices = new DataTransferIndices();
        int page = 25;

				dxoRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        dtiList =
          _dxfrProvider.GetDataTransferIndicesWithManifest(
            _settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], "MD5", dxoRequest.Manifest);

        dxoRequest.DataTransferIndices.DataTransferIndexList = dtiList.DataTransferIndexList.GetRange(0, page);

        dtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
            _settings["GraphName"], dxoRequest);

        string path = String.Format(
            "{0}DxfrGetDataTransferObjectsWithDxoRequest.xml",
            _settings["XmlPath"]
          );

        benchmark = XDocument.Load(path);
				String dtosString = ToXml(dtos.DataTransferObjectList).ToString();
				String benchmarkString = benchmark.ToString();
				Assert.AreEqual(dtosString, benchmarkString);
      }

			private IKernel prepareKernel()
			{
				IKernel _kernel = null;
				Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

				string relativePath = String.Format(
					"{0}BindingConfiguration.12345_000.ABC.xml",
					_settings["XmlPath"]
					);

				string bindingConfigurationPath = Path.Combine(
					_settings["BaseDirectoryPath"],
					relativePath
					 );

				var ninjectSettings = new NinjectSettings { LoadExtensions = false };
				_kernel = new StandardKernel(ninjectSettings, new AdapterModule());
				_kernel.Load(new XmlExtensionModule());
				_settings = _kernel.Get<AdapterSettings>();
				_kernel.Load(bindingConfigurationPath);

				relativePath = String.Format("{0}BindingConfiguration.Adapter.xml",
						_settings["XmlPath"]
					);

				bindingConfigurationPath = Path.Combine(
					_settings["BaseDirectoryPath"],
					relativePath
				);

				_kernel.Load(bindingConfigurationPath);
				IIdentityLayer _identityLayer = _kernel.Get<IIdentityLayer>("IdentityLayer");
				IDictionary _keyRing = _identityLayer.GetKeyRing();
				_kernel.Bind<IDictionary>().ToConstant(_keyRing).Named("KeyRing");

				if (_keyRing.Count > 0)
				{
					if (_keyRing["Provider"].ToString() == "WindowsAuthenticationProvider")
					{
						string userName = _keyRing["Name"].ToString();
						_settings["UserName"] = userName;
					}
				}				
				return _kernel;
			}

			/* Copy initial state of the table for ABC */
			private void Initialize()
			{
				IKernel _kernel = prepareKernel();
				_settings["Scope"] = "12345_000.ABC";

				try
				{					
					_dataLayer = _kernel.Get<IDataLayer>("DataLayer");
				}
				catch (Exception ex)
				{
					string message = "Error initializing datalayer: " + ex;
					_logger.Error(message);
					throw new Exception(message);
				}

				dataObjects = _dataLayer.Get("LINES", null);
			}
			/*
			[Test]
			public void PostDataTransferObjects()
			{
				XDocument benchmark = null;
				Response response = null;				
				DxoRequest dxoRequest = new DxoRequest();
				DataTransferObjects postDtos = null;
				List<DataTransferObject> dtoList = null;

				// Copy initial state of the table for ABC 
			  Initialize();

				setSettings();   
				dxoRequest.Manifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

				dxoRequest.DataTransferIndices = new DataTransferIndices();

				dxoRequest.DataTransferIndices =
					_dxfrProvider.GetDataTransferIndicesWithManifest(
						_settings["ProjectName"], _settings["ApplicationName"],
						_settings["GraphName"], "MD5", dxoRequest.Manifest);

				postDtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
						_settings["GraphName"], dxoRequest);				

				dtoList = postDtos.DataTransferObjectList;

				dtoList[0].transferType = TransferType.Delete;
				dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].oldValue = dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].value; 
				dtoList[1].classObjects[1].templateObjects[0].roleObjects[2].value = "200";
				
				string path = String.Format(
						"{0}DxfrNewDto.xml",
						_settings["XmlPath"]
					);
				benchmark = XDocument.Load(path);

				DataTransferObject newDto = Utility.DeserializeDataContract<DataTransferObject>(benchmark.ToString());

				dtoList.Add(newDto);				

				response = _dxfrProvider.PostDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
						_settings["GraphName"], postDtos);

				path = String.Format(
						"{0}DxfrRespones.xml",
						_settings["XmlPath"]
					);

				benchmark = XDocument.Load(path);

				String res = ToXml(response).ToString();
				Response xmlResponse = Utility.DeserializeDataContract<Response>(benchmark.ToString());
				
				Assert.AreEqual(response.Level.ToString(), xmlResponse.Level.ToString());
				foreach (Status status in response.StatusList)
					foreach (Status xmlStatus in xmlResponse.StatusList)
					{
						Assert.AreEqual(status.Messages.ToString(), xmlStatus.Messages.ToString());
						Assert.AreEqual(status.Identifier, xmlStatus.Identifier);
						xmlResponse.StatusList.Remove(xmlStatus);
						break;
					}

				//restore the table
				_dataLayer.Post(dataObjects);				
			}
*/
    }
}

