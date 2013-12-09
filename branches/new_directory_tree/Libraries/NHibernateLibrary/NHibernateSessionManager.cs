using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using org.iringtools.utility;

namespace org.iringtools.nhibernate
{
  public sealed class NHibernateSessionManager
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateSessionManager));
    private static volatile NHibernateSessionManager _instance;
    private static object _lockObj = new Object();
    private static volatile Dictionary<string, ISessionFactory> _sessionFactories;

    private NHibernateSessionManager() { }

    public static NHibernateSessionManager Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (_lockObj)
          {
            if (_instance == null)
            {
              _instance = new NHibernateSessionManager();
              _sessionFactories = new Dictionary<string, ISessionFactory>();
            }
          }
        }

        return _instance;
      }
    }

    public ISession GetSession(string path, string context)
    {
      try
      {
        lock (_lockObj)
        {
          string factoryKey = context.ToLower();

          if (!_sessionFactories.ContainsKey(factoryKey))
          {
            InitSessionFactory(path, context);
          }

          return _sessionFactories[factoryKey].OpenSession();
        }
      }
      catch (Exception e)
      {
        _logger.Error("Unable to obtain session for [" + context + "]. " + e);
        throw e;
      }
    }

    private void InitSessionFactory(string path, string context)
    {
      try
      {
        lock (_lockObj)
        {
          string cfgPath = string.Format("{0}nh-configuration.{1}.xml", path, context);
          string mappingPath = string.Format("{0}nh-mapping.{1}.xml", path, context);

          if (File.Exists(cfgPath) && File.Exists(mappingPath))
          {
            Configuration cfg = new Configuration();
            cfg.Configure(cfgPath);

            string connStrProp = "connection.connection_string";
            string connStr = cfg.Properties[connStrProp];
            string keyFile = string.Format("{0}{1}.key", path, context);
              
            if (!Utility.IsBase64Encoded(connStr))
            {
              //
              // connection string is not encrypted, encrypt and write it back
              //
              string encryptedConnStr = EncryptionUtility.Encrypt(connStr, keyFile);
              cfg.Properties[connStrProp] = encryptedConnStr;
              SaveConfiguration(cfg, cfgPath);

              // restore plain text connection string for creating session factory
              cfg.Properties[connStrProp] = connStr;
            }
            else
            {
              cfg.Properties[connStrProp] = EncryptionUtility.Decrypt(connStr, keyFile);
            }

            ISessionFactory sessionFactory = cfg.AddFile(mappingPath).BuildSessionFactory();
            string factoryKey = context.ToLower();
            _sessionFactories[factoryKey] = sessionFactory;
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error updating NHibernate session factory [" + context + "]. " + e);
        throw e;
      }
    }

    private void SaveConfiguration(Configuration cfg, string path)
    {
      try
      {
        lock (_lockObj)
        {
          XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);
          writer.Formatting = Formatting.Indented;

          writer.WriteStartElement("configuration");
          writer.WriteStartElement("hibernate-configuration", "urn:nhibernate-configuration-2.2");
          writer.WriteStartElement("session-factory");

          if (cfg.Properties != null)
          {
            foreach (var property in cfg.Properties)
            {
              if (property.Key != "use_reflection_optimizer")
              {
                writer.WriteStartElement("property");
                writer.WriteAttributeString("name", property.Key);
                writer.WriteString(property.Value);
                writer.WriteEndElement();
              }
            }
          }

          writer.WriteEndElement(); // end session-factory
          writer.WriteEndElement(); // end hibernate-configuration
          writer.WriteEndElement(); // end configuration

          writer.Close();
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error saving NHibernate configuration. " + e);
        throw e;
      }
    }
  }
}