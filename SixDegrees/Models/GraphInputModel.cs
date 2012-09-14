using System.Collections.Generic;
using System.Web.Mvc;
using SixDegrees.Domain;

namespace SixDegrees.Models
{
    public class GraphEditModel
    {
        public Graph Graph { get; set; }
    }
    public class GraphInputModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Delimiter { get; set; }
        public string Submit { get; set; }
        public IList<NodeLineItem> Nodes { get; set; }

        public GraphInputModel()
        {
            Nodes = new List<NodeLineItem>();
        }
    }
}