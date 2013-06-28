package org.iringtools.utility;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Scanner;

import javax.xml.bind.JAXBException;
import javax.xml.namespace.QName;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.OMXMLBuilderFactory;
import org.iringtools.common.Configuration;
import org.iringtools.common.Setting;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Graph;
import org.iringtools.directory.Scope;
import org.iringtools.utility.JaxbUtils;

public class DirectoryMigrationUtility {
	private static String ns = "http://www.iringtools.org/directory";

	@SuppressWarnings("unchecked")
	public static void main(String[] args) {
		Scanner scanner = new Scanner(System.in);
		Map<String, String> uriMaps = new HashMap<String, String>();

		try {

			String inPath = "";
			String configFileName = "";
			String outPath = "";

			for (String s : args) {
				if (s.startsWith("-i")) {
					inPath = s.substring(2);
				}
				if (s.startsWith("-u")) {
					configFileName = s.substring(2);
				}
				if (s.startsWith("-o")) {
					outPath = s.substring(2);
				}
			}

			if (inPath.length() == 0) {
				System.out.print("Enter old directory path: ");
				inPath = scanner.nextLine();
			}

			if (configFileName.length() == 0) {
				System.out.print("Enter uri-map config file: ");
				configFileName = scanner.nextLine();
			}

			if (outPath.length() == 0) {
				System.out.print("Enter new directory path: ");
				outPath = scanner.nextLine();
			}

			scanner.close();

			System.out.print("Old Directory path: ".concat(inPath));
			System.out.print("Uri-map file: ".concat(configFileName));
			System.out.print("New Directory path: ".concat(outPath));

			if (!inPath.endsWith("/"))
				inPath += "/";
			if (!(new File(inPath).exists()))
				throw new Exception("Old Directory path does not exist.");

			File uriMapConfigFile = new File(configFileName);
			if (uriMapConfigFile.exists()) {
				Configuration config = JaxbUtils.read(Configuration.class,
						configFileName);

				if (config != null) {
					for (Setting setting : config.getSetting()) {
						uriMaps.put(setting.getName(), setting.getValue());
					}
				}
			}

			if (!outPath.endsWith("/"))
				outPath += "/";
			if (!(new File(outPath).exists()))
				throw new Exception("New Directory path does not exist.");

			String oldDirectoryPath = inPath.concat("directory.xml");
			String newDirectoryPath = outPath.concat("directory.xml");

			InputStream inStream = new FileInputStream(new File(
					oldDirectoryPath));
			OMElement root = OMXMLBuilderFactory.createOMBuilder(inStream)
					.getDocumentElement();

			Directory directory = new Directory();

			//
			// process scopes
			//
			Iterator<OMElement> scopes = root.getChildElements();

			while (scopes.hasNext()) {
				OMElement scopeElt = scopes.next();
				OMElement scopeNameElt = scopeElt
						.getFirstChildWithName(new QName(ns, "name"));

				Scope scope = new Scope();
				scope.setName(scopeNameElt.getText());
				directory.getScope().add(scope);

				//
				// process app data
				//
				ApplicationData appData = new ApplicationData();
				scope.setApplicationData(appData);

				OMElement appDataElt = scopeElt
						.getFirstChildWithName(new QName(ns, "applicationData"));

				if (appDataElt != null) {
					Iterator<OMElement> appElts = appDataElt
							.getChildrenWithName(new QName(ns, "application"));

					while (appElts.hasNext()) {
						OMElement appElt = appElts.next();

						Application app = new Application();
						appData.getApplication().add(app);

						OMElement appNameElt = appElt
								.getFirstChildWithName(new QName(ns, "name"));
						if (appNameElt != null) {
							app.setName(appNameElt.getText());
						}

						OMElement appContextElt = appElt
								.getFirstChildWithName(new QName(ns, "context"));
						if (appContextElt != null) {
							app.setContext(appContextElt.getText());
						}

						OMElement appDescElt = appElt
								.getFirstChildWithName(new QName(ns,
										"description"));
						if (appDescElt != null) {
							app.setDescription(appDescElt.getText());
						}

						OMElement appBaseUriElt = appElt
								.getFirstChildWithName(new QName(ns, "baseUri"));
						if (appBaseUriElt != null) {
							String baseUri = appBaseUriElt.getText();

							if (uriMaps.containsKey(baseUri)) {
								baseUri = uriMaps.get(baseUri);
							}

							app.setBaseUri(baseUri);
						}

						//
						// process graph
						//
						OMElement graphsWrapElt = appElt
								.getFirstChildWithName(new QName(ns, "graphs"));

						if (graphsWrapElt != null) {
							Iterator<OMElement> graphElts = graphsWrapElt
									.getChildrenWithName(new QName(ns, "graph"));

							while (graphElts.hasNext()) {
								OMElement graphElt = graphElts.next();

								Graph graph = new Graph();
								app.getGraph().add(graph);

								OMElement graphNameElt = graphElt
										.getFirstChildWithName(new QName(ns,
												"name"));
								if (graphNameElt != null) {
									graph.setName(graphNameElt.getText());
								}

								OMElement graphDescElt = graphElt
										.getFirstChildWithName(new QName(ns,
												"description"));
								if (graphDescElt != null) {
									graph.setDescription(graphDescElt.getText());
								}

								OMElement graphCommElt = graphElt
										.getFirstChildWithName(new QName(ns,
												"commodity"));
								if (graphCommElt != null) {
									graph.setCommodity(graphCommElt.getText());
								}
							}
						}
					}
				}

				//
				// process data exchange
				//
				DataExchanges dxs = new DataExchanges();
				scope.setDataExchanges(dxs);
				int xSeqId = 0;

				OMElement dxElt = scopeElt.getFirstChildWithName(new QName(ns,
						"dataExchanges"));

				if (dxElt != null) {
					Iterator<OMElement> commElts = dxElt
							.getChildrenWithName(new QName(ns, "commodity"));

					while (commElts.hasNext()) {
						OMElement commElt = commElts.next();

						Commodity comm = new Commodity();
						comm.setName(commElt.getFirstChildWithName(
								new QName(ns, "name")).getText());
						dxs.getCommodity().add(comm);

						//
						// process exchanges
						//
						OMElement xsWrapElt = commElt
								.getFirstChildWithName(new QName(ns,
										"exchanges"));

						if (xsWrapElt != null) {
							Iterator<OMElement> xsElt = xsWrapElt
									.getChildrenWithName(new QName(ns,
											"exchange"));

							while (xsElt.hasNext()) {
								OMElement xElt = xsElt.next();

								Exchange xchange = new Exchange();
								xchange.setId(String.valueOf(xSeqId));
								xSeqId++;

								comm.getExchange().add(xchange);

								OMElement xNameElt = xElt
										.getFirstChildWithName(new QName(ns,
												"name"));
								if (xNameElt != null) {
									xchange.setName(xNameElt.getText());
								}

								OMElement xDescElt = xElt
										.getFirstChildWithName(new QName(ns,
												"description"));
								if (xDescElt != null) {
									xchange.setDescription(xDescElt.getText());
								}

								//
								// port exchange definitions
								//
								String xId = xElt.getFirstChildWithName(
										new QName(ns, "id")).getText();
								File xPath = new File(inPath
										.concat("exchange-")
										.concat(scopeNameElt.getText())
										.concat("-").concat(xId).concat(".xml"));

								if (xPath.exists()) {
									InputStream xPathStream = new FileInputStream(
											xPath);
									OMElement xRoot = OMXMLBuilderFactory
											.createOMBuilder(xPathStream)
											.getDocumentElement();

									//
									// general exchange info
									//
									OMElement xCacheableElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "cachecable"));
									if (xCacheableElt != null) {
										xchange.setCacheable(Boolean
												.parseBoolean(xCacheableElt
														.getText()));
									}

									OMElement xPoolSizeElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "poolSize"));
									if (xPoolSizeElt != null) {
										xchange.setPoolSize(Integer
												.parseInt(xPoolSizeElt
														.getText()));
									}

									//
									// sender info
									//
									OMElement xSrcUriElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "sourceUri"));
									if (xSrcUriElt != null) {
										String sourceUri = xSrcUriElt.getText();

										if (uriMaps.containsKey(sourceUri)) {
											sourceUri = uriMaps.get(sourceUri);
										}

										xchange.setSourceUri(sourceUri);
									}

									OMElement xSrcScopeElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "sourceScopeName"));
									if (xSrcScopeElt != null) {
										xchange.setSourceScope(xSrcScopeElt
												.getText());
									}

									OMElement xSrcAppElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "sourceAppName"));
									if (xSrcAppElt != null) {
										xchange.setSourceApp(xSrcAppElt
												.getText());
									}

									OMElement xSrcGraphElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "sourceGraphName"));
									if (xSrcGraphElt != null) {
										xchange.setSourceGraph(xSrcGraphElt
												.getText());
									}

									//
									// receiver info
									//
									OMElement xTargetUriElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "targetUri"));
									if (xTargetUriElt != null) {
										String targetUri = xTargetUriElt
												.getText();

										if (uriMaps.containsKey(targetUri)) {
											targetUri = uriMaps.get(targetUri);
										}

										xchange.setTargetUri(targetUri);
									}

									OMElement xTargetScopeElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "targetScopeName"));
									if (xTargetScopeElt != null) {
										xchange.setTargetScope(xTargetScopeElt
												.getText());
									}

									OMElement xTargetAppElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "targetAppName"));
									if (xTargetAppElt != null) {
										xchange.setTargetApp(xTargetAppElt
												.getText());
									}

									OMElement xTargetGraphElt = xRoot
											.getFirstChildWithName(new QName(
													ns, "targetGraphName"));
									if (xTargetGraphElt != null) {
										xchange.setTargetGraph(xTargetGraphElt
												.getText());
									}
								}

								//
								// port filter definitions
								//
								File filterPath = new File(inPath
										.concat("Filter-")
										.concat(scopeNameElt.getText())
										.concat("-").concat(xId).concat(".xml"));

								if (filterPath.exists()) {
									DataFilter filter = JaxbUtils.read(
											DataFilter.class,
											filterPath.getAbsolutePath());
									xchange.setDataFilter(filter);
								}
							}
						}
					}
				}
			}System.out.println("entereing");
			
			sortDirectory(directory, newDirectoryPath);
		//	JaxbUtils.write(directory, newDirectoryPath, false);

			System.out.println("Succeded => See new directory output at "
					+ newDirectoryPath.replace("\\", "/").replace("//", "/"));
		} catch (Exception e) {
			System.out.println("Error => " + e.getMessage());
		}
	}

	public static void sortDirectory(Directory directory, String newDirectoryPath) { 
	 
		try { 	System.out.println("entered");
		List<Scope> directorytList = directory.getScope(); 
		ScopeComparator scopeCompare = new ScopeComparator(); 
		Collections.sort(directorytList, scopeCompare);
	 
	 for (Scope scope : directorytList) { 
		 ApplicationData appData =	scope.getApplicationData();
	 
	 if (appData != null) { // sorting Application Data
		 List<Application> AppDataList = appData.getApplication();
		 AppDataComparator appCompare =	 new AppDataComparator();
		 Collections.sort(AppDataList, appCompare);
	 
	 for (Application app : AppDataList) { // sorting graphs
	 List<org.iringtools.directory.Graph> graphList = app .getGraph();
	 GraphComparator graphCompare = new GraphComparator();
	 Collections.sort(graphList, graphCompare);
	 
	 } }
	 DataExchanges exchangeData = scope.getDataExchanges(); 
	 // sorting	 exchangeData 
	 List<Commodity> commodityList = exchangeData.getCommodity();
	 CommodityComparator commodityCompare = new CommodityComparator();
	 Collections.sort(commodityList, commodityCompare);
	 
	 for (Commodity commodity : commodityList) { // sorting commodity
	 List<Exchange> exchangeList = commodity.getExchange(); ExchangeComparator
	 exchangeCompare = new ExchangeComparator();
	 Collections.sort(exchangeList, exchangeCompare);
	 
	 } } // write it back JaxbUtils.write(dir, path);
	
			JaxbUtils.write(directory, newDirectoryPath, false);
		} catch (JAXBException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 
	 
	}
}
