using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SixDegrees.Domain
{
  //   overridable: false,  
  //type: 'circle',  
  //color: '#ccb',  
  //alpha: 1,  
  //dim: 3,  
  //height: 20,  
  //width: 90,  
  //autoHeight: false,  
  //autoWidth: false,  
  //lineWidth: 1,  
  //transform: true,  
  //align: "center",  
  //angularWidth:1,  
  //span:1,  

    
    public class Node
    {
        public string id { get; set; }
        public string name { get; set; }
        public IDictionary<string, object> data { get; set; }
        public IList<Adjacency> adjacencies { get; set; }

        public Node()
        {
            data = new Dictionary<string, object>();
            adjacencies = new List<Adjacency>();
        }
    }
}
