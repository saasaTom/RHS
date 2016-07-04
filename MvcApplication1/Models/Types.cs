using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace RobynHandMadeSoap.Models
{
    public class Types
    {
        public class HTMLString
        {
            private Regex _HTMLToDBMarkup = new Regex(@"<([^>]*)>");
            //private Regex _DBMarkUpToHTML = new Regex(@"\[([/]?[pPbBeE][1rRmM]?)\]|\[a[^\]]*]");
            private Regex _DBMarkUpToHTML = new Regex(@"\[([pPbBeEa][1rRmM]?[^\]]*]*|[/][^\]]*)\]");
            
            //Whatever the user actually entered into the Fron/Back End.  This should remain unchanged by us
            public string actualValue {get;set;}
            public string safeValue { get { return this.toDBMarkup(); }}
            public string htmlValue { get { return this.toHTMLMarkup(); } }

            public HTMLString(){
                actualValue = "";
            }

            public HTMLString(string str){
                actualValue = str;
                
            }

            public void setHTMLtoDBRegEx(string reg)
            {
                try
                {
                    _HTMLToDBMarkup = new Regex(reg);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public void setDBtoHTMLRegEx(string reg)
            {
                try
                {
                    _DBMarkUpToHTML = new Regex(reg);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public static string convertToMarkup(Regex converterRegEx, string replaceString, string toBeConverted)
            {
                if (toBeConverted != null)
                {
                    return converterRegEx.Replace(toBeConverted, replaceString);
                }
                else
                {
                    return null;
                }
            }

            public string toDBMarkup(string replaceToDB = "[$1]")
            {
                return convertToMarkup(_HTMLToDBMarkup,replaceToDB, this.actualValue);
            }

            public string toHTMLMarkup(string replaceToHTML = "<$1>", string replaceToDB = "[$1]")
            {
                //to return HTML markup we only want to convert from the safeValue.  Thus we will have safe HTML.
                return convertToMarkup(_DBMarkUpToHTML,replaceToHTML,this.safeValue);
            }

            
        }

        public class HTMLDate
        {
            public DateTime date{get;set;}
            public string AddOrdinal(int num)
            {
                        //Don't really need this for dates!  But why not.
                        switch (num % 100)
                        {
                            case 11:
                            case 12:
                            case 13:
                                return num.ToString() + "th";
                        }

                        switch (num % 10)
                        {
                            case 1:
                                return num.ToString() + "st";
                            case 2:
                                return num.ToString() + "nd";
                            case 3:
                                return num.ToString() + "rd";
                            default:
                                return num.ToString() + "th";
                        }

                    }
            public string toOrdinal()
            {
                return this.AddOrdinal(this.date.Day) + this.date.ToString(" MMMM yyyy");
            }
            public HTMLDate()
            {
                this.date = new DateTime();
            }
            public HTMLDate(DateTime value)
            {
                this.date = value;
            }
        }
    }
}