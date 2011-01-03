using System.Security.Principal;
using System.ServiceModel;
using Ninject;

namespace org.iringtools.adapter.identity
{
    public class WindowsAutheticationProvider : IIdentityLayer 
    {
        private AdapterSettings _settings = null;
        
        [Inject]
        public WindowsAutheticationProvider(AdapterSettings settings)
        {
            _settings = settings;
        }
        
        public void Initialize()
        {
            if (ServiceSecurityContext.Current != null)
            {
                IIdentity identity = ServiceSecurityContext.Current.PrimaryIdentity;
                _settings["UserName"] = identity.Name;
            }
        }
    }
}
