using System;
using System.Collections.Generic;
using SixDegrees.Domain;

namespace SixDegrees.Models
{
    public class GraphViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public IList<Node> nodes { get; set; }

        public GraphViewModel()
        {
            nodes = new List<Node>();
        }

    }

    public class HomeIndexViewModel
    {
        public IList<GraphLineItem> Graphs { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public HomeIndexViewModel()
        {
            Graphs = new List<GraphLineItem>();
        }
    }

    public class GraphLineItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
    }
}