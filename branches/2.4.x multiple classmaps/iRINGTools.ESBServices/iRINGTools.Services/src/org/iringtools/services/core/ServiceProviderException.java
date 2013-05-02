package org.iringtools.services.core;

public class ServiceProviderException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public ServiceProviderException()
  {
    super();
    error = "";
  }

  public ServiceProviderException(Exception e)
  {
    super(e);
  }

  public ServiceProviderException(String error)
  {
    super(error);
    this.error = error;
  }

  public String toString()
  {
    return error;
  }
}
