package org.iringtools.utility;

public class EncryptionException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public EncryptionException()
  {
    super();
    error = "";
  }

  public EncryptionException(Exception e)
  {
    super(e);
  }

  public EncryptionException(String error)
  {
    super(error);
    this.error = error;
  }

  public String toString()
  {
    return error;
  }
}
