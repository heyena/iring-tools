package org.iringtools.models;

public class DataModelException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public DataModelException()
  {
    super();
    error = "";
  }

  public DataModelException(Exception e)
  {
    super(e);
  }

  public DataModelException(String error)
  {
    super(error);
    this.error = error;
  }

  public String toString()
  {
    return error;
  }
}
