using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Raven.Abstractions.Data;
using Raven.Client.Linq;
using Raven.Json.Linq;
using SixDegrees.Domain;
using SixDegrees.Models;

namespace SixDegrees.Controllers
{
    public class HomeController : RavenController
    {
        [HttpGet]
        public ActionResult Index(int? pagesize, int? pagenumber)
        {
            RavenQueryStatistics stats;
            var iq =
                Queryable.Skip(Session.Query<Graph>().Statistics(out stats),
                               pagesize.GetValueOrDefault(10) * (pagenumber.GetValueOrDefault(1) - 1))
                    .Take(pagesize.GetValueOrDefault(10)).Select(s => s).ToList();
            var items =
                iq.Select(s => new GraphLineItem { Id = s.Id, Name = s.Name, LastModified = s.LastModified });
            var totalpages = Math.Ceiling((double)stats.TotalResults / pagesize.GetValueOrDefault(10));
            var model = new HomeIndexViewModel
                            {
                                Graphs = items.ToList(),
                                CurrentPage = pagenumber.GetValueOrDefault(1),
                                TotalCount = stats.TotalResults,
                                TotalPages = (int)totalpages
                            };
            return View(model);
        }

        public ActionResult Demo()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(HttpPostedFileBase file, string name, string delimiter)
        {
            if (file == null) return View();
            if (file.ContentLength == 0) return View();
            var id = Persist(file, name, delimiter, null);
            return RedirectToAction("Edit", "Home", new { Id = id });
        }

        [HttpPost]
        public ActionResult EditWithNewFile(HttpPostedFileBase file, string name, string delimiter, string editid)
        {
            if (file == null) return RedirectToAction("Edit", "Home", new { Id = editid });
            if (file.ContentLength == 0) return RedirectToAction("Edit", "Home", new { Id = editid });
            var id = Persist(file, name, delimiter, editid);
            return RedirectToAction("Edit", "Home", new { Id = id });
        }

        [HttpGet]
        public ActionResult GetFile(string id)
        {
            var attachment = MvcApplication.DocumentStore.DatabaseCommands.GetAttachment("graphs/" + id);
            return File(ReadFully(attachment.Data()), attachment.Metadata["ContentType"].ToString(),
                        attachment.Metadata["FileName"].ToString());
        }


        [HttpGet]
        public ActionResult Edit(string id)
        {
            var graph = Session.Load<Graph>("graphs/" + id);
            var model = new GraphEditModel
                            {
                                Graph = graph
                            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(GraphInputModel input)
        {
            input.Id = input.Id.Replace("graphs/", string.Empty);
            if (input.Submit.ToLower() == "delete")
            {
                return RedirectToAction("Delete", "Home", new { deleteme = input.Id });
            }
            Persist(input, null);
            return RedirectToAction("Edit", "Home", new { input.Id });
        }


        [HttpGet]
        public ActionResult ViewGraph(string id)
        {
            id = id.Replace("graphs/", string.Empty);
            var model = new GraphViewModel { Id = id };
            var item = Session.Load<Graph>("graphs/" + id);
            if (item != null)
            {
                model.Name = item.Name;
                model.LastModified = item.LastModified;
                model.nodes = item.nodes;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string deleteid, int? pagesize, int? pagenumber)
        {
            deleteid = deleteid.Replace("graphs/", string.Empty);
            var item = Session.Load<Graph>("graphs/" + deleteid);
            if (item != null)
            {
                Session.Delete(item);
            }
            return RedirectToAction("Index", new { pagesize, pagenumber });
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }


        private string Persist(GraphInputModel input, HttpPostedFileBase file)
        {
            var filecontents = new StringBuilder();
            var exists = true;
            var graph = Session.Load<Graph>("graphs/" + input.Id);
            if (graph == null)
            {
                graph = new Graph();
                exists = false;
            }
            graph.Id = "graphs/" + input.Id;
            graph.Name = input.Name;
            graph.LastModified = DateTime.Now;

            foreach (var lineitem in input.Nodes)
            {
                var id = lineitem.Id.Trim();
                var name = lineitem.Name.Trim();
                var description = string.IsNullOrEmpty(lineitem.Description) ? string.Empty : lineitem.Description.Trim();
                var itemtype = string.IsNullOrEmpty(lineitem.ItemType) ? string.Empty : lineitem.ItemType.Trim();
                var otherlist = string.IsNullOrEmpty(lineitem.Other) ? new List<string>() : new List<string>( lineitem.Other.Trim().Split(',')); 
                var data = new Dictionary<string, object>();
                filecontents.Append(string.Format("{0},{1},{2},{3}{4}",
                    name,
                    itemtype,
                    description,
                    string.Join(",", otherlist.Select(e => e.Trim())),
                    Environment.NewLine));
                var node = graph.nodes.SingleOrDefault(e => e.id == id);
                if (node == null)
                {
                    node = new Node
                               {
                                   id = name.Replace(" ", ""),
                                   name = name,
                                   data = data
                               };
                    graph.nodes.Add(node);
                }
                else
                {
                    node.name = name;
                }
                if (!node.data.ContainsKey("itemtype"))
                {
                    node.data.Add("itemtype", itemtype);
                }
                node.data["itemtype"] = itemtype;
                if (!node.data.ContainsKey("description"))
                {
                    node.data.Add("description", description);
                }
                node.data["description"] = description;
                if (!node.data.ContainsKey("other"))
                {
                    node.data.Add("other", string.Join(",", otherlist));
                }
                else
                {
                    var existingotherlist = node.data["other"].ToString().Split(',');
                    var union = existingotherlist.Union(otherlist);
                    node.data["other"] = string.Join(",", union);
                }
                var nodeadjacency = new Adjacency
                                        {
                                            nodeTo = node.id,
                                            data = new Dictionary<string, object> { { "adjacentname", name } }
                                        };
                var columns = new List<string>(lineitem.Other.Split(','));
                for (var x = 0; x < columns.Count(); x++)
                {
                    var adjacentnode = graph.nodes.SingleOrDefault(e => e.id == columns[x].Replace(" ", ""));
                    if (adjacentnode == null)
                    {
                        adjacentnode = new Node
                                           {
                                               id = columns[x].Replace(" ", ""),
                                               name = columns[x],
                                               data = new Dictionary<string, object>
                                                          {
                                                              {"itemtype", string.Empty},
                                                              {"description", string.Empty},
                                                              {"other", node.name}
                                                          }
                                           };
                        graph.nodes.Add(adjacentnode);
                    }
                    var adjacentotherlist = adjacentnode.data["other"].ToString().Split(',');
                    if (!adjacentotherlist.Contains(node.name))
                    {
                        adjacentnode.data["other"] = adjacentnode.data["other"] + "," + node.name;
                    }

                    if (adjacentnode.adjacencies.SingleOrDefault(e => e.nodeTo == nodeadjacency.nodeTo) == null)
                    {
                        adjacentnode.adjacencies.Add(nodeadjacency);
                    }
                    if (node.adjacencies.Where(e => e.nodeTo == adjacentnode.id).Count() == 0)
                    {
                        var adjacentnodeadjacency = new Adjacency
                                                        {
                                                            nodeTo = adjacentnode.id,
                                                            data =
                                                                new Dictionary<string, object> { { "adjacentname", columns[x] } }
                                                        };

                        node.adjacencies.Add(adjacentnodeadjacency);
                    }
                }
            }
            Session.Store(graph);
            byte[] bytearray;
            var filename = string.Empty;
            var filemimetype = string.Empty;

            if (exists)
            {
                var attachment = MvcApplication.DocumentStore.DatabaseCommands.GetAttachment("graphs/" + input.Id);
                filemimetype = attachment.Metadata["ContentType"].ToString();
                filename = attachment.Metadata["FileName"].ToString();

                MvcApplication.DocumentStore.DatabaseCommands.DeleteAttachment("graphs/" + input.Id, null);

            }

            if (file == null)
            {
                bytearray = System.Text.Encoding.UTF8.GetBytes(filecontents.ToString());
                filename = string.IsNullOrEmpty(filename) ? input.Name.Trim() + ".txt" : filename;
                filemimetype = string.IsNullOrEmpty(filemimetype) ? "text/plain" : filemimetype;
            }
            else
            {
                bytearray = ReadToEnd(file.InputStream);
                filename = file.FileName;
                filemimetype = file.ContentType;
            }
            using (var mem = new MemoryStream(bytearray))
            {
                MvcApplication.DocumentStore.DatabaseCommands.PutAttachment("graphs/" + input.Id, null, mem,
                                                                            new RavenJObject
                                                                                    {
                                                                                        {"FileName", filename},
                                                                                        {"ContentType", filemimetype}
                                                                                    });
            }




            return input.Id;
        }

        private string Persist(HttpPostedFileBase file, string name, string delimiter, string editid)
        {
            var input = new GraphInputModel
                            {
                                Id = string.IsNullOrEmpty(editid) ? GuidUtil.GenerateComb().ToString() : editid,
                                Name = name,
                                Delimiter = delimiter
                            };


            using (var csvfile = new StreamReader(file.InputStream))
            {
                var line = string.Empty;
                while ((line = csvfile.ReadLine()) != null)
                {
                    var columns = new List<string>(line.Split(','));
                    var itemid = columns[0].Replace(" ", "");
                    var itemname = columns[0];
                    var itemtype = columns[1];
                    var description = columns[2];
                    var node = new NodeLineItem { Description = description, Id = itemid, ItemType = itemtype, Name = itemname };
                    if (columns[3].Contains(delimiter))
                    {
                        var delimiterlist = new List<string> { delimiter };
                        columns =
                            new List<string>(columns[3].Split(delimiterlist.ToArray(),
                                                              StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        columns.RemoveRange(0, 3);
                    }
                    node.Other = string.Join(",", columns);
                    input.Nodes.Add(node);
                }
                file.InputStream.Position = 0;
                var id = Persist(input, file);
                return id;
            }
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                var readBuffer = new byte[4096];

                var totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead != readBuffer.Length) continue;
                    var nextByte = stream.ReadByte();
                    if (nextByte == -1) continue;
                    var temp = new byte[readBuffer.Length * 2];
                    Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                    Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                    readBuffer = temp;
                    totalBytesRead++;
                }

                var buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}