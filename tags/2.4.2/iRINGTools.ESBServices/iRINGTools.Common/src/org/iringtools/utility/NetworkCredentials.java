package org.iringtools.utility;

public class NetworkCredentials
{
  protected String userName;
  protected String password;
  protected String domain;
  
  public NetworkCredentials() {}
  
  public NetworkCredentials(String userName, String password) 
  {
    this(userName, password, null);
  }
  
  public NetworkCredentials(String userName, String password, String domain) 
  {
    setUserName(userName);
    setPassword(password);
    setDomain(domain);
  }
  
  public void setUserName(String userName)
  {
    this.userName = userName;
  }
  
  public String getUserName()
  {
    return userName;
  }

  public void setPassword(String password)
  {
    this.password = password;
  }

  public String getPassword()
  {
    return password;
  }

  public void setDomain(String domain)
  {
    this.domain = domain;
  }

  public String getDomain()
  {
    return domain;
  }
  
  
}
