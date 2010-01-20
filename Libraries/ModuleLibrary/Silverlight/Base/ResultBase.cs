
namespace org.iringtools.ontologyservice.presentation.Base
{
    public class ResultBase
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public string StatusID { get; set; }
    }
}
