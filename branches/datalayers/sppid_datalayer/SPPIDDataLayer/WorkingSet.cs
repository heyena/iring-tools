using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace org.iringtools.adapter.datalayer.sppid
{
  public class WorkingSet
  {
    private Dictionary<string, string> _connStrs = new Dictionary<string, string>();
    private DBType _dbType;
    private string _plantSchema = string.Empty;

    public WorkingSet(string plantConnStr)
    {
      _dbType = Utility.GetDBType(plantConnStr);
      _connStrs[Constants.SPPID_PLANT_SCHEMA] = plantConnStr;

      if (Utility.GetDBType(plantConnStr) == DBType.ORACLE)
      {
        DataTable result = DBManager.Instance.ExecuteQuery(plantConnStr, Constants.ORACLE_GET_CURRENT_SCHEMA);

        if (result != null && result.Rows.Count > 0)
        {
          _plantSchema = result.Rows[0][0].ToString();
        }
      }
      else if (Utility.GetDBType(plantConnStr) == DBType.SQLServer)
      {
        //TODO: need to verify
        _plantSchema = "dbo";
      }
    }

    public WorkingSet(string plantSchemaConnStr, string plantDictConnStr, string pidSchemaConnStr, string pidDictConnStr)
      : this(plantSchemaConnStr)
    {
      _connStrs[Constants.SPPID_PLANT_DICTIONARY] = plantDictConnStr;
      _connStrs[Constants.SPPID_PID_SCHEMA] = pidSchemaConnStr;
      _connStrs[Constants.SPPID_PID_DICTIONARY] = pidDictConnStr;
    }

    public void GrantPrivilege(string privilege)
    {
      GrantPrivileges(new List<string>{ privilege });
    }

    public void GrantPrivileges(List<string> privileges)
    {
      foreach (string connStr in _connStrs.Values)
      {
        GrantPrivileges(connStr, privileges, _plantSchema);
      }
    }

    public void RevokePrivilege(string privilege)
    {
      RevokePrivileges(new List<string> { privilege });
    } 

    public void RevokePrivileges(List<string> privileges)
    {
      foreach (string connStr in _connStrs.Values)
      {
        GrantPrivileges(connStr, privileges, _plantSchema);
      }
    }

    public void GrantPrivilege(string connStr, string privilege, string user)
    {
      GrantPrivileges(connStr, new List<string> { privilege }, user);
    }

    public void GrantPrivilege(List<string> connStr, string privilege, string user)
    {
      GrantPrivileges(connStr, new List<string> { privilege }, user);
    }

    public void GrantPrivileges(string connStr, List<string> privileges, string user)
    {
      string privilegeStr = string.Join(",", privileges.ToArray());
      string grantCmd = string.Empty;

      if (_dbType == DBType.ORACLE)
      {
        grantCmd = string.Format(Constants.ORACLE_GRANT_PRVILEGES_TEMPLATE, privilegeStr, user);
      }
      else if (_dbType == DBType.SQLServer)
      {
        grantCmd = string.Format(Constants.SQLSERVER_GRANT_PRVILEGES_TEMPLATE, privilegeStr, user);
      }

      if (!string.IsNullOrEmpty(grantCmd))
      {
        DBManager.Instance.ExecuteNonQuery(connStr, grantCmd);
      }
    }

    public void GrantPrivileges(List<string> connStrs, List<string> privileges, string user)
    {
      foreach (string connStr in connStrs)
      {
        GrantPrivileges(connStr, privileges, user);
      }
    }

    public void RevokePrivilege(string connStr, string privilege, string user)
    {
      RevokePrivileges(connStr, new List<string> { privilege }, user);
    }

    public void RevokePrivilege(List<string> connStrs, string privilege, string user)
    {
      RevokePrivileges(connStrs, new List<string> { privilege }, user);
    }

    public void RevokePrivileges(string connStr, List<string> privileges, string user)
    {
      string privilegeStr = string.Join(",", privileges.ToArray());
      string revokeCmd = string.Empty;

      if (_dbType == DBType.ORACLE)
      {
        revokeCmd = string.Format(Constants.ORACLE_REVOKE_PRVILEGES_TEMPLATE, privilegeStr, user);
      }
      else if (_dbType == DBType.SQLServer)
      {
        revokeCmd = string.Format(Constants.SQLSERVER_REVOKE_PRVILEGES_TEMPLATE, privilegeStr, user);
      }

      if (!string.IsNullOrEmpty(revokeCmd))
      {
        DBManager.Instance.ExecuteNonQuery(connStr, revokeCmd);
      }
    }

    public void RevokePrivileges(List<string> connStrs, List<string> privileges, string user)
    {
      foreach (string connStr in connStrs)
      {
        RevokePrivileges(connStr, privileges, user);
      }
    }  
  }
}
