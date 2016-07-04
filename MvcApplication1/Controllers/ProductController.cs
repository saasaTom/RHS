using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using RobynHandMadeSoap.Models;
using System.Json;

namespace RobynHandMadeSoap.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductController : Controller
    {
        #region Members 

        private RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();
        private static  RobynsHandMadeSoapEntities dbshop = new RobynsHandMadeSoapEntities();
        //private static RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();
        //private RobynsHandMadeSoapEntities dbMod = new RobynsHandMadeSoapEntities();
        //private static IQueryable<Product_Categories> prodCat = dbshop.Product_Categories.Where(pc => pc.Products.Count > 0);
        private RobynHandMadeSoap.Models.Shop shop = new RobynHandMadeSoap.Models.Shop(dbshop.Product_Categories);

        #endregion

        #region Private Methods

        //Refresh the STATIC variable productList which normally doesn't change, but obviously does here when we are adding/editing/deleting.
        private void refreshProductList()
        {
            dbshop.Dispose();
            dbshop = new RobynsHandMadeSoapEntities();
            shop.refresh(db.Product_Categories.Where(pc => pc.Products.Count > 0));
            ShopController.refreshProductList();
            //db = new RobynsHandMadeSoapEntities();
            
            //shop = new RobynHandMadeSoap.Models.Shop(db.Product_Categories.Where(pc => pc.Products.Count > 0));
        }

        #endregion

        #region OLD Product Editing Procedures

        //
        // GET: /Product/
        //The MAIN landing page for editing Products returns the Index.cshtml view with a SelectList of Category IDS.
        //The view then does AJAX calls to return product and product category grid data.
        public ViewResult Index_Old(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false)
        {
            ViewBag.Count = db.Products.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            var query = db.Product_Categories
              .Select(c => new
              {
                  c.Id,
                  c.Name
              });
            ViewBag.Categories = new SelectList(query.AsEnumerable(), "Id", "Name", 1);

            return View();
        }

        //
        // GET: /Product/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true
        //Returns one or more Products in TABLE format for AJAX grid table DISPLAY
        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false, int catId = 0)
        {

            //Find all Active Products
            ObjectQuery<Product> products = db.Products.Where("it.Status is null");

            if (catId != 0)
            {
                //Return only Products related to the category selected
                Response.AppendHeader("X-Total-Row-Count", "1");
                products = (ObjectQuery<Product>)products.Where(p => p.Category_Id == catId).Include("Product_Categories");
                return PartialView(products);
            }
            else
            {
                //Return ALL products ordered as requested (default ID)
                Response.AppendHeader("X-Total-Row-Count", products.Count().ToString());
                products = products.Include("Product_Categories").OrderBy("it." + orderBy + (desc ? " desc" : ""));

                return PartialView(products.Skip(start).Take(itemsPerPage));
            }

        }

        //
        //Get the data for Category Grid Table
        public ActionResult CategoryGridData(int start = 0, int itemsPerPage = 20, string orderBy = "Id", bool desc = false, int catId = 0)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Product_Categories.Count().ToString());
            List<Product_Categories> product_categories;

            product_categories = db.Product_Categories.OrderBy("it." + orderBy + (desc ? " desc" : "")).ToList();
            return PartialView("CategoryGridData", product_categories.Skip(start).Take(itemsPerPage).ToList());

        }

        //
        //Get the header for Category Grid Table
        public ActionResult CategoryGridHeader()
        {
            Product_Categories product_categories = new Product_Categories();

            return PartialView(product_categories);

        }

        //
        // GET: /Default5/RowData/5
        //Returns the DISPLAY for when we CANCEL an EDIT ro
        public ActionResult RowData(int id)
        {
            Product product = db.Products.Single(p => p.Id == id);
            return PartialView("GridData", new Product[] { product });
        }

        //
        // GET: /Product/Create
        //Creates the new empty row for a NEW product request
        public ActionResult Create(int? catId = 0)
        {
            ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", catId);
            return PartialView("Edit");
        }

        //
        // POST: /Product/Create
        //Handles the SAVING of a NEW product
        [HttpPost]
        public ActionResult Create_Old(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.AddObject(product);
                db.SaveChanges();
                refreshProductList();
                return PartialView("GridData", new Product[] { product });
            }

            ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView("Edit", product);
        }


        //Handles the saving of a NEW category
        [HttpPost]
        public ActionResult CreateCat(Product_Categories product_categories)
        {
            //If the Model passed in is VALID then add to DB and save, returning the new row back to the HTML
            if (ModelState.IsValid)
            {
                //List<Product_Categories> prodCat = new List<Product_Categories>();
                //prodCat.Add(product_categories);

                db.Product_Categories.AddObject(product_categories);
                db.SaveChanges();
                refreshProductList();
                return PartialView("CategorySingleRow", product_categories);
                //return PartialView("CategoryGridData", prodCat);
            }

            ViewBag.ButtonName = "Create";
            //If there is an error, we need to return a slightly different view so we can show the error
            return PartialView("CategorySingleRow", product_categories);
            //return PartialView("NewCategories", new Product_Categories[] { product_categories });
        }


        //Create a new EMPTY row when creating a NEW category
        public ActionResult CreateCat()
        {

            Product_Categories pc = new Product_Categories
            {
                Id = 0,
            };
            ViewBag.ButtonName = "Create";
            return PartialView("CategoryGridData", new List<Product_Categories> { pc });
        }


        //
        // GET: /Product/Edit/5
        //Handle the request to EDIT a product by finding the product from DB and generating the EDIT template row
        public ActionResult Edit_OLD(int id)
        {
            Product product = db.Products.Single(p => p.Id == id);
            ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView(product);
        }



        //
        // POST: /Product/Edit/5
        //Action to handle product editing
        [HttpPost]
        public ActionResult Edit_Old(Product product)
        {
            if (ModelState.IsValid)
            {
                //If editing a product and is ALL GOOD then save changes and return the DISPLAY version of the new row
                db.Products.Attach(product);
                Product_Detail pd = product.Product_Detail.FirstOrDefault(d => d.Cease_Date == null);
                pd.Long_Description = product.Description;
                db.ObjectStateManager.ChangeObjectState(product, EntityState.Modified);
                db.SaveChanges();
                refreshProductList();
                return PartialView("GridData", new Product[] { product });
            }

            //Otherwise we have errors and we need to return the oringal edit row with the errors
            ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView(product);
        }

        //Action to handle category editing
        [HttpPost]
        public ActionResult EditCat(Product_Categories product_categories)
        {

            //List<Product_Categories> prodCat = new List<Product_Categories>();
            //prodCat.Add(product_categories);

            if (ModelState.IsValid)
            {
                //If editing a category and it is ALL GOOD then save changes to the DB and return the changed row
                db.Product_Categories.Attach(product_categories);
                db.ObjectStateManager.ChangeObjectState(product_categories, EntityState.Modified);
                db.SaveChanges();
                refreshProductList();
                return PartialView("CategorySingleRow", product_categories);
                //return PartialView("CategoryGridData",prodCat);
                //return PartialView("GridData", new Product_Categories[] { product_categories[0] });
            }

            //Otherwise we have errors
            return PartialView("CategorySingleRow", product_categories);
            //return PartialView(product_categories);
        }

        #endregion

        //
        // GET: /Product/
        //The MAIN landing page for editing Products returns the Index.cshtml view with a SelectList of Category IDS.
        //The view then does AJAX calls to return product and product category grid data.
        public ViewResult Index()
        {
            return View("EditProduct", shop);
        }

        // GET: /Product/Edit/5
        //The MAIN landing page for editing Products returns the Index.cshtml view with a SelectList of Category IDS.
        //The view then does AJAX calls to return product and product category grid data.
        public ViewResult Edit(int id)
        {
            Product product = db.Products.First(p => p.Id == id);
            Models.ProductAndDetail prodDet = Models.ProductAndDetail.FromEntity(product);

            return View("ProductEditView", prodDet);
        }

        // GET: /Product/Edit/5
        //The MAIN landing page for editing Products returns the Index.cshtml view with a SelectList of Category IDS.
        //The view then does AJAX calls to return product and product category grid data.
        public ViewResult New(int catId)
        {
            //Product product = new Product();
            //Need to add check for Category ID!  Not a big deal but would be nice :-)
            //product.Category_Id = catId;
            //db.Products.AddObject(product);
            Models.ProductAndDetail prodDet = new Models.ProductAndDetail();
            prodDet.SetDefaultValues();
            prodDet.product.Category_Id = catId;
            return View("ProductEditView", prodDet);
        }

        //
        // POST: /Product/Create
        //Handles the SAVING of a NEW product
        [HttpPost]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.AddObject(product);
                db.SaveChanges();
                refreshProductList();
                return PartialView("ProductEditSection", new Product[] { product });
            }

            ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView("Edit", product);
        }

        //
        // POST: /Product/Edit/5
        //Action to handle product editing
        [HttpPost]
        public ActionResult Edit_DELTE(Product product)
        {
            
            if (ModelState.IsValid)
            {
                //If editing a product and is ALL GOOD then save changes and return the DISPLAY version of the new row
                db.Products.Attach(product);

                //Product_Detail pd = product.Product_Detail.FirstOrDefault(d => d.Cease_Date == null);
                //pd.Long_Description = product.Description;
                product.Product_Detail.FirstOrDefault(d => d.Cease_Date == null).Long_Description = product.Description;

                db.ObjectStateManager.ChangeObjectState(product, EntityState.Modified);
                db.SaveChanges();
                refreshProductList();
                Models.ProductAndDetail prodDet = Models.ProductAndDetail.FromEntity(product);
                return PartialView("ProductEditSection", prodDet);
            }

            Models.ProductAndDetail pad = Models.ProductAndDetail.FromEntity(db.Products.First(p => p.Id == product.Id));
            //Otherwise we have errors and we need to return the oringal edit row with the errors
            //ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView("ProductEditView", pad);
        }

        //
        // POST: /Product/Edit/5
        //Action to handle product editing
        [HttpPost]
        public ActionResult Edit(RobynHandMadeSoap.Models.ProductAndDetail productAndDetail)
        {
            //Fixes up some problems with the model!  Mainly that the Product.Decsription field is useless.
            //So we set this directly to the Product_Detail.Long_Description.
            //We should either:  Allow for editing of Product.Description....OR Remove entirely.
            productAndDetail.hackDetails();

            if (ModelState.IsValid)
            {
                //User enters TEXT into some sort of input
                //JS converts ALL <> to []

                //Controller sets value in model to [] but in the backgeound also converts an <> to [] in case JS disabled or bypassed somehow
                //The DataBase saves the escaped string with []
                //This value is pulled back into actual value
                //When writing to HTML we use model.htmlString which has ONLY safe HTML i.e <br> <p> e.t.c
                //JS could also convert the SERVER SAFE HTML if we had too but prob not needed





                //Need to decode the text that has been passed in from escaped chars to non-escaped.  Ideally we 
                //do this when setting Long Description....OR
                //Allow <> tags when posting.....OR
                //convert ALL script tags from Java Script.....Maybe the best idea.
                //The user could still try and pass crap in but we would either deny it....OR convert it anyway so no real problem.
                productAndDetail.product_detail.Long_Description = Server.HtmlDecode(productAndDetail.product_detail.Long_Description);
                
                Product newProduct = productAndDetail.product.toProduct();
                Product_Detail newProductDetail = productAndDetail.product_detail.toProduct_Detail();
                db.Products.Attach(newProduct);
                db.ObjectStateManager.ChangeObjectState(newProduct, EntityState.Modified);

               //NEED TO END ALL EXISTING PRODUCTS.  SEEMS LIKE A PAIN IN THE ARSE
                
               //MOST OF THIS CODE NEEDS TO BE CLEANED UP.  STUCK IN MODELS E>T>C>

                //End existing product detail record
                //newProduct.Product_Detail.First(pd => pd.Cease_Date == null).Cease_Date = DateTime.Now;

                //db.Product_Detail.Where("it.Product_Id == " + newProduct.Id.ToString() + " and it.Cease_Date is null").First().Cease_Date = DateTime.Now;
                //Product p = (Product)db.Products.findById(newProduct.Id);
                //newProduct.Product_Detail = p.Product_Detail;

                newProduct.Product_Detail.LatestItem().Cease_Date = DateTime.Now;
                newProduct.Product_Detail.Add(newProductDetail);
                
                db.SaveChanges();
                //db.ObjectStateManager.ChangeObjectState(newProductDetail, EntityState.Added)

                //Create new product detail record
                
                //newProductDetail.Product_Id = newProduct.Id;
                //db.Product_Detail.AddObject(newProductDetail);
                //db.ObjectStateManager.ChangeObjectState(newProductDetail, EntityState.Added);
                //newProduct.Product_Detail.Add(newProductDetail);
                //newProduct.Product_Detail.Add(newProductDetail);

                //db.Products.Attach(newProduct);
                
                //db.ObjectStateManager.ChangeObjectState(newProduct, EntityState.Modified);
                //db.SaveChanges();
                refreshProductList();
                //Models.ProductAndDetail prodDet = Models.ProductAndDetail.FromEntity(product);
                return PartialView("ProductEditSection", productAndDetail);
            }

            //Models.ProductAndDetail pad = Models.ProductAndDetail.FromEntity(db.Products.First(p => p.Id == product.Id));
            //Otherwise we have errors and we need to return the oringal edit row with the errors
            //ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView("ProductEditSection", productAndDetail);
        }

        //
        // POST: /Product/Edit/5
        //Action to handle product editing
        [HttpPost]
        public ActionResult New(RobynHandMadeSoap.Models.ProductAndDetail   productAndDetail)
        {
            productAndDetail.hackDetails();
            if (ModelState.IsValid)
            {

                Product newProduct = productAndDetail.product.toProduct();
                db.Products.AddObject(newProduct);

                Product_Detail newProductDetail = productAndDetail.product_detail.toProduct_Detail();
                newProductDetail.Product_Id = newProduct.Id;
                db.Product_Detail.AddObject(newProductDetail);

                db.ObjectStateManager.ChangeObjectState(newProduct, EntityState.Added);
                db.SaveChanges();
                //shop.refresh(db.Product_Categories);
                refreshProductList();
                //Models.ProductAndDetail prodDet = Models.ProductAndDetail.FromEntity(product);
                //return RedirectToAction("Index");
                return PartialView("ProductEditSection", productAndDetail);
            }

            //Models.ProductAndDetail pad = Models.ProductAndDetail.FromEntity(db.Products.First(p => p.Id == product.Id));
            //Otherwise we have errors and we need to return the oringal edit row with the errors
            //ViewBag.Category_Id = new SelectList(db.Product_Categories, "Id", "Name", product.Category_Id);
            return PartialView("ProductEditView", productAndDetail);
        }

        //
        // POST: /Product/NewCategory
        //Action to handle product editing
        [HttpPost]
        public JsonResult NewCategory(string categoryTitle)
        {
            Dictionary<String,Object> result = new Dictionary<String,object>()
                {
                    {"Result","Success"},
                    {"Id",null},
                    {"Message",""}
                } ;
            
            try
            {
                Product_Categories cat = new Product_Categories()
                {
                    Name = categoryTitle,
                    Description = categoryTitle
                };
                db.Product_Categories.AddObject(cat);
                db.SaveChanges();
                result["Result"] = "Success";
                result["Id"] = cat.Id;
            }
            catch (Exception ex)
            {
                result["Result"] = "Failure";
                result["Message"] = ex.Message;
            }
            return Json(result);
        }

        //
        // POST: /Product/NewCategory
        //Action to handle product editing
        [HttpPost]
        public JsonResult DeleteCategory(int id)
        {
            JsonObject test = new JsonObject();
            Dictionary<String,object> result = new Dictionary<String,object>()
            {
                {"Result", "Fatal Error"},
                {"Message", "Starting"},
                {"Id",""}
                
            };
            test["res"] = "Success";
            try
            {
                Product_Categories cat = db.Product_Categories.First(x => x.Id == id);
                if (cat.Products.Count() == 0)
                {
                    db.Product_Categories.DeleteObject(cat);
                    
                }
                else if (cat.Products.Count(x => x.Status == null) == 0)
                {
                    cat.Status = "D";
                    cat.Status_Date = DateTime.Now;
                    cat.Status_Reason = "Deleted from Front End";
                    
                }
                else
                {
                    throw new Exception("Category can not be deleted with active products!");
                }
                db.SaveChanges();
                result["Result"] = "Success";
                result["Message"] = "Success";
            }
            catch (Exception ex)
            {
                result["Result"] = "Failure";
                result["Message"] = ex.Message;
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                Response.StatusDescription = "Error deleting record.  Please refer to JSON response.";
            }
            refreshProductList();
            return Json(result);
        }

        //The interface between ImageUpload.js (client side javascript file upload) and UploadFile.cs server side file upload class to save and resize images uploaded for the website.
        [HttpPost]
        public string Upload()
        {
            string virtualPath;
            try
            {
                //Get file information passed in request headers to an UploadedFile object 
                RobynHandMadeSoap.Models.UploadedFile file = RobynHandMadeSoap.Models.UploadedFile.RetrieveFileFromRequest(Request);

                //Save a copy and resized verion of the file
                virtualPath = RobynHandMadeSoap.Models.UploadedFile.SaveFile(file, Server.MapPath("~/Images/"), file.FileRelPath);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = ex.Message;
                return ex.Message;
            }
            return virtualPath;
        }

        //
        // POST: /Product/Delete/5
        //HANDLE delete requess for Products by setting status to "D" for deleted.  This way we can recover in case of an accident or malicious attempt
        [HttpPost]
        public void Delete(int id)
        {
            Product product = db.Products.Single(p => p.Id == id);
            product.Status = "D";
            product.Status_Date = DateTime.Now;
            product.Status_Reason = "Deleted from Web Front End";
            //db.Products.DeleteObject(product);
            db.SaveChanges();
            refreshProductList();
            ViewBag.Message = "Successfully Deleted Product: " + product.Name;
            //RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
