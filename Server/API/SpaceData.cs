using EFCore.BulkExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static API.SpaceData;

namespace API
{
    public class SpaceData
    {
        private static readonly double rad = (Math.PI / 180);
        private static readonly string NASA_API_URL = @"https://exoplanetarchive.ipac.caltech.edu/TAP/sync?query=SELECT+pl_name,hostname,ra,dec,sy_plx+FROM+ps+WHERE+ra+IS+NOT+NULL+AND+dec+IS+NOT+NULL+AND+sy_plx+IS+NOT+NULL+AND+pl_bmasse+%3E+1&format=json";
        
        private static readonly string CachePath = Path.Combine(Directory.GetCurrentDirectory(), "cache");
        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        
        public static readonly string StarDataFilePath = Path.Combine(DataPath, "STAR_DATA.csv");
        public static readonly string ExoplanetFilePath = Path.Combine(DataPath, "EXOPLANET_DATA.csv");

        public static string PythonPath = "";

        public static async Task<bool> AddGAIAStarDataToDB(int numberOfRecords, AppDbContext context)
        {
            var query = $@"
                SELECT TOP {numberOfRecords}
                    source_id, 
                    ra, 
                    dec,
                    parallax
                FROM gaiadr3.gaia_source
                WHERE ra IS NOT NULL 
                    AND dec IS NOT NULL 
                    AND parallax is NOT NULL 
                    AND phot_g_mean_mag IS NOT NULL
                ORDER BY phot_g_mean_mag DESC
            ";

            var result = await Task.Run(() => CallPythonGAIADataRequester(StarDataFilePath, query));
            
            if (!result)
                return false;

            using (StreamReader streamReader = new StreamReader(StarDataFilePath))
            {
                string line = streamReader.ReadLine();
                List<StarData> data = new List<StarData>();
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] elements = line.Split(',');

                    var starData = new StarData
                    {
                        GAIA_id = elements[0],
                        parallax = double.Parse(elements[1], CultureInfo.InvariantCulture),
                        x = double.Parse(elements[2], CultureInfo.InvariantCulture),
                        y = double.Parse(elements[3], CultureInfo.InvariantCulture),
                        z = double.Parse(elements[4], CultureInfo.InvariantCulture)
                    };

                    data.Add(starData);

                    var res = context.Stars.Add(starData);
                }

                context.BulkInsert(data);
            }

            return true;
        }

        public static async Task<bool> AddNASAExoplanetDataToDB(AppDbContext context)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var result = await client.GetAsync(NASA_API_URL);
                    result.EnsureSuccessStatusCode();
                    var responseStream = await result.Content.ReadAsStreamAsync();

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        var jsonData = reader.ReadToEnd();
                        var exoplanetData = JsonConvert.DeserializeObject<List<ExoplanetJsonDeserializeData>>(jsonData);

                        foreach (var ex in exoplanetData)
                        {
                            /*if (context.Exoplanets.Any(x => x.name == ex.pl_name))
                                continue;
*/
                            (double _x, double _y, double _z) = ConvertToCartesian(ex.ra, ex.dec, ex.sy_plx);

                            var exoplanet = new ExoplanetData
                            {
                                name = ex.pl_name,
                                parallax = ex.sy_plx,
                                x = _x,
                                y = _y,
                                z = _z
                            };

                            context.Exoplanets.Add(exoplanet);
                            
                        }
                        context.SaveChanges();
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private static bool CallPythonGAIADataRequester(string path, string query)
        {
            bool success = false;

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = PythonPath;
            start.Arguments = string.Format("{0} {1} \"{2}\"", "dataRequest.py", path, query);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using(Process process = Process.Start(start))
            {
                using(StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);

                    if (result.Contains("SUCCESS"))
                        success = true;
                }
            }

            return success;
        }

        public static bool GetExoplanetStars(string exoplanetName, AppDbContext context)
        {
            var searchResult = context.Exoplanets.Find(exoplanetName);

            if (searchResult == null)
                return false;


            JsonSerializeWrapper<StarJsonSerializeData> stars = new JsonSerializeWrapper<StarJsonSerializeData>();
            var starsData = context.Stars.ToList();

            foreach(var star in starsData)
            {
                stars.items.Add(new StarJsonSerializeData
                {
                    GAIA_id = star.GAIA_id,
                    x = star.x,
                    y = star.y,
                    z = star.z,
                });
            }

            string jsonData = JsonConvert.SerializeObject(stars, Formatting.None);

            File.WriteAllText(Path.Combine(CachePath, $"{exoplanetName}.json"), jsonData);

            return true;
        }

        public static void GetExoplanets(AppDbContext context)
        {
            JsonSerializeWrapper<ExoplanetJsonSerializeData> exoplanetData = new JsonSerializeWrapper<ExoplanetJsonSerializeData>();
            exoplanetData.items = context.Exoplanets.Select(e => new ExoplanetJsonSerializeData
            {
                name = e.name,
                x = e.x,
                y = e.y,
                z = e.z,
            }).ToList();

            string jsonData = JsonConvert.SerializeObject(exoplanetData, Formatting.None);
            File.WriteAllText(Path.Combine(CachePath, "EXOPLANETS.json"), jsonData);
        }

        private static (double, double, double) ConvertToCartesian(double ra, double dec, double parallax)
        {
            double dist = (1 / parallax) * 1000;

            double ra_rad = rad * ra;
            double dec_rad = rad * dec;

            double sin_ra_rad = Math.Sin(ra_rad);
            double cos_ra_rad = Math.Cos(ra_rad);

            double sin_dec_rad = Math.Sin(dec_rad);
            double cos_dec_rad = Math.Cos(dec_rad);

            double x = dist * cos_dec_rad * cos_ra_rad;
            double y = dist * cos_dec_rad * sin_ra_rad;
            double z = dist * sin_dec_rad;

            return (x, y, z);
        }


        private class ExoplanetJsonDeserializeData
        {
            public string pl_name { get; set; }
            public string hostname { get; set; }
            public double ra {  get; set; }
            public double dec { get; set; }
            public double sy_plx { get; set; }
        }

        private class ExoplanetJsonSerializeData
        {
            public string name { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }

        private class StarJsonSerializeData
        {
            public string GAIA_id { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }

        public class JsonSerializeWrapper<T>
        {
            public List<T> items = new List<T>();
        }
    }
}
