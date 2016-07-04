using System.Data.Entity;
using System.Data.Objects;
using System.Data.EntityModel;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;


namespace RobynHandMadeSoap.Models
{
    public class ShopModel : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<RobynHandMadeSoap.Models.ShopContext>());
        //public ObjectQuery<ProductCategories> Categories { get; set; }
        //public ObjectQuery<Product> Products { get; set; }
        //public ObjectQuery<Product_Detail> ProductDetails { get; set; }
    }

    public static class MyExtensions{
        public static Product_Detail LatestItem(this System.Data.Objects.DataClasses.EntityCollection<Product_Detail> me)
        {
            return me.Where(pd => pd.Cease_Date == null).OrderBy(pd => pd.Effective_Date).Reverse().First();
        }

        /*
        public static System.Data.Objects.DataClasses.EntityObject findById(this System.Data.Objects.DataClasses.EntityCollection<System.Data.Objects.DataClasses.EntityObject> me, int id)
        {
            return me.First(p => (int)p.EntityKey.EntityKeyValues.First().Value == id);
        }

        public static System.Data.Objects.DataClasses.EntityObject findById(this ObjectSet<System.Data.Objects.DataClasses.EntityObject> me, int id)
        {
            return me.First(p => (int)p.EntityKey.EntityKeyValues.First().Value == id);
        }
        */
        public static System.Data.Objects.DataClasses.EntityObject findById(this ObjectSet<Product> me, int id)
        {
            return me.First(p => p.Id == id);
        }

    }



    public class Shop
    {
        public static string DefaultCurrency = "EUR";
        public List<ProductCategories> product_categories;

        public void refresh(IQueryable<Product_Categories> prodCat)
        {
            product_categories = prodCat.ToList().Select(p => RobynHandMadeSoap.Models.ProductCategories.FromEntity(p)).ToList();
        }
        public Shop(IQueryable<Product_Categories> prodCat)
        {
            product_categories = prodCat.ToList().Select(p => ProductCategories.FromEntity(p)).ToList();
        }
    }
    
    public class ProductCategories
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image_Uri { get; set; }
        public string Description { get; set; }
        public List<ProductAndDetail> product_and_detail {get;set;}

        public static ProductCategories FromEntity(RobynHandMadeSoap.Product_Categories dbCategories)
        {

            if (dbCategories != null)
            {
                return new ProductCategories()
                {
                    //Product Table details
                    Id = dbCategories.Id,
                    Name = dbCategories.Name,
                    Image_Uri = dbCategories.Image_Uri,
                    Description = dbCategories.Description,
                    product_and_detail = dbCategories.Products.Where(p => p.Status == null).ToList().Select(p => ProductAndDetail.FromEntity(p)).ToList(),
                    //RobynHandMadeSoap.Product_Detail prodDetail =  dbProduct.Product_Detail;
                };
                //return me;

            }
            return new ProductCategories();
        }

    }
    public class product
    {
       
        private Product _product;
        public int Id { get { return _product.Id; } set { _product.Id = value; } }
        [Required]
        public string Name { get { return _product.Name; } set { _product.Name = value; } }
        [Required]
        public string Image_Uri { get { return _product.Image_Uri; } set { _product.Image_Uri = value; } }
        private Types.HTMLString _HTMLString { get; set; }
        [Required]
        public int? Category_Id { get { return _product.Category_Id; } set { _product.Category_Id = value; } }
        public string Description
        {
            get { return _HTMLString.safeValue; }
            set { _HTMLString.actualValue = value; this._product.Description = this._HTMLString.safeValue; }
        }
        public string DescriptionHTML { get { return _HTMLString.htmlValue; } }
        public string Category_Name { get { return _product.Product_Categories.Name; } }
        public string Product_Code { get { return _product.Product_Code; } set { _product.Product_Code = value; } }
        public string PP_Hosted_Button_Id { get { return _product.PP_Hosted_Button_Id; }  set { _product.PP_Hosted_Button_Id = value; } }
        public string PP_Hosted_Button_Merchant { get { return _product.PP_Hosted_Button_Merchant; } set { _product.PP_Hosted_Button_Merchant = value; } }

        public product()
        {
            _HTMLString = new Types.HTMLString();
            _product = new Product();
            //this.SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            _product = new Product();
            this.Name = "PRODUCT_NAME";
            this.Image_Uri = "/Images/dropbox.jpg";
            this.Category_Id = 0;
            this.Description = "PRODUCT_DESCRIPTION";
        }

        public string PP_Button(bool isMerchant) {
            return isMerchant ? this.PP_Hosted_Button_Merchant : this.PP_Hosted_Button_Id;
        }

        public Product toProduct()
        {
            return _product;
        }

        public void fromEntity(Product dbProduct)
        {
            if (dbProduct != null)
            {
                this._product = dbProduct;
                this._HTMLString.actualValue = dbProduct.Description;
                
            }
            else
            {
                this.SetDefaultValues();
            }
        }
    }
    public class product_detail
    {
        
        //private Regex _HTMLToDBMarkup = new Regex(@"<([^>]*)>");
        //private Regex _DBMarkUpToHTML = new Regex(@"\[([/]?[pPbBeE][1rRmM]?)\]|\[a[^\]]*]");
        private Product_Detail _product_detail;
        public int Product_Id { get { return _product_detail.Product_Id; } set { _product_detail.Product_Id = value; } }
        public System.DateTime Effective_Date { get { return _product_detail.Effective_Date; } set { _product_detail.Effective_Date = value; } }
        [Required]
        public decimal Price { get { return _product_detail.Price; } set { _product_detail.Price = value; } }
        public decimal? Merchant_Price { get { return _product_detail.Price_Merchant; } set { _product_detail.Price_Merchant = value; } }
        public string Price_Currency_Code { get { return _product_detail.Price_Currency_Code; } set { _product_detail.Price_Currency_Code = value; } }

        private Types.HTMLString _HTMLString { get; set; }
        [Required]
        public string Long_Description {
            get { return _HTMLString.safeValue; }
            set { _HTMLString.actualValue = value; this._product_detail.Long_Description = this._HTMLString.safeValue; }
        }
        public string Long_DescriptionHTML{ get { return _HTMLString.htmlValue; }  }
        public int Stock_Quantity { get { return (int)(_product_detail.Stock_Quantity ?? 0); } set { _product_detail.Stock_Quantity = value; } }
        public bool inStock { get { return (Stock_Quantity > 0) ? true : false; } set { if (value) { this.Stock_Quantity = 100; } else { this.Stock_Quantity = 0; } } }

        public product_detail()
        {
            _HTMLString = new Types.HTMLString();
            _product_detail = new Product_Detail();
            //Required to stop a hissy fit when returning StockQuantity or LongDescription which are computed values that do not like NULL
            //this.SetDefaultValues();
        }
        public void SetDefaultValues(int product_id = 0)
        {
            _product_detail = new Product_Detail();

            this.Product_Id = product_id;
            this.Price = 4.50M;
            this.Effective_Date = System.DateTime.Now;
            this.Price_Currency_Code = Shop.DefaultCurrency;
            this.Long_Description = "PRODUCT_DESCRIPTION";
            this.Stock_Quantity = 100;
        }
        


        public decimal? calculatePrice(bool isMerchant) {
            return isMerchant ? this.Merchant_Price : this.Price;
        }

        public Product_Detail toProduct_Detail()
        {
            return _product_detail;
        }

        public void fromEntity(Product_Detail dbProduct_Detail)
        {
            if (dbProduct_Detail != null)
            {
                this._product_detail = dbProduct_Detail;
                //Need to set this at the moment as Long_Description is a derived value from HTMLString class
                //this.Long_Description = dbProduct_Detail.Long_Description;
                this._HTMLString.actualValue = dbProduct_Detail.Long_Description;
            }
            else
            {
                this.SetDefaultValues();
            }
        }
    }

    public class ProductAndDetail
    {
        public product product {get; set;}
        public product_detail product_detail {get; set;}

        public void hackDetails()
        {
            this.product.Description = this.product_detail.Long_Description;
        }

        public ProductAndDetail()
        {
            product = new product();
            product_detail = new product_detail();
        }

        public void SetDefaultValues()
        {
            try
            {
                this.product.SetDefaultValues();
                this.product_detail.SetDefaultValues();
            }
            catch (System.Exception ex)
            {
                if (ex is System.ArgumentNullException)
                {
                    throw (new System.Exception("Either the Product or Product Detail has not been set up properly. :" + ex.Message));
                }
                else
                {
                    throw ex;
                }
            }
        }

        public static ProductAndDetail FromEntity(RobynHandMadeSoap.Product dbProduct)
        {


            ProductAndDetail me = new ProductAndDetail();
            me.product.fromEntity(dbProduct);
            //me.product_detail.fromEntity(dbProduct.Product_Detail.Where(pd => pd.Cease_Date == null).OrderBy(p => p.Effective_Date ).First());
            me.product_detail.fromEntity(dbProduct.Product_Detail.LatestItem());
            return me;
        }
    }

    public class ProductAndDetail_OLD
    {
        public int Id { get; set; }
        public string Name {get;set;}
        public string Image_Uri {get;set;}
        public string Description {get;set;}
        public string DescriptionHTML { get { return this.Description.Replace("\n", "<br>"); } }
        public decimal Price { get; set; }
        public string Price_Currency_Cdoe { get; set; }
        public string Long_Description { get; set; }
        public string Long_DescriptionHTML { get { return this.Long_Description.Replace("\n", "<br>"); } }
        public int Stock_Quantity { get; set; }
        public int Category_Id { get; set; }
        public string Category_Name { get; set; }
        public string Product_Code { get; set; }
        public string PP_Hosted_Button_Id { get; set; }


        public static ProductAndDetail_OLD FromEntity(RobynHandMadeSoap.Product dbProduct)
        {


            ProductAndDetail_OLD me = new ProductAndDetail_OLD();
            if (dbProduct != null && dbProduct.Id >0 )
            {
                //Product Table details
                me.Id = dbProduct.Id;
                me.Name = dbProduct.Name;
                me.Image_Uri = dbProduct.Image_Uri;
                me.Description = dbProduct.Description;
                me.Category_Id = (int) dbProduct.Category_Id;

                me.PP_Hosted_Button_Id = dbProduct.PP_Hosted_Button_Id;
                me.Product_Code = dbProduct.Product_Code;

                //RobynHandMadeSoap.Product_Detail prodDetail =  dbProduct.Product_Detail;

                //System.Data.Objects.DataClasses.EntityCollection<RobynHandMadeSoap.Product_Detail> dbProdDetail = dbProduct.Product_Detail;
                RobynHandMadeSoap.Product_Detail pdList = dbProduct.Product_Detail.Where(p => p.Cease_Date == null).FirstOrDefault();
                if (pdList != null)
                {
                    //Product Detail table details
                    me.Price = pdList.Price;
                    me.Price_Currency_Cdoe = pdList.Price_Currency_Code;
                    me.Stock_Quantity = (int)pdList.Stock_Quantity;
                    me.Long_Description = pdList.Long_Description;
                }

               //Product Category table details
                me.Category_Name =  dbProduct.Product_Categories.Name;
            }
            else
            {
                me.Category_Id = (int)dbProduct.Category_Id;
                me.Description = "";
                me.Long_Description = "";
                me.Name = "PRODUCT_TITLE";
                me.Description = "PRODUCT_DESCRIPTION";
                me.Image_Uri = "~/Images/Default/Product.jpg";
            }
            return me;

        }
    }
}
