using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace RobynHandMadeSoap.Controllers
{
    public class ShopController : Controller
    {
        private static RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();
        //private List<Product> productList;
        private static List<RobynHandMadeSoap.Models.ProductAndDetail> productList = db.Products.Where(p => p.Status == null ).ToList().Select(p => Models.ProductAndDetail.FromEntity(p)).ToList();
        private static RobynHandMadeSoap.Models.Shop shop = new RobynHandMadeSoap.Models.Shop(db.Product_Categories.Where(pc => pc.Products.Count > 0));

        public static void refreshProductList()
        {
            productList = null;
            db.Dispose();
            db = new RobynsHandMadeSoapEntities();
            shop.refresh(db.Product_Categories.Where(pc => pc.Products.Count > 0));
            //db.Dispose();
        }

        //
        // GET: /Shop/

        public ViewResult Index()
        {
            /*ViewBag.Count = db.Products.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;
              */

            return View(shop);
        }

        /*
        public ActionResult Product(int id)
        {
            //Product product = db.Products.Single(p => p.Id == id);
            Product_Detail productDetail = db.Product_Detail.Single(p => p.Id == id);
            return PartialView(productDetail);
        }
        */

        public ActionResult Product(int id)
        {
            //Find the Product Category that holds this product
            //var prodCat = shop.product_categories.Where(pc => pc.product_and_detail.Exists(pd =>pd.Id == productId));
            //var prodCat = shop.product_categories.Where(pc => pc.Id == catId);

            try {
                Product product = db.Products.First(p => p.Id == id);

                Models.ProductAndDetail prodDet = Models.ProductAndDetail.FromEntity(product);
                //Find the Product details that belong to this product within the category
                //shop.product_categories.First().
                //RobynHandMadeSoap.Models.ProductAndDetail productDetail = prodCat.FirstOrDefault().product_and_detail.Find(p => p.product.Id == productId);

                //Regex HTMLToDBMarkup = new Regex(@"<([^>]*)>");
                //Regex DBMarkUpToHTML = new Regex(@"\[([/]?[pPbBeE][1rRmM]?)\]|\[a[^\]]*]");
                //productDetail.product_detail.Long_Description = DBMarkUpToHTML.Replace(HTMLToDBMarkup.Replace(productDetail.product_detail.Long_Description, "[$1]"), "<$1>");


                return View(prodDet);
            }catch {
                ViewBag.Error = "We are sorry but the product you were looking for no longer exists.  Please try check out our other soaps";
                return View(); 
            }
        }


        public ActionResult ShopSuccess()
        {
            return View();
        }
        

        ////
        //// GET: /Shop/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true

        //public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        //{
        //    Response.AppendHeader("X-Total-Row-Count", db.Products.Count().ToString());
        //    ObjectQuery<Product> products = db.Products;
        //    products = products.OrderBy("it." + orderBy + (desc ? " desc" : ""));

        //    return PartialView(products.Skip(start).Take(itemsPerPage));
        //}

        ////
        //// GET: /Default5/RowData/5

        //public ActionResult RowData(int id)
        //{
        //    Product product = db.Products.Single(p => p.Id == id);
        //    return PartialView("GridData", new Product[] { product });
        //}

        ////
        //// GET: /Shop/Create

        //public ActionResult Create()
        //{
        //    return PartialView("Edit");
        //}

        ////
        //// POST: /Shop/Create

        //[HttpPost]
        //public ActionResult Create(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Products.AddObject(product);
        //        db.SaveChanges();
        //        return PartialView("GridData", new Product[] { product });
        //    }

        //    return PartialView("Edit", product);
        //}


        ////
        //// GET: /Shop/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    Product product = db.Products.Single(p => p.Id == id);
        //    return PartialView(product);
        //}

        ////
        //// POST: /Shop/Edit/5

        //[HttpPost]
        //public ActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Products.Attach(product);
        //        db.ObjectStateManager.ChangeObjectState(product, EntityState.Modified);
        //        db.SaveChanges();
        //        return PartialView("GridData", new Product[] { product });
        //    }

        //    return PartialView(product);
        //}

        ////
        //// POST: /Shop/Delete/5

        //[HttpPost]
        //public void Delete(int id)
        //{
        //    Product product = db.Products.Single(p => p.Id == id);
        //    db.Products.DeleteObject(product);
        //    db.SaveChanges();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}
    }
}
