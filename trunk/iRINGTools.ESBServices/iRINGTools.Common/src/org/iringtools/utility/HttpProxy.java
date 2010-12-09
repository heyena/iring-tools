package org.iringtools.utility;

public class HttpProxy extends NetworkCredentials
{
  private String host;
  private int port;
  private NetworkCredentials networkCredentials;
  
  public HttpProxy() {}
  
  public HttpProxy(String host, int port)
  {
    this(host, port, null);
  }
  
  public HttpProxy(String host, int port, NetworkCredentials networkCredentials)
  {
    setHost(host);
    setPort(port);
    setNetworkCredentials(networkCredentials);
  }
  
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

  public void setNetworkCredentials(NetworkCredentials networkCredentials)
  {
    this.networkCredentials = networkCredentials;
    
    if (networkCredentials != null)
    {
      setUserName(networkCredentials.getUserName());
      setPassword(networkCredentials.getPassword());
      setDomain(networkCredentials.getDomain());
    }
  }

  public NetworkCredentials getNetworkCredentials()
  {
    return networkCredentials;
  }
}
