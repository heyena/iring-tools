using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using NHibernate;
using org.iringtools.nhibernate;
using org.iringtools.library;
using org.iringtools.adapter.datalayer;
using org.iringtools.adapter;

namespace org.iringtools.nhibernate.ext
{
  public class NHibernateAuthorization : NHibernateDataLayer, IAuthorization
  {
    private NHibernateSettings _nhSettings;

    [Inject]
    public NHibernateAuthorization(AdapterSettings settings, IDictionary keyRing, NHibernateSettings nhSettings)
      : base(settings, keyRing)
    {
      _nhSettings = nhSettings;
    }

    public DataFilter Authorize(DataFilter dataFilter)
    {
      List<Object> objects = new List<Object>();
      ISession session = null;

      try
      {
        session = NHibernateSessionManager.Instance.GetSession(
          _nhSettings["AppDataPath"], _nhSettings["Scope"]);

        if (session != null)
        {
          //Do Stuff Here!
        }
      }
      finally
      {
        if (session != null)
          session.Close();
      }

      return dataFilter;
    }
  }
}
