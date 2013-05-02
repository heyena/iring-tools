package org.iringtools.services.core;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Map;
import java.util.Random;

public class IDGeneratorProvider
{
  private Random useGetRandom = new Random();
  private Map<String, Object> settings;

  public IDGeneratorProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public Connection getConnection()
  {
    Connection conn = null;
    
    try
    {
      Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver").newInstance();
      String dbConnectionString = (String)settings.get("idGeneratorConnectionString");
      conn = java.sql.DriverManager.getConnection(dbConnectionString);
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }
    
    return conn;
  }

  public boolean checkExist(String uri, String comment, String randomNumber)
  {
    Connection con = null;
    PreparedStatement stmt = null;
    
    try
    {
      con = this.getConnection();
      stmt = con.prepareStatement("select count(*) count from idGeneratorLog where url=? and idnumber=?");
      stmt.setString(1, uri);
      stmt.setString(2, randomNumber);

      ResultSet rs = stmt.executeQuery();
      
      while (rs.next())
      {
        System.out.println("count:" + rs.getInt("count"));
        
        if (rs.getInt("count") != 0)
        {
          return true;
        }
        else
        {
          stmt = con.prepareStatement("insert into idGeneratorLog ( srno , url, idnumber, comment)values ((select MAX(srno) from idGeneratorLog)+1, ?, ?,?)");
          stmt.setString(1, uri);
          stmt.setString(2, randomNumber);
          stmt.setString(3, comment);
          stmt.executeUpdate();
          return false;
        }
      }
      
      con.close();
      stmt.close();
      rs.close();
    }
    catch (SQLException e)
    {
      e.printStackTrace();
    }
    catch (Exception e)
    {
      e.printStackTrace();
    }

    return false;
  }

  public String generateRandomNumber(String uri, String comment)
  {
    long floor = 0L;
    long ceiling = 100000000000L;
    final long i = useGetRandom.nextLong();
    long x = floor + ((i & 0x7fffffffffffffffL) % (ceiling + 1 - floor));
    String randomNumber = "R" + x;
    System.out.println("Generated random number :" + randomNumber);
    
    if (this.checkExist(uri, comment, randomNumber))
    {
      generateRandomNumber(uri, comment);
    }
    
    return randomNumber;
  }
}
