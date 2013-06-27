using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using log4net;

namespace org.iringtools.library
{
  public static class SessionState
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionState));

    private static ConcurrentDictionary<string, Dictionary<string, object>> _sessions =
      new ConcurrentDictionary<string, Dictionary<string, object>>();

    public static void Start(string sessionId)
    {
      _logger.Debug("Session " + sessionId + " started.");
      _sessions[sessionId] = new Dictionary<string, object>();
    }

    public static void Set(string sessionId, string key, object value)
    {
      if (!_sessions.ContainsKey(sessionId))
      {
        _sessions[sessionId] = new Dictionary<string, object>();
      }

      _sessions[sessionId][key] = value;
    }

    public static object Get(string sessionId, string key)
    {
      object value = null;

      if (_sessions.ContainsKey(sessionId))
      {
        Dictionary<string, object> dict = _sessions[sessionId];

        if (dict.ContainsKey(key))
        {
          value = dict[key];
        }
      }

      return value;
    }

    public static Dictionary<string, object> End(string sessionId)
    {
      _logger.Debug("Session " + sessionId + " ended.");
      Dictionary<string, object> value = null;
      _sessions.TryRemove(sessionId, out value);
      return value;
    }
  }
}