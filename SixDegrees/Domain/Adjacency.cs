using System.Collections.Generic;

namespace SixDegrees.Domain
{
    public class Adjacency
    {
        public string nodeTo { get; set; }
        public IDictionary<string, object> data { get; set; }

        public Adjacency()
        {
            data = new Dictionary<string, object>();
        }
    }
}