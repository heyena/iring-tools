package org.iringtools.hibernate;

import java.util.Properties;

import org.hibernate.Session;
import org.hibernate.SessionFactory;
import org.hibernate.cfg.Configuration;

public class HibernateProvider {

	private Session getNHSession(String dbProvider, String dbServer, String dbInstance, String dbName, String dbSchema, String dbUserName, String dbPassword, String portNumber)
	{
		String connStr;

		if (portNumber.equals(""))
		{
			if (dbProvider.toUpperCase().contains("ORACLE"))
			{
				portNumber = "1521";
			}
			else if (dbProvider.toUpperCase().contains("MYSQL"))
			{
				portNumber = "3306";
			}
		}

		if (dbProvider.toUpperCase().contains("MSSQL"))
		{
			connStr = String.format("Data Source=%1$s\\%2$s;Initial Catalog=%3$s;User ID=%4$s;Password=%5$s", dbServer, dbInstance, dbName, dbUserName, dbPassword);
		}
		else
		{
			connStr = String.format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=%1$s)(PORT=%2$s)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=%3$s)));User ID=%4$s;Password=%5$s", dbServer, portNumber, dbInstance, dbUserName, dbPassword);
		}

		Properties properties = new Properties();
		properties.setProperty("hibernate.connection.provider_class", "org.hibernate.connection.DriverManagerConnectionProvider");
		//properties.setProperty("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
		//properties.setProperty("hibernate.connection.connection_string", connStr);
		properties.setProperty("hibernate.connection.driver_class", getConnectionDriver(dbProvider));
		properties.setProperty("hibernate.dialect", getDatabaseDialect(dbProvider));

		
		Configuration config = new Configuration();
		config.addProperties(properties);
		

		SessionFactory sessionFactory = config.buildSessionFactory();
		return sessionFactory.getCurrentSession();
	}

	private String getConnectionDriver(String dbProvider)
	{
		if (dbProvider.toUpperCase().contains("MSSQL"))
		{
			return "com.microsoft.sqlserver.jdbc.SQLServerDriver";
		}
		else if (dbProvider.toUpperCase().contains("MYSQL"))
		{
			return "com.mysql.jdbc.Driver";
		}
		else if (dbProvider.toUpperCase().contains("ORACLE"))
		{
			return "oracle.jdbc.driver.OracleDriver";
		}
		else
		{
			throw new RuntimeException(String.format("Database provider %1$s is not supported", dbProvider));
		}
	}

	private String getDatabaseDialect(String dbProvider)
	{
		if (dbProvider.toUpperCase().equals("MSSQL2008"))
		{
				return "org.hibernate.dialect.SQLServer2008Dialect";

		}
		else if (dbProvider.toUpperCase().equals("MSSQL2005"))
		{
				return "org.hibernate.dialect.SQLServer2005Dialect";

		}
		else if (dbProvider.toUpperCase().equals("MSSQL2000"))
		{
				return "org.hibernate.dialect.SQLServerDialect";

		}
		else if (dbProvider.toUpperCase().equals("ORACLE10G"))
		{
				return "org.hibernate.dialect.Oracle10gDialect";

		}
		else if (dbProvider.toUpperCase().equals("ORACLE9I"))
		{
				return "org.hibernate.dialect.Oracle9Dialect";

		}
		else if (dbProvider.toUpperCase().equals("ORACLE8I"))
		{
				return "org.hibernate.dialect.Oracle8iDialect";

		}
		else if (dbProvider.toUpperCase().equals("ORACLELITE"))
		{
				return "org.hibernate.dialect.OracleDialect";

		}else if (dbProvider.toUpperCase().equals("MYSQL3") || dbProvider.toUpperCase().equals("MYSQL4")){
			
			return "org.hibernate.dialect.MySQLDialect";
		}
		else if (dbProvider.toUpperCase().equals("MYSQL5"))
		{
				return "org.hibernate.dialect.MySQL5Dialect";
		}
		else
		{
				throw new RuntimeException(String.format("Database provider %1$s not supported.", dbProvider));
		}
	}

}
