using System.Web.Http;
using WebApiContrib.Formatting.Html;

namespace Mimic.Web.Controllers
{
    public class MimicController : ApiController
    {
        public IHttpActionResult Get(string uri = "")
        {
            // Get sitemap node by url
            var currentNode = MimicContext.Current.Services.MimicService.GetCurrentNodeByUrl(uri, true);

            // Get node view name
            var viewName = currentNode["view"]?.ToString() ?? "Index";

            // return the view
            return new ViewResult(Request, viewName, currentNode);
        }
    }
}