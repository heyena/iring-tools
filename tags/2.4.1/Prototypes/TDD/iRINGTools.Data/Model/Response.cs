using System;
using System.Collections.Generic;

namespace iRINGTools.Data
{
  [Serializable]
  public enum StatusLevel
  {
    Success,
    Warning,
    Error
  }

  [Serializable]
  public class Status
  {
    public StatusLevel Level { get; set; }
    public string Identifier { get; set; }
    public List<Dictionary<string, string>> Results { get; set; }
    public List<string> Messages { get; set; }

    public Status()
    {
    }
  }

  [Serializable]
  public class Response 
  {
    public StatusLevel Level { get; set; }
    public DateTime DateTimeStamp { get; set; }
    public List<Status> StatusList { get; set; }
    public List<string> Messages { get; set; }

    public Response()
    {
    }

    public void Append(Response response)
    {
      foreach (Status status in response.StatusList)
      {
        Append(status);
      }
    }

    public void Append(Status status)
    {
      Status foundStatus = null;
      bool wasFound = false;
      foreach (Status candidateStatus in StatusList)
      {
        if (status.Identifier == candidateStatus.Identifier)
        {
          foundStatus = candidateStatus;
          wasFound = true;
        }
      }

      if (!wasFound)
      {
        StatusList.Add(status);
      }
      else
      {
        if (foundStatus.Level < status.Level)
          foundStatus.Level = status.Level;

        foreach (string message in status.Messages)
        {
          foundStatus.Messages.Add(message);
        }
      }

      if (Level < status.Level)
        Level = status.Level;
    }

    public override string ToString()
    {
      string messages = String.Empty;

      foreach (Status status in StatusList)
      {
        foreach (string message in status.Messages)
        {
          messages += String.Format("{0} : {1}\\r\\n", status.Identifier, message);
        }
      }

      return messages;
    }
  }

}
