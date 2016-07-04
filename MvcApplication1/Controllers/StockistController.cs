using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobynHandMadeSoap.Controllers
{
    
    [Authorize(Roles = "Administrator")]
    public class StockistController : Controller
    {
        private static RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();
        //
        // GET: /Stockist/

        public ActionResult Index()
        {
            List<RobynHandMadeSoap.Models.Stockist_Locations> stockists = db.Stockist_Locations.ToList().Select(l => RobynHandMadeSoap.Models.Stockist_Locations.FromEntity(l)).ToList();
            return View(stockists);
        }

    }
}
