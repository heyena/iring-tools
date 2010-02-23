using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DemoControlPanel.Web
{
    // NOTE: If you change the interface name "IDemoControlPanelService" here, you must also update the reference to "IDemoControlPanelService" in Web.config.
    [ServiceContract]
    public interface IDemoControlPanelService
    {
        [OperationContract]
        void DoWork();
    }
}
