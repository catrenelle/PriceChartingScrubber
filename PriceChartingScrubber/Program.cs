using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PriceChartingScrubber.Models;

namespace PriceChartingScrubber
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "https://www.pricecharting.com/";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            Dictionary<string, string> consoles = new Dictionary<string, string>();

            Console.WriteLine($"{DateTime.Now.ToString()}\tBegan PriceCharting Update Process");
            
            using (var sys = new CatAPIEntities())
            {
                foreach (var item in doc.DocumentNode.Descendants("a").Where(x => x.Attributes["href"] != null && x.Attributes["href"].Value.Contains("/console/")))
                {
                    string consolename = item.InnerText;
                    string consoleurl = item.Attributes["href"].Value;

                    if (!sys.SystemLists.Any(x => x.URL == consoleurl))
                    {
                        sys.SystemLists.Add(new SystemList
                        {
                            SystemName = consolename.Replace("&amp;","&"),
                            URL = consoleurl
                        });
                        sys.SaveChanges();

                        Console.WriteLine($"{DateTime.Now.ToString()}\tAdded {consolename}.");
                    }
                }

                sys.SaveChanges();
                Console.WriteLine($"{DateTime.Now.ToString()}\tFinished Updating System List.");

                // Build game list from each system
                foreach (var system in sys.SystemLists) { GetGameList(system.URL); Console.WriteLine($"{DateTime.Now.ToString()}\tFinished Updating {system.SystemName}"); }
            }

            Console.WriteLine($"{DateTime.Now.ToString()}\tFinished Updating Basic Game List.");
            // Update games
            UpdateGames();

            Console.WriteLine($"Finished {DateTime.Now.ToString()}");
        }

        public static void GetGameList(string url)
        {
            using (var wc = new WebClient())
            {
                using (var sys = new CatAPIEntities())
                {
                    int systemid = sys.SystemLists.FirstOrDefault(x => x.URL == url).id;

                    for (int i = 0; i <= 99999; i++) // arbitrary cap
                    {
                        string response = wc.DownloadString(url + $"?sort=&cursor={50 * i}&format=json"); // increments of 50

                        // Page has no data.
                        if (response.Length < 20)
                            break;

                        dynamic gameitem = JObject.Parse(response);

                        // Let's prep for insert
                        foreach (var game in gameitem.products)
                        {
                            int id = game.id;
                            if (!sys.Games.Any(x => x.PriceChartingID == id))
                            {
                                sys.Games.Add(new Game
                                {
                                    SystemId = systemid,
                                    GameName = game.productName,
                                    PriceChartingID = id
                                });
                            }

                            sys.SaveChanges();
                        }
                    }
                }
            }
        }

        public static void UpdateGames()
        {
            using (var sys = new CatAPIEntities())
            {
                IList<Game> games = sys.Games.ToList();
                foreach (var game in games)
                {
                    if (game.LastModified < DateTime.Now.AddDays(-6) || game.LastModified is null)
                    {
                        var url = "https://www.pricecharting.com/game/" + game.PriceChartingID;
                        var web = new HtmlWeb();
                        var doc = web.Load(url);

                        string gamename = "";
                        string genre = "";
                        DateTime? releasedate = null;
                        string esrb = "";
                        string upc = "";
                        string asin = "";
                        string epid = "";
                        string pricechartingid = "";

                        gamename = doc.DocumentNode.Descendants("h1").FirstOrDefault(x => x.Id == "product_name").InnerHtml.Trim();
                        gamename = gamename.Split('\n')[0].Replace("Prices", "").Trim();

                        foreach (var item in doc.DocumentNode.Descendants("span").Where(x => x.HasClass("attribute")))
                        {
                            var value = item.Descendants("span").FirstOrDefault().InnerHtml.Trim();

                            switch (item.InnerHtml.Split(':')[0].Trim())
                            {
                                case "Genre":
                                    genre = value;
                                    break;
                                case "Release Date":
                                    releasedate = ((!string.IsNullOrWhiteSpace(value) && value != "none") ? DateTime.Parse(value.Replace("0992", "1992").Replace("1905", "1995").Replace("0017", "2017")) : releasedate);
                                    break;
                                case "ESRB Rating":
                                    esrb = value;
                                    break;
                                case "UPC":
                                    upc = value;
                                    break;
                                case "ASIN (Amazon)":
                                    asin = value;
                                    break;
                                case "ePID (eBay)":
                                    epid = value;
                                    break;
                                case "PriceCharting ID":
                                    pricechartingid = value;
                                    break;
                            }
                        }

                        string looseprice = "";
                        string completeprice = "";
                        string newprice = "";
                        string gradedprice = "";
                        string boxonlyprice = "";
                        string manualonlyprice = "";

                        looseprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "used_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();
                        completeprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "complete_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();
                        newprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "new_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();
                        gradedprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "graded_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();
                        boxonlyprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "box_only_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();
                        manualonlyprice = doc.DocumentNode.Descendants("td").FirstOrDefault(x => x.Id == "manual_only_price").Descendants("span").FirstOrDefault(x => x.HasClass("price")).InnerHtml.Replace("$", "").Trim();

                        game.ReleaseDate = releasedate;
                        game.ESRBRating = esrb;
                        game.UPC = upc;
                        game.AmazonASIN = asin;
                        game.EbayEPID = epid;
                        game.Genre = genre;
                        game.LoosePricing = (looseprice != "N/A" ? Decimal.Parse(looseprice) : (Decimal)0.00);
                        game.CompletePricing = (completeprice != "N/A" ? Decimal.Parse(completeprice) : (Decimal)0.00);
                        game.NewPrice = (newprice != "N/A" ? Decimal.Parse(newprice) : (Decimal)0.00);
                        game.GradedPrice = (gradedprice != "N/A" ? Decimal.Parse(gradedprice) : (Decimal)0.00);
                        game.BoxOnlyPrice = (boxonlyprice != "N/A" ? Decimal.Parse(boxonlyprice) : (Decimal)0.00);
                        game.ManualOnlyPrice = (manualonlyprice != "N/A" ? Decimal.Parse(manualonlyprice) : (Decimal)0.00);
                        game.LastModified = DateTime.UtcNow;

                        sys.SaveChanges();
                    }
                }
            }
        }
    }
}
