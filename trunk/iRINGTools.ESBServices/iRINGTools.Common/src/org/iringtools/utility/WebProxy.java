package org.iringtools.utility;

public class WebProxy
{
  private String host;
  private int port;
  private String username;
  private String password;
  
  public void setHost(String host)
  {
    this.host = host;
  }
  
  public String getHost()
  {
    return host;
  }
  
  public void setPort(int port)
  {
    this.port = port;
  }
  
  public int getPort()
  {
    return port;
  }
  
  public void setUsername(String username)
  {
    this.username = username;
  }
  
  public String getUsername()
  {
    return username;
  }
  
  public void setPassword(String password)
  {
    this.password = password;
  }
  
  public String getPassword()
  {
    return password;
  }
}
