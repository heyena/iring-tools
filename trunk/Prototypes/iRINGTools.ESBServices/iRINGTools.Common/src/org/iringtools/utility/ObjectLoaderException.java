package org.iringtools.utility;

public class ObjectLoaderException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public ObjectLoaderException()
  {
    super();
    error = "";
  }
  
  public ObjectLoaderException(Exception e)
  {
    super(e);
  }

  public ObjectLoaderException(String error)
  {
    super(error);
    this.error = error;
  }
  
  public String toString()
  {
    return error;
  }
}
  