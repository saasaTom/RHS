using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RobynHandMadeSoap.Models
{
    public class Stockist
    {
        public int Location_Id { get; set; }
        public string Name { get; set; }

        public long GPS_Lat { get; set; }
        public long GP_Long { get; set; }
        public string Link_Address { get; set; }
    }

    public class Stockist_Locations
    {
        public int Location_Id {get;set;}
        public string Name {get;set;}
        public List<Stockist> Stockist_List {get;set;}

        public static Stockist_Locations FromEntity(RobynHandMadeSoap.Stockist_Locations dbStockist_Locations)
        {
            Stockist_Locations me = new Stockist_Locations();
            me.Stockist_List = new List<Stockist>();
            if (dbStockist_Locations != null)
            {
                me.Location_Id = dbStockist_Locations.Id;
                me.Name = dbStockist_Locations.Name;
                List<RobynHandMadeSoap.Stockist> myList = dbStockist_Locations.Stockists.Where(p => p.Cease_Date == null).ToList();
                foreach (RobynHandMadeSoap.Stockist stockee in myList)
                {
                    Stockist temp = new Stockist();
                    temp.Location_Id = stockee.Location_Id;
                    temp.Name = stockee.Name;
                    temp.Link_Address = stockee.Link_Address;

                    me.Stockist_List.Add(temp);
                
                }
            }
            return me;
        }
    }
}