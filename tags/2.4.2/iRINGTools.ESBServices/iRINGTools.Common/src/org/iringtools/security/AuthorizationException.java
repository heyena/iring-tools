package org.iringtools.security;

public class AuthorizationException extends Exception
{
  private static final long serialVersionUID = 1L;
  private String error;

  public AuthorizationException()
  {
    super();
    error = "";
  }

  public AuthorizationException(Exception e)
  {
    super(e);
  }

  public AuthorizationException(String error)
  {
    super(error);
    this.error = error;
  }

  public String toString()
  {
    return error;
  }
}
