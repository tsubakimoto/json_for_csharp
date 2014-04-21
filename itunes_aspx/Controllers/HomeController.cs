using itunes_aspx.Models;
using json_for_csharp.common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace itunes_aspx.Controllers
{
    public class HomeController : Controller
    {
        private readonly string URL = "http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/wa/wsSearch?term={0}&country=jp&media=music&entity=album&limit=20&lang=ja_jp";

        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(SearchModel model, string returnUrl)
        {
            var term = HttpUtility.UrlEncode(model.Artist, Encoding.UTF8);
            var root = new RootObject();

            try
            {
                var json = GetJson(string.Format(URL, term));
                root = JsonConvert.DeserializeObject<RootObject>(json);
                ViewBag.Root = root;
            }
            catch (WebException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return View();
        }

        private string GetJson(string url)
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            return client.DownloadString(url);
        }
    }
}