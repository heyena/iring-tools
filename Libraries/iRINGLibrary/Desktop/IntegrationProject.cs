namespace org.iringtools.library
{
   public class IntegrationProject
    {
        private string _name;
        private Response _applications;

        public IntegrationProject()
        {
            _name = string.Empty;
            Response _applications = new Response();
        }

        public IntegrationProject(string Name)
        {
            _name = Name;
            Response _applications = new Response();
        }

        public string Name()
        {
            return _name;
        }

        public void AddApplication(IntegrationApplication Application)
        {
            _applications.Add(Application.Name());
        }

        public Response Applications()
        {
            return _applications;
        }
    }
}
