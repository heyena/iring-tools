package org.iringtools.library.directory;

//import java.util.Collections;  // comment out unused import
import java.util.Collections;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.OrderExpressions;
import org.iringtools.directory.Application;
import org.iringtools.directory.ApplicationData;
import org.iringtools.directory.Commodity;
import org.iringtools.directory.DataExchanges;
import org.iringtools.directory.Directory;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Scope;
import org.iringtools.utility.AppDataComparator;
import org.iringtools.utility.CommodityComparator;
import org.iringtools.utility.ExchangeComparator;
import org.iringtools.utility.GraphComparator;
//import org.iringtools.utility.AppDataComparator; // comment out unused import
//import org.iringtools.utility.CommodityComparator; // comment out unused import
//import org.iringtools.utility.ExchangeComparator; // comment out unused import
//import org.iringtools.utility.GraphComparator; // comment out unused import
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.ScopeComparator;

//import org.iringtools.utility.ScopeComparator; // comment out unused import

public class DirectoryProvider {
	private static final Logger logger = Logger
			.getLogger(DirectoryProvider.class);
	private Map<String, Object> settings;
	String path;

	public DirectoryProvider(Map<String, Object> settings) {
		this.settings = settings;
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
	}

	public Directory getDirectory() throws Exception {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");

		if (IOUtils.fileExists(path)) {
			return JaxbUtils.read(Directory.class, path);
		} else {
			logger.info("Directory file does not exist. Create empty one.");

			Directory directory = new Directory();
			JaxbUtils.write(directory, path, false);

			return directory;
		}
	}

	/*
	 * public Directory getDirectory() { logger.debug("getDirectory()");
	 * 
	 * path =
	 * settings.get("basePath").toString().concat("WEB-INF/data/directory.xml");
	 * try { if (IOUtils.fileExists(path)) { Directory directory =
	 * JaxbUtils.read(Directory.class, path); // Sorting Scopes... List<Scope>
	 * directorytList = directory.getScope(); ScopeComparator scopeCompare = new
	 * ScopeComparator(); Collections.sort(directorytList, scopeCompare);
	 * 
	 * for (Scope scope : directorytList) { ApplicationData appData =
	 * scope.getApplicationData();
	 * 
	 * if (appData != null) { // sorting Application Data List<Application>
	 * AppDataList = appData.getApplication(); AppDataComparator appCompare =
	 * new AppDataComparator(); Collections.sort(AppDataList, appCompare);
	 * 
	 * for (Application app : AppDataList) { // sorting graphs
	 * List<org.iringtools.directory.Graph> graphList = app .getGraph();
	 * GraphComparator graphCompare = new GraphComparator();
	 * Collections.sort(graphList, graphCompare);
	 * 
	 * } } DataExchanges exchangeData = scope.getDataExchanges(); // sorting
	 * exchangeData List<Commodity> commodityList = exchangeData.getCommodity();
	 * CommodityComparator commodityCompare = new CommodityComparator();
	 * Collections.sort(commodityList, commodityCompare);
	 * 
	 * for (Commodity commodity : commodityList) { // sorting commodity
	 * List<Exchange> exchangeList = commodity.getExchange(); ExchangeComparator
	 * exchangeCompare = new ExchangeComparator();
	 * Collections.sort(exchangeList, exchangeCompare);
	 * 
	 * } } // write it back JaxbUtils.write(dir, path);
	 * JaxbUtils.write(directory, path, false); return directory; }
	 * 
	 * Directory directory = new Directory(); JaxbUtils.write(directory, path,
	 * false); return directory; } catch (Exception e) { String message =
	 * "Error getting exchange definitions: " + e; logger.error(message); //
	 * throw new ServiceProviderException(message); } return null; }
	 */

	public Exchange postExchageDefinition(Exchange exchange, String scope,
			String name) {

		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (name.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								int maxid = 0;
								// exchange.setId(Integer.toString(i));
								for (Exchange exchangefile : exchangeList) {
									String sid = exchangefile.getId();
									int id = Integer.parseInt(sid);
									if (id > maxid) {
										maxid = id;
									}
								}
								exchange.setId(Integer.toString(maxid + 1));
								exchangeList.add(exchange);
								System.out.println(".....");
								/*
								 * for (Exchange exchangeDirectory :
								 * exchangeList) { i++; }
								 */
								ExchangeComparator exchangeDefSort = new ExchangeComparator();
								Collections.sort(exchangeList, exchangeDefSort);
							}
						}
					}
				}
				
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting exchange definition of [" + scope
					+ "." + name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();
		return exchange;
	}

	public void editExchageDefinition(Exchange exchange, String scope,
			String name, String oldConfigName) {

		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (name.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								for (Exchange exchangefile : exchangeList) {
									if (exchangefile.getName()
											.equalsIgnoreCase(oldConfigName)) {

										exchangefile
												.setName(exchange.getName());
										exchangefile.setDescription(exchange
												.getDescription());
										exchangefile.setSourceUri(exchange
												.getSourceUri());
										exchangefile.setSourceScope(exchange
												.getSourceScope());
										exchangefile.setSourceApp(exchange
												.getSourceApp());
										exchangefile.setSourceGraph(exchange
												.getSourceGraph());
										exchangefile.setTargetUri(exchange
												.getTargetUri());
										exchangefile.setTargetScope(exchange
												.getTargetScope());
										exchangefile.setTargetApp(exchange
												.getTargetApp());
										exchangefile.setTargetGraph(exchange
												.getTargetGraph());
									}

								}
								ExchangeComparator exchangeDefSort = new ExchangeComparator();
								Collections.sort(exchangeList, exchangeDefSort);
							}
						}
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting exchange definition of [" + scope
					+ "." + name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();
	}

	public Exchange getCommodityConfigInfo(String comm, String scope,
			String name) {

		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (comm.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								for (Exchange exchange : exchangeList) {
									if (exchange.getName().equalsIgnoreCase(
											name)) {
										return exchange;
									}

								}

							}
						}
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting exchange definition of [" + scope
					+ "." + name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();
		return null;
	}

	public Exchange deleteExchangeConfig(String comm, String scope, String name) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (comm.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								for (Exchange exchange : exchangeList) {
									if (exchange.getName().equalsIgnoreCase(
											name)) {
										exchangeList.remove(exchange);
										break;
									}

								}
								ExchangeComparator exchangeDefSort = new ExchangeComparator();
								Collections.sort(exchangeList, exchangeDefSort);
							}
						}
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting exchange definition of [" + scope
					+ "." + name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();
		return null;
	}

	public DataFilter getDataFilter(String commName, String scope,
			String name) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (commName.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								for (Exchange exchange : exchangeList) {
									if (exchange.getName().equalsIgnoreCase(
											name)) {
										return exchange.getDataFilter();
									}

								}

							}
						}
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error getting Data Filter of [" + scope + "."
					+ name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();
		return null;
	}

	public synchronized void postDataFilterExpressions(String scope,
			String name, String commName, Expressions mex, OrderExpressions mOe) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();

						for (Commodity commodity : commodityList) {
							if (commName.equalsIgnoreCase(commodity.getName())) {

								List<Exchange> exchangeList = commodity
										.getExchange();
								for (Exchange exchange : exchangeList) {
									if (exchange.getName().equalsIgnoreCase(
											name)) {
										DataFilter df = exchange
												.getDataFilter();
										if (df == null) {
											df = new DataFilter();
											df.setExpressions(null);
											df.setOrderExpressions(null);
										}
										exchange.setDataFilter(df);
										if (mex != null) {
											df.setExpressions(null);
											df.setExpressions(mex);
										}
										if (mOe != null) {
											df.setOrderExpressions(null);
											df.setOrderExpressions(mOe);
										}
										// exchange.setDataFilter(df);
										// //exchangeList.add(exchange);
									}
								}

							}
						}
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting Data Filter of [" + scope + "."
					+ name + "]: " + e;
			logger.error(message);
		}
		// return Response.ok().entity(exchange).build();

	}

	public Scope postNewScope(Scope scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();
				for (Scope scopefromfile : directorytList) {
					String scopeName = scopefromfile.getName();
					if (scopeName.equalsIgnoreCase(scope.getName())) {
						return scope;
					}
				}
				directorytList.add(scope);
				ScopeComparator scopeSort = new ScopeComparator();
				Collections.sort(directorytList, scopeSort);

				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting new Scope of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
		return scope;
	}

	public void postEditedScope(String newScope, String oldScope) {

		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();
				for (Scope scopefromfile : directorytList) {
					String scopefile = scopefromfile.getName();
					if (oldScope.equalsIgnoreCase(scopefile)) {
						scopefromfile.setName(newScope);
						break;
					}
				}
				ScopeComparator scopeSort = new ScopeComparator();
				Collections.sort(directorytList, scopeSort);
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error editing Scope of [" + newScope + "."
					+ "]: " + e;
			logger.error(message);
		}

	}

	public void deleteScope(String scopeName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();
				for (Scope scopefromfile : directorytList) {
					String scope = scopefromfile.getName();
					if (scopeName.equalsIgnoreCase(scope)) {
						directorytList.remove(scopefromfile);
						break;
					}
				}
				ScopeComparator scopeSort = new ScopeComparator();
				Collections.sort(directorytList, scopeSort);
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting new Scope of [" + scopeName + "."
					+ "]: " + e;
			logger.error(message);
		}
	}

	public Scope getScopeInfo(String scopeName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();
				for (Scope scopefromfile : directorytList) {
					String scope = scopefromfile.getName();
					if (scopeName.equalsIgnoreCase(scope)) {
						return scopefromfile;
					}
				}
				JaxbUtils.write(directory, path, false);
			}
		} catch (Exception e) {
			String message = "Error posting new Scope of [" + scopeName + "."
					+ "]: " + e;
			logger.error(message);
		}
		return null;
	}

	public Application postApplication(Application app, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						if (applicationData != null) {
							List<Application> appData = applicationData
									.getApplication();
							for (Application applicatio : appData) {
								if (applicatio.getName().equalsIgnoreCase(
										app.getName())) {
									return app;
								}
							}
							appData.add(app);
							AppDataComparator applicationSort = new AppDataComparator();
							Collections.sort(appData, applicationSort);
							JaxbUtils.write(directory, path, false);
						}
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Application of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
		return app;
	}

	public void deleteApplication(String app, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						if (applicationData != null) {
							List<Application> appData = applicationData
									.getApplication();
							for (Application applicatio : appData) {
								if (applicatio.getName().equalsIgnoreCase(app)) {
									appData.remove(applicatio);
									break;
								}
							}
							AppDataComparator applicationSort = new AppDataComparator();
							Collections.sort(appData, applicationSort);
							JaxbUtils.write(directory, path, false);
						}
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Application of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
	}

	public void editApplication(Application app, String oldAppName, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						if (applicationData != null) {
							List<Application> appData = applicationData
									.getApplication();
							for (Application applicatio : appData) {
								if (applicatio.getName().equalsIgnoreCase(
										oldAppName)) {
									applicatio.setName(app.getName());
									applicatio.setBaseUri(app.getBaseUri());
									applicatio.setDescription(app
											.getDescription());
									
									break;
								}
							}
							AppDataComparator applicationSort = new AppDataComparator();
							Collections.sort(appData, applicationSort);
							JaxbUtils.write(directory, path, false);

						}
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Application of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
	}

	public Application getApplicationInfo(String app, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						if (applicationData != null) {
							List<Application> appData = applicationData
									.getApplication();
							for (Application applicatio : appData) {
								if (applicatio.getName().equalsIgnoreCase(app)) {
									return applicatio;
								}
							}
						}
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Application of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
		return null;
	}

	public org.iringtools.directory.Graph postGraph(
			org.iringtools.directory.Graph graph, String scope, String appName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						List<Application> appData = applicationData
								.getApplication();

						for (Application application : appData) {
							if (appName.equalsIgnoreCase(application.getName())) {
								List<org.iringtools.directory.Graph> graf = application
										.getGraph();
								for (org.iringtools.directory.Graph graphfile : graf) {
									if (graphfile.getName().equalsIgnoreCase(
											graph.getName())) {
										return graph;
									}

								}
								graf.add(graph);
								GraphComparator graphSort = new GraphComparator();
								Collections.sort(graf, graphSort);
							}
						}
						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Graph of [" + scope + "." + "]: "
					+ e;
			logger.error(message);
		}
		return graph;
	}

	public void deleteGraph(String graph, String scope, String appName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						List<Application> appData = applicationData
								.getApplication();

						for (Application application : appData) {
							if (appName.equalsIgnoreCase(application.getName())) {
								List<org.iringtools.directory.Graph> graf = application
										.getGraph();
								for (org.iringtools.directory.Graph graphfile : graf) {
									if (graphfile.getName().equalsIgnoreCase(
											graph)) {
										graf.remove(graphfile);
										break;
									}

								}
								GraphComparator graphSort = new GraphComparator();
								Collections.sort(graf, graphSort);
							}
						}
						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Graph of [" + scope + "." + "]: "
					+ e;
			logger.error(message);
		}
	}

	public void editGraph(org.iringtools.directory.Graph graph, String scope,
			String appName, String oldGraphName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						List<Application> appData = applicationData
								.getApplication();

						for (Application application : appData) {
							if (appName.equalsIgnoreCase(application.getName())) {
								List<org.iringtools.directory.Graph> graf = application
										.getGraph();
								for (org.iringtools.directory.Graph graphfile : graf) {
									if (graphfile.getName().equalsIgnoreCase(
											oldGraphName)) {
										graphfile.setName(graph.getName());
										graphfile.setDescription(graph
												.getDescription());
							/*			graphfile.setCommodity(graph
												.getCommodity()); */
										
										break;
									}
								}
								GraphComparator graphSort = new GraphComparator();
								Collections.sort(graf, graphSort);
								JaxbUtils.write(directory, path, false);
							}
						}

					}
				}
			}
		} catch (Exception e) {
			String message = "Error editing Graph of [" + scope + "." + "]: "
					+ e;
			logger.error(message);
		}
	}

	public org.iringtools.directory.Graph getGraphInfo(String graph,
			String scope, String appName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						ApplicationData applicationData = scopeDirectory
								.getApplicationData();
						List<Application> appData = applicationData
								.getApplication();

						for (Application application : appData) {
							if (appName.equalsIgnoreCase(application.getName())) {
								List<org.iringtools.directory.Graph> graf = application
										.getGraph();
								for (org.iringtools.directory.Graph graphfile : graf) {
									if (graphfile.getName().equalsIgnoreCase(
											graph)) {
										return graphfile;
									}

								}
							}
						}
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Graph of [" + scope + "." + "]: "
					+ e;
			logger.error(message);
		}
		return null;
	}

	public Commodity postCommodity(Commodity comm, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();
						for (Commodity commodity : commodityList) {
							if (commodity.getName().equalsIgnoreCase(
									comm.getName())) {
								return comm;
							}
						}

						commodityList.add(comm);
						CommodityComparator commoditySort = new CommodityComparator();
						Collections.sort(commodityList, commoditySort);
						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Commodity of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
		return comm;
	}

	public void deleteCommodity(String comm, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();
						for (Commodity commodity : commodityList) {
							if (commodity.getName().equalsIgnoreCase(comm)) {
								commodityList.remove(commodity);
								break;
							}
						}
						CommodityComparator commoditySort = new CommodityComparator();
						Collections.sort(commodityList, commoditySort);
						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Commodity of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
	}

	public Commodity getCommodityInfo(String comm, String scope) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();
						for (Commodity commodity : commodityList) {
							if (commodity.getName().equalsIgnoreCase(comm)) {
								return commodity;
							}
						}

						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Commodity of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
		return null;
	}

	public void editCommodity(Commodity comm, String scope, String oldCommName) {
		path = settings.get("basePath").toString()
				.concat("WEB-INF/data/directory.xml");
		try {
			if (IOUtils.fileExists(path)) {
				Directory directory = JaxbUtils.read(Directory.class, path);
				List<Scope> directorytList = directory.getScope();

				for (Scope scopeDirectory : directorytList) {
					// ApplicationData appData =
					// scopeDirectory.getApplicationData();

					if (scope.equalsIgnoreCase(scopeDirectory.getName())) {
						DataExchanges exchangeData = scopeDirectory
								.getDataExchanges();
						List<Commodity> commodityList = exchangeData
								.getCommodity();
						for (Commodity commodity : commodityList) {
							if (commodity.getName().equalsIgnoreCase(
									oldCommName)) {
								commodity.setName(comm.getName());
								
								break;
							}
						}
						CommodityComparator commoditySort = new CommodityComparator();
						Collections.sort(commodityList, commoditySort);
						JaxbUtils.write(directory, path, false);
					}
				}
			}
		} catch (Exception e) {
			String message = "Error posting Commodity of [" + scope + "."
					+ "]: " + e;
			logger.error(message);
		}
	}
}
