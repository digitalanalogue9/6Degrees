using System.Collections.Generic;
using System.Web.Mvc;
using SixDegrees.Domain;

namespace SixDegrees.Controllers
{
    public class GraphController : RavenController
    {
        public ActionResult Get(string id)
        {
            var graph = Session.Load<Graph>("graphs/" + id);
            return Json(graph, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DemoGet(string id)
        {

            var graph = new Graph
            {
                nodes = new List<Node>
                                            {
                                                new Node
                                                    {
                                                        id = "node0",
                                                        name = "node0 name",
                                                        data =
                                                            {
                                                                {"$dim", "16"},
                                                                {"some other key", "some other value"}
                                                            },
                                                        adjacencies =
                                                            {
                                                                new Adjacency
                                                                    {
                                                                        nodeTo = "node1",
                                                                        data =
                                                                            {
                                                                                {"weight", "3"}
                                                                            }
                                                                    }
                                                            }
                                                    },
                                                new Node
                                                    {
                                                        id = "node1",
                                                        name = "node1 name",
                                                        data =
                                                            {
                                                                {"$dim", "16"},
                                                                {"some other key", "some other value"}
                                                            },
                                                        adjacencies =
                                                            {
                                                                new Adjacency
                                                                    {
                                                                        nodeTo = "node0",
                                                                        data =
                                                                            {
                                                                                {"weight", "3"}
                                                                            }
                                                                    }
                                                            }
                                                    }
                                            }
            };
            return Json(graph, JsonRequestBehavior.AllowGet);
        }

    }

}
