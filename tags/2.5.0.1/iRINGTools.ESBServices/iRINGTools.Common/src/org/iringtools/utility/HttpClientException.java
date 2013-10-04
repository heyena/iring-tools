package org.iringtools.utility;

public class HttpClientException extends Exception
{
  private static final long serialVersionUID = 1L;
  private int errorCode;
  private String errorMessage;

  public HttpClientException()
  {
    super();
    setErrorMessage("");
  }
  
  public HttpClientException(Exception e)
  {
    super(e);
  }

  public HttpClientException(int errorCode, Exception e)
  {
    super(e);
    setErrorCode(errorCode);
  }

  public HttpClientException(int errorCode, String errorMessage)
  {
    super(errorMessage);
    this.setErrorCode(errorCode);
    setErrorMessage(errorMessage);
  }
  
  public HttpClientException(String errorMessage)
  {
    super(errorMessage);
    setErrorMessage(errorMessage);
  }
  
  public void setErrorCode(int errorCode)
  {
    this.errorCode = errorCode;
  }

  public int getErrorCode()
  {
    return errorCode;
  }
  
  public void setErrorMessage(String errorMessage)
  {
    this.errorMessage = errorMessage;
  }

  public String getErrorMessage()
  {
    return errorMessage;
  }

  public String toString()
  {
    return errorMessage;
  }
}
  