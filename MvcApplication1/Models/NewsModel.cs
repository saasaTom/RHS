using System.Data.Entity;
using System.Data.Objects;
using System.Data.EntityModel;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System;

namespace RobynHandMadeSoap.Models
{
    public class NewsModel : DbContext
    {
        public class news
        {
            public int Id { get; set; }
            public System.DateTime Date { get { return _HTMLDate.date; } set { this._HTMLDate.date = value; } }
            private Types.HTMLDate _HTMLDate  { get; set; }
            public string DateString { get { return this._HTMLDate.toOrdinal(); } }
            private Types.HTMLString _HTMLString { get; set; }
            [Required]
            public string Detail
            {
                get { return _HTMLString.safeValue; }
                set { _HTMLString.actualValue = value; }
            }
            public string ImgURL {get;set;}
            public string DetailHTML { get { return _HTMLString.htmlValue; } }
            public string jsArrayDate { get { return this.Date.ToString("dd MMM yyyy"); } }

            public news(){
            this._HTMLString = new Types.HTMLString();
            this._HTMLDate = new Types.HTMLDate();
        }

            public RobynHandMadeSoap.News toNews() {
                RobynHandMadeSoap.News myNews = new News()
                {
                    Id = this.Id,
                    Date = this.Date,
                    Detail = this.Detail,
                    ImgURL = this.ImgURL
                };
                return myNews;
            }

            public news(RobynHandMadeSoap.News newsItem)
        {
            this.Id = newsItem.Id;
            this._HTMLString = new Types.HTMLString();
            this._HTMLDate = new Types.HTMLDate();
            this.Date = newsItem.Date;
            this._HTMLDate.date = newsItem.Date;
            this._HTMLString.actualValue = newsItem.Detail;
            this.ImgURL = newsItem.ImgURL;
        }

            public void mockNew(DateTime newsDate) {
                this.Detail = @"[p class=""detail""]TEST[/p]  ";
                this.Date = newsDate;
            }

        }
    }
}