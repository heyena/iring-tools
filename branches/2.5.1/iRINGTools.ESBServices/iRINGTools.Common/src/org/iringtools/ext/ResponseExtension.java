package org.iringtools.ext;

import org.iringtools.common.response.Level;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;

public class ResponseExtension extends Response
{
  private static final long serialVersionUID = 1L;
  protected Level statuslevel;

  public void append(Response response)
  {
    StatusList statuslist = response.getStatusList();

    for (Status status : statuslist.getItems())
    {
      append(status);
    }
  }

  public void append(Status status)
  {
    Status foundStatus = null;
    boolean wasFound = false;

    StatusList statuslist = new StatusList();

    for (Status candidateStatus : statuslist.getItems())
    {
      if (status.getIdentifier().equalsIgnoreCase(candidateStatus.getIdentifier()))
      {
        foundStatus = (Status) candidateStatus;
        wasFound = true;
      }
    }

    if (!wasFound)
    {
      statuslist.getItems().add(status);
    }
    else
    {
      if (foundStatus.getLevel().ordinal() < ((Status) status).getLevel().ordinal())
      {
        foundStatus.setLevel(((Status) status).getLevel());
      }

      for (String message : status.getMessages().getItems())
      {
        foundStatus.getMessages().getItems().add(message);
      }
    }

    if (this.statuslevel.ordinal() < ((Status) status).getLevel().ordinal())
    {
      this.setLevel(((Status) status).getLevel());
    }
  }

  public void setLevel(Level statuslevel)
  {
    this.statuslevel = statuslevel;
  }
}
