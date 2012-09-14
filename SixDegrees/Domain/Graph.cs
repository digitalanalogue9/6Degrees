using System;
using System.Collections.Generic;

namespace SixDegrees.Domain
{
    public class Graph
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public IList<Node> nodes { get; set; }

        public Graph()
        {
            nodes = new List<Node>();
        }
    }
}