using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace API
{
    public class SpaceData
    {
        private static string PythonPath = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python39_64\python.exe";
        private static readonly string NASA_API_URL = @"https://exoplanetarchive.ipac.caltech.edu/TAP/sync?query=SELECT+pl_name,hostname,ra,dec,sy_plx+FROM+ps+WHERE+ra+IS+NOT+NULL+AND+dec+IS+NOT+NULL+AND+sy_plx+IS+NOT+NULL+AND+pl_bmasse+%3E+1&format=csv";
        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        public static readonly string StarDataFilePath = Path.Combine(DataPath, "STAR_DATA.csv");
        public static readonly string ExoplanetFilePath = Path.Combine(DataPath, "EXOPLANET_DATA.csv");
        
        public static async Task<bool> RequestStarDataFromGAIA(int numberOfRecords)
        {
            var query = $@"
                SELECT TOP {numberOfRecords}
                    source_id, 
                    ra, 
                    dec,
                    parallax
                FROM gaiadr3.gaia_source
                WHERE ra IS NOT NULL AND dec IS NOT NULL AND parallax is NOT NULL
            ";

            return await Task.Run(() => CallPythonGAIADataRequester(StarDataFilePath, query));
        }

        public static async Task<bool> RequestExoplanetDataFromNASA()
        {
            return await CallHttpNASADataRequester(ExoplanetFilePath);
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

        private static async Task<bool> CallHttpNASADataRequester(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var result = await client.GetAsync(NASA_API_URL);
                    result.EnsureSuccessStatusCode();

                    var responseStream = await result.Content.ReadAsStreamAsync();

                    double rad = (Math.PI / 180);
                    List<string[]> csvData = new List<string[]>();

             
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] values = line.Split(',');
                            csvData.Add(values);
                        }
                    }

                    using (var writer = new StreamWriter(ExoplanetFilePath))
                    {
                        var firstEntry = csvData.First();
                        await writer.WriteLineAsync($"{firstEntry[0]},{firstEntry[1]},{firstEntry[4]},x,y,z");
                        csvData.RemoveAt(0);
                        foreach (var entry in csvData)
                        {
                            double dist = 1 / double.Parse(entry[4], CultureInfo.InvariantCulture);

                            double ra_rad = rad * double.Parse(entry[2], CultureInfo.InvariantCulture);
                            double dec_rad = rad * double.Parse(entry[3], CultureInfo.InvariantCulture);

                            double sin_ra_rad = Math.Sin(ra_rad);
                            double cos_ra_rad = Math.Cos(ra_rad);

                            double sin_dec_rad = Math.Sin(dec_rad);
                            double cos_dec_rad = Math.Cos(dec_rad);

                            StringBuilder sb = new StringBuilder();
                            sb.Append(entry[0]);
                            sb.Append(',');
                            sb.Append(entry[1]);
                            sb.Append(',');
                            sb.Append(entry[4]);
                            sb.Append(',');

                            double x = dist * cos_dec_rad * cos_ra_rad;
                            double y = dist * cos_dec_rad * sin_ra_rad;
                            double z = dist * sin_dec_rad;

                            sb.Append(x.ToString(CultureInfo.InvariantCulture));
                            sb.Append(',');
                            sb.Append(y.ToString(CultureInfo.InvariantCulture));
                            sb.Append(',');
                            sb.Append(z.ToString(CultureInfo.InvariantCulture));
                            sb.Append('\n');

                            await writer.WriteAsync(sb.ToString());
                        }
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
