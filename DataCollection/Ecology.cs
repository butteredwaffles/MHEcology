using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using SQLite;

namespace DataCollection {
    class Ecology {
        public async Task RunScraper () {
            string address = "http://monsterhunter.wikia.com/wiki/Category:Monster_Ecology";
            var config = Configuration.Default.WithDefaultLoader ();
            var context = BrowsingContext.New (config);
            var page = await context.OpenAsync (address);
            List<Task> dbtasks = new List<Task> ();
            while (page.QuerySelector ("link[rel=\"next\"]") != null) {
                var ele = (IHtmlLinkElement) page.QuerySelector ("link[rel=\"next\"]");
                string addr = ele.Href;

                var ecology_page = page.QuerySelectorAll (".category-gallery-item a").OfType<IHtmlAnchorElement> ().Select (a => a.Href)
                    .Where (a => !a.Contains ("blog") && !a.Contains ("Category") && !a.Contains ("showcase")).Distinct ().ToArray ();
                foreach (string ad in ecology_page) {
                    Task dbtask = UpdateDatabase (GetInfo (ad));
                    dbtasks.Add (dbtask);
                }
                page = await BrowsingContext.New (config).OpenAsync (addr);
            }
            await Task.WhenAll (dbtasks.ToArray ());
        }

        public async Task UpdateDatabase (Task<string[]> task) {
            string[] data = await task;
            var db = new SQLiteAsyncConnection ("../Databases/ecology.db");
            try { await db.CreateTableAsync<MonsterEcology> (); } catch { }
            try {
                if (data[0] == "ERROR") { throw new NullReferenceException (); }
                await db.InsertAsync (new MonsterEcology () {
                    name = data[0],
                        image_url = data[1],
                        taxonomy = data[2],
                        habitat = data[3],
                        niche = data[4],
                        biology = data[5],
                        behavior = data[6]
                });
                Console.WriteLine (String.Format ("Inserted {0} into the database!", data[0]));
            } catch (SQLiteException) { Console.WriteLine (data[0] + " is already in the database."); } catch (Exception) { }
        }

        public async Task<string[]> GetInfo (string address) {
            string name = System.Web.HttpUtility.UrlDecode (address.Split ('/') [4].Replace ("_Ecology", "").Replace ("_", " "));
            Console.WriteLine (String.Format ("Attempting to write data for {0}.", name));
            var config = Configuration.Default.WithDefaultLoader ();
            var context = BrowsingContext.New (config);
            var page = await context.OpenAsync (address);
            string image_url;
            try {
                var imelement = (IHtmlImageElement) page.QuerySelector ("#In-Game_Information")
                    .ParentElement.NextElementSibling.FirstElementChild.FirstElementChild;
                image_url = imelement.Source;
                if (image_url.Contains ("data:image")) {
                    image_url = imelement.GetAttribute ("data-src");
                }
            } catch (Exception) {
                try {
                    var imelement = (IHtmlImageElement) page.QuerySelector ("#In-Game_Description")
                        .ParentElement.NextElementSibling.FirstElementChild.FirstElementChild;
                    image_url = imelement.Source;
                    if (image_url.Contains ("data:image")) {
                        image_url = imelement.GetAttribute ("data-src");
                    }

                } catch (Exception) {
                    image_url = "https://vignette.wikia.nocookie.net/monsterhunter/images/2/2e/MHFU-Question_Mark_Icon.png/revision/latest?cb=20100610145952";
                }
            }
            var tax = page.QuerySelector ("#Taxonomy"); //can contain list elements so needs to be handled seperately
            IElement ul;
            try {
                ul = tax.ParentElement.QuerySelector ("ul");
            } catch (NullReferenceException) {
                ul = null;
            }
            string habitat, niche, biology, behavior, taxonomy;
            try {
                if (ul != null) {
                    taxonomy = String.Join ('\n', ul.QuerySelectorAll ("li").Select (li => li.TextContent));
                } else {
                    taxonomy = tax.ParentElement.NextElementSibling.TextContent;
                }
                habitat = page.QuerySelector ("#Habitat_Range").ParentElement.NextElementSibling.TextContent;
                niche = page.QuerySelector ("#Ecological_Niche").ParentElement.NextElementSibling.TextContent;
                try // people's lovely spelling errors :)
                {
                    biology = page.QuerySelector ("#Biological_Adaptations").ParentElement.NextElementSibling.TextContent;
                } catch {
                    biology = page.QuerySelector ("#Biological_Adaptions").ParentElement.NextElementSibling.TextContent;
                }
                behavior = page.QuerySelector ("#Behavior").ParentElement.NextElementSibling.TextContent;
                return new string[] { name, image_url, taxonomy, habitat, niche, biology, behavior };
            } catch (Exception) { Console.WriteLine ("Either " + name + " is not a monster or some spelling error prevented the data from being accessed."); }
            return new string[] { "ERROR" };
        }
    }

    public class MonsterEcology {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Unique]
        public string name { get; set; }
        public string image_url { get; set; }
        public string taxonomy { get; set; }
        public string habitat { get; set; }
        public string niche { get; set; }
        public string biology { get; set; }
        public string behavior { get; set; }
    }
}