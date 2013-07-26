package org.iringtools.library.directory;

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
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.ScopeComparator;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  //private Map<String, Object> settings;
  private String path;

  public DirectoryProvider(Map<String, Object> settings)
  {
    //this.settings = settings;
    this.path = settings.get("basePath").toString().concat("WEB-INF/data/directory.xml");
  }

  public Directory getDirectory() throws Exception
  {
    if (IOUtils.fileExists(path))
    {
      return JaxbUtils.read(Directory.class, path);
    }
    else
    {
      logger.info("Directory file does not exist. Create empty one.");

      Directory directory = new Directory();
      JaxbUtils.write(directory, path, false);

      return directory;
    }
  }

  public Exchange postExchangeDefinition(Exchange exchange, String scope, String name)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (name.equalsIgnoreCase(commodity.getName()))
            {
              List<Exchange> exchangeList = commodity.getExchange();
              int maxid = 0;
              
              for (Exchange exchangefile : exchangeList)
              {
                String sid = exchangefile.getId();
                int id = Integer.parseInt(sid);
                if (id > maxid)
                {
                  maxid = id;
                }
              }
              exchange.setId(Integer.toString(maxid + 1));
              exchangeList.add(exchange);
              
              ExchangeComparator exchangeDefSort = new ExchangeComparator();
              Collections.sort(exchangeList, exchangeDefSort);
            }
          }
        }
      }

      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error posting exchange definition of [" + scope + "." + name + "]: " + e;
      logger.error(message);
    }
    
    return exchange;
  }

  public void editExchangeDefinition(Exchange exchange, String scope, String name, String oldConfigName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (name.equalsIgnoreCase(commodity.getName()))
            {

              List<Exchange> exchangeList = commodity.getExchange();
              for (Exchange exchangefile : exchangeList)
              {
                if (exchangefile.getName().equalsIgnoreCase(oldConfigName))
                {

                  exchangefile.setName(exchange.getName());
                  exchangefile.setDescription(exchange.getDescription());
                  exchangefile.setSourceUri(exchange.getSourceUri());
                  exchangefile.setSourceScope(exchange.getSourceScope());
                  exchangefile.setSourceApp(exchange.getSourceApp());
                  exchangefile.setSourceGraph(exchange.getSourceGraph());
                  exchangefile.setTargetUri(exchange.getTargetUri());
                  exchangefile.setTargetScope(exchange.getTargetScope());
                  exchangefile.setTargetApp(exchange.getTargetApp());
                  exchangefile.setTargetGraph(exchange.getTargetGraph());
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
    catch (Exception e)
    {
      String message = "Error posting exchange definition of [" + scope + "." + name + "]: " + e;
      logger.error(message);
    }
  }

  public Exchange getCommodityConfigInfo(String comm, String scope, String name)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (comm.equalsIgnoreCase(commodity.getName()))
            {

              List<Exchange> exchangeList = commodity.getExchange();
              for (Exchange exchange : exchangeList)
              {
                if (exchange.getName().equalsIgnoreCase(name))
                {
                  return exchange;
                }

              }

            }
          }
        }
      }
      
      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error posting exchange definition of [" + scope + "." + name + "]: " + e;
      logger.error(message);
    }
    
    return null;
  }

  public Exchange deleteExchangeConfig(String comm, String scope, String name)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (comm.equalsIgnoreCase(commodity.getName()))
            {

              List<Exchange> exchangeList = commodity.getExchange();
              for (Exchange exchange : exchangeList)
              {
                if (exchange.getName().equalsIgnoreCase(name))
                {
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
    catch (Exception e)
    {
      String message = "Error posting exchange definition of [" + scope + "." + name + "]: " + e;
      logger.error(message);
    }
    
    return null;
  }

  public DataFilter getDataFilter(String commName, String scope, String xid)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (commName.equalsIgnoreCase(commodity.getName()))
            {

              List<Exchange> exchangeList = commodity.getExchange();
              for (Exchange exchange : exchangeList)
              {
                if (exchange.getId().equalsIgnoreCase(xid))
                {
                  return exchange.getDataFilter();
                }

              }

            }
          }
        }
      }
      
      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error getting Data Filter of [" + scope + "." + xid + "]: " + e;
      logger.error(message);
    }
    
    return null;
  }

  public synchronized void postDataFilterExpressions(String scope, String xid, String commName, Expressions mex,
      OrderExpressions mOe)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();

          for (Commodity commodity : commodityList)
          {
            if (commName.equalsIgnoreCase(commodity.getName()))
            {
              List<Exchange> exchangeList = commodity.getExchange();
              for (Exchange exchange : exchangeList)
              {
                if (exchange.getId().equalsIgnoreCase(xid))
                {
                  DataFilter df = exchange.getDataFilter();
                  if (df == null)
                  {
                    df = new DataFilter();
                    df.setExpressions(null);
                    df.setOrderExpressions(null);
                  }
                  exchange.setDataFilter(df);
                  if (mex != null)
                  {
                    df.setExpressions(null);
                    df.setExpressions(mex);
                  }
                  if (mOe != null)
                  {
                    df.setOrderExpressions(null);
                    df.setOrderExpressions(mOe);
                  }
                }
              }
            }
          }
        }
      }
      
      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error posting Data Filter of [" + scope + "." + xid + "]: " + e;
      logger.error(message);
    }
  }

  public Scope postNewScope(Scope scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();
      for (Scope scopefromfile : scopes)
      {
        String scopeName = scopefromfile.getName();
        if (scopeName.equalsIgnoreCase(scope.getName()))
        {
          return scope;
        }
      }
      scopes.add(scope);
      ScopeComparator scopeSort = new ScopeComparator();
      Collections.sort(scopes, scopeSort);

      JaxbUtils.write(directory, path, false);              
    }
    catch (Exception e)
    {
      String message = "Error posting new Scope of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    return scope;
  }

  public void postEditedScope(String newScopeName, String oldScopeName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();
      
      for (Scope scope : scopes)
      {
        String scopeName = scope.getName();
        
        if (oldScopeName.equalsIgnoreCase(scopeName))
        {
          scope.setName(newScopeName);
          
          ApplicationData applicationData = scope.getApplicationData();
          
          if (applicationData != null)
          {
            List<Application> applications = applicationData.getApplication();
            
            for (Application application : applications)
            {
              if (application.getContext() == null || application.getContext().length() == 0)
                application.setContext(scopeName);
            }
          }
          
          break;
        }
      }
      
      ScopeComparator scopeSort = new ScopeComparator();
      Collections.sort(scopes, scopeSort);
      
      JaxbUtils.write(directory, path, false);  
    }
    catch (Exception e)
    {
      String message = "Error editing Scope of [" + newScopeName + "." + "]: " + e;
      logger.error(message);
    }

  }

  public void deleteScope(String scopeName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();
      
      for (Scope scopefromfile : scopes)
      {
        String scope = scopefromfile.getName();
        if (scopeName.equalsIgnoreCase(scope))
        {
          scopes.remove(scopefromfile);
          break;
        }
      }
      
      ScopeComparator scopeSort = new ScopeComparator();
      Collections.sort(scopes, scopeSort);
      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error posting new Scope of [" + scopeName + "." + "]: " + e;
      logger.error(message);
    }
  }

  public Scope getScopeInfo(String scopeName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();
      
      for (Scope scopefromfile : scopes)
      {
        String scope = scopefromfile.getName();
        if (scopeName.equalsIgnoreCase(scope))
        {
          return scopefromfile;
        }
      }
      
      JaxbUtils.write(directory, path, false);
    }
    catch (Exception e)
    {
      String message = "Error posting new Scope of [" + scopeName + "." + "]: " + e;
      logger.error(message);
    }
    
    return null;
  }

  public Application postApplication(Application app, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          if (applicationData != null)
          {
            List<Application> appData = applicationData.getApplication();
            for (Application applicatio : appData)
            {
              if (applicatio.getName().equalsIgnoreCase(app.getName()))
              {
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
    catch (Exception e)
    {
      String message = "Error posting Application of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    
    return app;
  }

  public void deleteApplication(String app, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          if (applicationData != null)
          {
            List<Application> appData = applicationData.getApplication();
            for (Application applicatio : appData)
            {
              if (applicatio.getName().equalsIgnoreCase(app))
              {
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
    catch (Exception e)
    {
      String message = "Error posting Application of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
  }

  public void editApplication(Application app, String oldAppName, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          
          if (applicationData != null)
          {
            List<Application> applications = applicationData.getApplication();
            
            for (Application application : applications)
            {
              if (application.getName().equalsIgnoreCase(oldAppName))
              {
                application.setName(app.getName());
                application.setBaseUri(app.getBaseUri());
                application.setContext(app.getContext());
                application.setDescription(app.getDescription());

                break;
              }
            }
            
            AppDataComparator applicationSort = new AppDataComparator();
            Collections.sort(applications, applicationSort);
            JaxbUtils.write(directory, path, false);
          }
          
          break;
        }
      }
    }
    catch (Exception e)
    {
      String message = "Error updating application [" + app.getName() + "." + "]: " + e;
      logger.error(message);
    }
  }

  public Application getApplicationInfo(String app, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          
          if (applicationData != null)
          {
            List<Application> applications = applicationData.getApplication();
            
            for (Application application : applications)
            {
              if (application.getName().equalsIgnoreCase(app))
              {
                if (application.getContext() == null || application.getContext().length() == 0)
                  application.setContext(s.getName());
                
                return application;
              }
            }
          }
        }
      }
    }
    catch (Exception e)
    {
      String message = "Error adding application of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    
    return null;
  }

  public org.iringtools.directory.Graph postGraph(org.iringtools.directory.Graph graph, String scope, String appName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          List<Application> appData = applicationData.getApplication();

          for (Application application : appData)
          {
            if (appName.equalsIgnoreCase(application.getName()))
            {
              List<org.iringtools.directory.Graph> graf = application.getGraph();
              for (org.iringtools.directory.Graph graphfile : graf)
              {
                if (graphfile.getName().equalsIgnoreCase(graph.getName()))
                {
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
    catch (Exception e)
    {
      String message = "Error posting Graph of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    return graph;
  }

  public void deleteGraph(String graph, String scope, String appName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          List<Application> appData = applicationData.getApplication();

          for (Application application : appData)
          {
            if (appName.equalsIgnoreCase(application.getName()))
            {
              List<org.iringtools.directory.Graph> graf = application.getGraph();
              for (org.iringtools.directory.Graph graphfile : graf)
              {
                if (graphfile.getName().equalsIgnoreCase(graph))
                {
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
    catch (Exception e)
    {
      String message = "Error posting Graph of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
  }

  public void editGraph(org.iringtools.directory.Graph graph, String scope, String appName, String oldGraphName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          List<Application> appData = applicationData.getApplication();

          for (Application application : appData)
          {
            if (appName.equalsIgnoreCase(application.getName()))
            {
              List<org.iringtools.directory.Graph> graf = application.getGraph();
              for (org.iringtools.directory.Graph graphfile : graf)
              {
                if (graphfile.getName().equalsIgnoreCase(oldGraphName))
                {
                  graphfile.setName(graph.getName());
                  graphfile.setDescription(graph.getDescription());
                  /*
                   * graphfile.setCommodity(graph .getCommodity());
                   */

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
    catch (Exception e)
    {
      String message = "Error editing Graph of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
  }

  public org.iringtools.directory.Graph getGraphInfo(String graph, String scope, String appName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          ApplicationData applicationData = s.getApplicationData();
          List<Application> appData = applicationData.getApplication();

          for (Application application : appData)
          {
            if (appName.equalsIgnoreCase(application.getName()))
            {
              List<org.iringtools.directory.Graph> graf = application.getGraph();
              for (org.iringtools.directory.Graph graphfile : graf)
              {
                if (graphfile.getName().equalsIgnoreCase(graph))
                {
                  return graphfile;
                }

              }
            }
          }
        }
      }
    }
    catch (Exception e)
    {
      String message = "Error posting Graph of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    return null;
  }

  public Commodity postCommodity(Commodity comm, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();
          for (Commodity commodity : commodityList)
          {
            if (commodity.getName().equalsIgnoreCase(comm.getName()))
            {
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
    catch (Exception e)
    {
      String message = "Error posting Commodity of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    
    return comm;
  }

  public void deleteCommodity(String comm, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();
          for (Commodity commodity : commodityList)
          {
            if (commodity.getName().equalsIgnoreCase(comm))
            {
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
    catch (Exception e)
    {
      String message = "Error posting Commodity of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
  }

  public Commodity getCommodityInfo(String comm, String scope)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();
          for (Commodity commodity : commodityList)
          {
            if (commodity.getName().equalsIgnoreCase(comm))
            {
              return commodity;
            }
          }

          JaxbUtils.write(directory, path, false);
        }
      }
    }
    catch (Exception e)
    {
      String message = "Error posting Commodity of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
    return null;
  }

  public void editCommodity(Commodity comm, String scope, String oldCommName)
  {
    try
    {
      Directory directory = JaxbUtils.read(Directory.class, path);
      List<Scope> scopes = directory.getScope();

      for (Scope s : scopes)
      {
        if (scope.equalsIgnoreCase(s.getName()))
        {
          DataExchanges exchangeData = s.getDataExchanges();
          List<Commodity> commodityList = exchangeData.getCommodity();
          for (Commodity commodity : commodityList)
          {
            if (commodity.getName().equalsIgnoreCase(oldCommName))
            {
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
    catch (Exception e)
    {
      String message = "Error posting Commodity of [" + scope + "." + "]: " + e;
      logger.error(message);
    }
  }
}
