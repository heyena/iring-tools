using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class Scope
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }

    public IList<Application> Applications { get; set; }

    public Scope()
    {
      
    }

    public Scope(string name, string description)
    {
      Name = name;
      Description = description;
    }

    /// <summary>
    /// Finds an application in the scope
    /// </summary>
    /// <param name="applicationId">The application id to find</param>
    /// <returns>Application</returns>
    public Application FindApplication(int applicationId)
    {
      this.Applications = this.Applications ?? new LazyList<Application>();
      //see if this item is in the scope already
      return (from si in this.Applications
              where si.Id == applicationId
              select si).SingleOrDefault();
    }

    /// <summary>
    /// Adds a application to the scope
    /// </summary>
    public void AddApplication(Application application) 
    {
      //see if this application exists already
      if (!this.Applications.Contains(application))
      {
        //add to list
        this.Applications.Add(application);
      }
    }

    /// <summary>
    /// Remmoves a application from the scope
    /// </summary>
    public void RemoveApplication(Application application)
    {
      RemoveApplication(application.Id);
    }

    /// <summary>
    /// Remmoves a application from the scope
    /// </summary>
    public void RemoveApplication(int applicationId)
    {
      var itemToRemove = FindApplication(applicationId);
      if (itemToRemove != null)
      {
        this.Applications.Remove(itemToRemove);
      }
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is Scope)
      {
        Scope compareTo = (Scope)obj;
        return compareTo.Id == this.Id;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.Name;
    }

    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }
    #endregion
  }
}
