using System.Web.Mvc;

namespace wtyler_Blog.Controllers {
    [RequireHttps]
    public class TinyMCESampleController : Controller {

        //
        // GET: /TinyMCESample/

        public ActionResult Index() {

            return View();

        }

    }
}