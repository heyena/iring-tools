using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.dxfr.manifest;
using System.Collections.Generic;
using org.iringtools.mapping;
using org.iringtools.utility;
using log4net;

namespace NUnit.Tests
{
    [TestFixture]
    public class DxfrTest
    {
			private static readonly ILog _logger = LogManager.GetLogger(typeof(DxfrTest));
      private AdapterProvider _adapterProvider = null;
      private AdapterSettings _settings = null;
      private string _baseDirectory = string.Empty;
      private DataTranferProvider _dxfrProvider = null;

      public DxfrTest()
      {
        _settings = new AdapterSettings();
        _settings.AppendSettings(ConfigurationManager.AppSettings);

        //_settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
        _settings["ProjectName"] = "12345_000";
        _settings["ApplicationName"] = "ABC";
        _settings["GraphName"] = "Lines";
        _settings["Identifier"] = "90002-RV";
        _settings["TestMode"] = "UseFiles"; //UseFiles/WriteFiles

        _baseDirectory = Directory.GetCurrentDirectory();
        _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
        _settings["BaseDirectoryPath"] = _baseDirectory;
        Directory.SetCurrentDirectory(_baseDirectory);
        _settings["GraphBaseUri"] = "http://www.example.com/";

        _adapterProvider = new AdapterProvider(_settings);
        _dxfrProvider = new DataTranferProvider(ConfigurationManager.AppSettings);
      }

			private ClassTemplates getClassTemplates(List<ClassTemplates> classTemplatesList, String classId)
			{
				foreach (ClassTemplates classTemplates in classTemplatesList)
				{
					org.iringtools.dxfr.manifest.Class clazz = classTemplates.@class;

					if (clazz.id == classId)
					{
						return classTemplates;
					}
				}
    
				return null;
			}

			private Template getTemplate(List<Template> templates, String templateId)
			{
				foreach (Template template in templates)
				{
					if (template.id == templateId)
						return template;
				}
    
				return null;
			}

			private Role getRole(List<Role> roles, String roleId)
			{
				foreach (Role role in roles)
				{
					if (role.id == roleId)
						return role;
				}
    
				return null;
			}

      private Manifest getCrossedManifest() 
      {
        Manifest sourceManifest = null, targetManifest = null;       

        sourceManifest = _dxfrProvider.GetManifest(_settings["ProjectName"], _settings["ApplicationName"]);

        targetManifest = _dxfrProvider.GetManifest(_settings["ProjectName"], "DEF");
  
        Graph sourceGraph = sourceManifest.graphs[0];
        Graph targetGraph = targetManifest.graphs[0];

        if (sourceGraph.classTemplatesList != null && targetGraph.classTemplatesList != null)
        {
          ClassTemplatesList sourceClassTemplatesList = sourceGraph.classTemplatesList;
          ClassTemplatesList targetClassTemplatesList = targetGraph.classTemplatesList;
              
          for (int i = 0; i < targetClassTemplatesList.Count; i++)
          {
            org.iringtools.dxfr.manifest.Class targetClass = targetClassTemplatesList[i].@class;            
						ClassTemplates sourceClassTemplates = getClassTemplates(sourceClassTemplatesList, targetClass.id);
        
            if (sourceClassTemplates != null && sourceClassTemplates.templates != null)
            {
              Templates targetTemplates = targetClassTemplatesList[i].templates;
              Templates sourceTemplates = sourceClassTemplatesList[i].templates;
          
              for (int j = 0; j < targetTemplates.Count; j++)
              {
                Template targetTemplate = targetTemplates[j];
								Template sourceTemplate = getTemplate(sourceTemplates, targetTemplate.id);
            
                if (sourceTemplate == null)
                {
                  if (targetTemplate.transferOption == TransferOption.Required)
                  {
                    throw new Exception("Required template [" + targetTemplate.id + "] not found");
                  }
                  else
                  {
                    targetTemplates.RemoveAt(j--);
                  }
                }
                else if (targetTemplate.roles != null && sourceTemplate.roles != null)
                {
                  Roles targetRoles = targetTemplate.roles;
                  Roles sourceRoles = sourceTemplate.roles;
              
                  for (int k = 0; k < targetRoles.Count; k++)
                  {                    
                    Role sourceRole = getRole(sourceRoles, targetRoles[k].id);
                
                    if (sourceRole == null)
                    {
                      targetRoles.RemoveAt(k--);
                    }
                  }
                }
              }
            }
            else 
            {
              targetClassTemplatesList.RemoveAt(i--);
            }      
          }
        }
    
        return targetManifest;
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

			/*
			public DataTransferIndices diff(String SourceScopeName, String SourceAppName, List<DataTransferIndex> sourceDtiList)
			{
				DataTransferIndices resultDtis = new DataTransferIndices();
				List<DataTransferIndex> resultDtiListItems = new List<DataTransferIndex>();
				resultDtis.DataTransferIndexList = resultDtiListItems;
    
				DataTransferIndices sourceDtis = null;
				DataTransferIndices targetDtis = null;
    
				
				List<DataTransferIndices> dtisList = null;
				dtisList = sourceDtiList;
    
				if (sourceDtiList == null || sourceDtiList.size() < 2) 
					return null;
    
				if (dtisList.get(0).getScopeName().equalsIgnoreCase(SourceScopeName) &&
					dtisList.get(0).getAppName().equalsIgnoreCase(SourceAppName))
				{
					sourceDtis = dtisList.get(0);
					targetDtis = dtisList.get(1);
				}
				else
				{
					sourceDtis = dtisList.get(1);
					targetDtis = dtisList.get(0);
				}    

				
				if (sourceDtis == null || sourceDtis.getDataTransferIndexList().getItems().size() == 0)
				{
					if (targetDtis != null)
					{
						for (DataTransferIndex dti : targetDtis.getDataTransferIndexList().getItems())
						{
							dti.setTransferType(TransferType.DELETE);
							dti.setHashValue(null);
						}
					}
      
					return targetDtis;
				}

				
				if (targetDtis == null || targetDtis.getDataTransferIndexList().getItems().size() == 0)
				{
					if (sourceDtis != null)
					{
						for (DataTransferIndex dti : sourceDtis.getDataTransferIndexList().getItems())
						{
							dti.setTransferType(TransferType.ADD);
						}
					}

					return sourceDtis;
				}

				List<DataTransferIndex> sourceDtiList = sourceDtis.getDataTransferIndexList().getItems();
				List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndexList().getItems();      
				IdentifierComparator identifierComparator = new IdentifierComparator();
    
				Collections.sort(sourceDtiList, identifierComparator);
				Collections.sort(targetDtiList, identifierComparator);

				
				if (sourceDtiList.get(0).getIdentifier().compareTo(targetDtiList.get(targetDtiList.size() - 1).getIdentifier()) > 0 ||
						targetDtiList.get(0).getIdentifier().compareTo(sourceDtiList.get(sourceDtiList.size() - 1).getIdentifier()) > 0)
				{
					for (DataTransferIndex dti : sourceDtiList)
					{
						dti.setTransferType(TransferType.ADD);
					}
      
					for (DataTransferIndex dti : targetDtiList)
					{
						dti.setTransferType(TransferType.DELETE);
						dti.setHashValue(null);
					}

					resultDtiListItems.addAll(sourceDtiList);
					resultDtiListItems.addAll(targetDtiList);
      
					resultDtis.setSortType(sourceDtis.getSortType());
					resultDtis.setSortOrder(sourceDtis.getSortOrder());
      
					return resultDtis;
				}

				
				int sourceIndex = 0;
				int targetIndex = 0;
    
				while (sourceIndex < sourceDtiList.size() && targetIndex < targetDtiList.size())
				{
					DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(sourceIndex);
					DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(targetIndex);
      
					int value = sourceDti.getIdentifier().compareTo(targetDti.getIdentifier());
      
					if (value < 0)
					{
						sourceDti.setTransferType(TransferType.ADD);
						resultDtiListItems.add(sourceDti);
        
						if (sourceIndex < sourceDtiList.size()) sourceIndex++;
					}
					else if (value == 0)
					{
						if (sourceDti.getHashValue().compareTo(targetDti.getHashValue()) == 0)
						{
							targetDti.setTransferType(TransferType.SYNC);
						}
						else
						{
							targetDti.setTransferType(TransferType.CHANGE);
							targetDti.setHashValue(sourceDti.getHashValue());  // use source hash value
							targetDti.setSortIndex(sourceDti.getSortIndex());  // use source sort index 
						}
        
						resultDtiListItems.add(targetDti);
        
						if (sourceIndex < sourceDtiList.size()) sourceIndex++;          
						if (targetIndex < targetDtiList.size()) targetIndex++;
					}
					else
					{
						targetDti.setTransferType(TransferType.DELETE);
						targetDti.setHashValue(null);
						resultDtiListItems.add(targetDti);   
        
						if (targetIndex < targetDtiList.size()) targetIndex++;
					}
				}
    
				if (sourceIndex < sourceDtiList.size())
				{
					for (int i = sourceIndex; i < sourceDtiList.size(); i++)
					{
						DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(i);
						sourceDti.setTransferType(TransferType.ADD);
						resultDtiListItems.add(sourceDti);
					}
				}
				else if (targetIndex < targetDtiList.size())
				{
					for (int i = targetIndex; i < targetDtiList.size(); i++)
					{
						DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(i);
						targetDti.setTransferType(TransferType.DELETE);
						targetDti.setHashValue(null);
						resultDtiListItems.add(targetDti);
					}
				}
    
				resultDtis.setSortType(sourceDtis.getSortType());
				resultDtis.setSortOrder(sourceDtis.getSortOrder());
    
				return resultDtis;
			}
*/
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

        manifest = getCrossedManifest();
            
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

        manifest = getCrossedManifest();

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

        dxiRequest.Manifest = getCrossedManifest();

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

        manifest = getCrossedManifest();

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

        dxoRequest.Manifest = getCrossedManifest();

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


			[Test]
			public void PostDataTransferObjects()
			{
				XDocument benchmark = null;
				Response response = null;
				DataTransferIndices dtiList = null;
				DxoRequest poolDxoRequest = new DxoRequest();
				List<DataTransferIndex> sourcePoolDtiList = new List<DataTransferIndex>();
				List<DataTransferIndex> deleteDtiList = new List<DataTransferIndex>();
				DataTransferObjects sourceDtos = null;

				poolDxoRequest.Manifest = getCrossedManifest();

				dtiList =
					_dxfrProvider.GetDataTransferIndicesWithManifest(
						_settings["ProjectName"], _settings["ApplicationName"],
						_settings["GraphName"], "MD5", poolDxoRequest.Manifest);

				foreach (DataTransferIndex poolDti in dtiList.DataTransferIndexList)
				{
					switch (poolDti.TransferType)
					{
						case TransferType.Delete:
							deleteDtiList.Add(poolDti);
							break;
						case TransferType.Change:
						case TransferType.Add:
							sourcePoolDtiList.Add(poolDti);
							break;
					}
				}

				if (sourcePoolDtiList.Count > 0)
				{
					// request source DTOs

					DataTransferIndices poolDataTransferIndices = new DataTransferIndices();
					poolDxoRequest.DataTransferIndices = poolDataTransferIndices;
					List<DataTransferIndex> poolDtiList = new List<DataTransferIndex>();
					poolDataTransferIndices.DataTransferIndexList = sourcePoolDtiList;
					poolDtiList = sourcePoolDtiList;

					sourceDtos = _dxfrProvider.GetDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
						_settings["GraphName"], poolDxoRequest);

					List<DataTransferObject> sourceDtoListItems = sourceDtos.DataTransferObjectList;

					// set transfer type for each DTO : poolDtoList and remove/report ones that have changed
					// and deleted during review and acceptance period
					for (int j = 0; j < sourceDtoListItems.Count; j++)
					{
						DataTransferObject sourceDto = sourceDtoListItems[j];
						String identifier = sourceDto.identifier;

						if (sourceDto.GetClassObject(identifier) != null)
						{
							foreach (DataTransferIndex dti in poolDtiList)
							{
								if (dti.Identifier == identifier)
								{
									sourcePoolDtiList.Remove(dti);
									if (dti.TransferType == TransferType.Sync)
									{
										sourceDtoListItems.RemoveAt(j--);  // exclude SYNC DTOs
									}
									else
									{
										sourceDto.transferType = dti.TransferType;
									}
									break;
								}
							}
						}
					}
				}
				DataTransferObjects poolDtos = new DataTransferObjects();
				List<DataTransferObject> poolDtoListItems = new List<DataTransferObject>();
				poolDtos.DataTransferObjectList = poolDtoListItems;

				if (sourceDtos != null && sourceDtos.DataTransferObjectList != null)
				{
					poolDtoListItems = sourceDtos.DataTransferObjectList;
				}

				// create identifiers for deleted DTOs
				foreach (DataTransferIndex deleteDti in deleteDtiList)
				{
					DataTransferObject deleteDto = new DataTransferObject();
					deleteDto.identifier = deleteDti.Identifier;
					deleteDto.transferType = TransferType.Delete;
					poolDtoListItems.Add(deleteDto);
				}

				// post add/change/delete DTOs to target endpoint
				if (poolDtoListItems.Count > 0)
				{
					response = _dxfrProvider.PostDataTransferObjects(_settings["ProjectName"], _settings["ApplicationName"],
							_settings["GraphName"], poolDtos);

					string path = String.Format(
							"{0}DxfrResponesABC10.xml",
							_settings["XmlPath"]
						);

					String responseString = ToXml(response).ToString();
					benchmark = XDocument.Load(path);
					String benchmarkString = benchmark.ToString();
					Assert.AreEqual(responseString, benchmarkString);
				}
			}

    }
}

