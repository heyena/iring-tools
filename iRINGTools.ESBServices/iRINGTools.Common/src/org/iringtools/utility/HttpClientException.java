package org.iringtools.utility;

public class HttpClientException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public HttpClientException()
  {
    super();
    error = "";
  }
  
  public HttpClientException(Exception e)
  {
    super(e);
  }

  public HttpClientException(String error)
  {
    super(error);
    this.error = error;
  }
  
  public String toString()
  {
    return error;
  }
}
  