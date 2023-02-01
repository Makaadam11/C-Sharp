using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using CsvHelper;
using System.Text;

namespace ReadJsonFromUrl
{
    public class Interns
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Invalid command: please provide a command (count or max-age) and a URL.");
                return;
            }

          
            var command = args[0];
            var url = args[1];

            string format = GetFormatFromUrl(url);
            Console.WriteLine("Format: " + format);

            Console.WriteLine("Command: " + command);

            if (format == "csv") {

                if (command == "count")
                {
                    var count = await GetInternCountCSV(url, args,null);
                    Console.WriteLine("count: " + count);
                    return;
                }

                else if (command == "max-age")
                {
                    var maxAge = await GetMaxAgeCSV(url,null);
                    Console.WriteLine("maxAge: "+ maxAge);
                    return;
                }
                else 
                {
                    Console.WriteLine("Error: Invalid command. Please use either count or max-age.");
                    return;
                }
            }
            else if (format == "json") {

                if (command == "count")
                {
                    var count = await GetInternCountJson(url, args);
                    Console.WriteLine("count: " + count);
                    return;
                }

                else if (command == "max-age")
                {
                    var maxAge = await GetMaxAgeJson(url);
                    Console.WriteLine("maxAge: " + maxAge);
                    return;
                }
                else
                {
                    Console.WriteLine("Error: Invalid command. Please use either count or max-age.");
                    return;
                }
            }
            else if(format == "zip")
            {
                if (command == "count")
                {
                    var count = await GetInternCountZIP(url, args);
                    Console.WriteLine("count: " + count);
                    return;
                }

                else if (command == "max-age")
                {
                    var maxAge = await GetMaxAgeZIP(url);
                    Console.WriteLine("maxAge: " + maxAge);
                    return;
                }
                else 
                {
                    Console.WriteLine("Error: Invalid command. Please use either count or max-age.");
                    return;
                }
            }
        }
        private static string GetFormatFromUrl(string url)
        {
            string format = url.Substring(url.LastIndexOf(".") + 1);
            return format;
        }

        private static async Task<int> GetInternCountZIP(string url, string[] args)
        {
            int count = 0;
            byte[] content = null;
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot get file: " + ex.Message);
            }

            using (var memoryStream = new MemoryStream(content))
            using (var archive = new ZipArchive(memoryStream))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".csv"))
                    {
                        using (var entryStream = entry.Open())
                        using (var reader = new StreamReader(entryStream))
                        {
                            var unzipped_content = reader.ReadToEnd();

                            if (unzipped_content == null)
                            {
                                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .zip file.");
                                return 0;
                            }

                            count = await GetInternCountCSV(null, args, unzipped_content);
                           
                        }
                    }
                }

            }
            return count;
        }
        private static async Task<int> GetMaxAgeZIP(string url)
        {
            int count = 0;
            byte[] content = null;
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot get file: " + ex.Message);
            }


            using (var memoryStream = new MemoryStream(content))
            using (var archive = new ZipArchive(memoryStream))
            {
                
                foreach (var entry in archive.Entries)
                {
                    
                    if (entry.FullName.EndsWith(".csv"))
                    {
                        using (var entryStream = entry.Open())
                        using (var reader = new StreamReader(entryStream))
                        {
                            
                            var unzipped_content = reader.ReadToEnd();
                            
                            if (unzipped_content== null)
                            {
                                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .zip file.");
                                return 0;
                            }

                            count = await GetMaxAgeCSV(null, unzipped_content);
                            
                        }
                    }
                    
                }

            }
            return count;
        }
        
        private static async Task<int> GetInternCountCSV(string url, string[] args, string unzipped)
        {
            InternData data;
            List<Intern> interns = new List<Intern>();
            
            if (unzipped == null) {


                byte[] content = null;
                try
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsByteArrayAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Cannot get file: " + ex.Message);
                }

                var contentString = Encoding.UTF8.GetString(content);
                var reader = new StringReader(contentString);



                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var intern = new Intern
                        {
                            Id = csv.GetField<int>("interns/id"),
                            Age = csv.GetField<int>("interns/age"),
                            Name = csv.GetField("interns/name"),
                            Email = csv.GetField("interns/email"),
                            InternshipStart = csv.GetField("interns/internshipStart"),
                            InternshipEnd = csv.GetField("interns/internshipEnd")
                        };
                        interns.Add(intern);
                    }
                }


                data = new InternData { Interns = interns.ToArray() };
   

            }
            else 
            {
                using (StringReader reader = new StringReader(unzipped))
                {
                    bool firstLine = true;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (firstLine)
                        {
                            firstLine = false;
                            continue;
                        }

                        string[] values = line.Split(',');
                        Intern intern = new Intern
                        {
                            Id = int.Parse(values[0]),
                            Age = int.Parse(values[1]),
                            Name = values[2],
                            Email = values[3],
                            InternshipStart = values[4],
                            InternshipEnd = values[5]
                        };
                        interns.Add(intern);
                    }
                }
                data = new InternData { Interns = interns.ToArray() };

               
            }

            if (data.Interns == null)
            {
                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .csv file.");
                return 0;
            }

            int ageThreshold = 0;
            int count = 0;

            int greaterThan = 1;

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--age-gt")
                {
                    greaterThan = 2;
                }
                else if (args[i] == "--age-lt")
                {
                    greaterThan = 0;
                }
                else
                {
                    ageThreshold = int.Parse(args[i]);

                    break;
                }
            }

            if (greaterThan != 1)
            {

                foreach (var intern in data.Interns)
                {
                    if (greaterThan == 2)
                    {
                        if (intern.Age > ageThreshold)
                        {
                            count++;
                        }
                    }
                    else if (greaterThan == 0)
                    {
                        if (intern.Age < ageThreshold)
                        {
                            count++;
                        }
                    }

                }
            }
            else
            {
                foreach (var intern in data.Interns)
                {
                    count++;
                }
            }
            return count;

        }
        private static async Task<int> GetMaxAgeCSV(string url,string unzipped)
        {
            InternData data;
            List<Intern> interns = new List<Intern>();

            if (unzipped == null)
            {
                byte[] content = null;
                try
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsByteArrayAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Cannot get file: " + ex.Message);
                }

                var contentString = Encoding.UTF8.GetString(content);
                var reader = new StringReader(contentString);


                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var intern = new Intern
                        {
                            Id = csv.GetField<int>("interns/id"),
                            Age = csv.GetField<int>("interns/age"),
                            Name = csv.GetField("interns/name"),
                            Email = csv.GetField("interns/email"),
                            InternshipStart = csv.GetField("interns/internshipStart"),
                            InternshipEnd = csv.GetField("interns/internshipEnd")
                        };
                        interns.Add(intern);
                    }
                }


                data = new InternData { Interns = interns.ToArray() };

               
            }
            else
            {
                using (StringReader reader = new StringReader(unzipped))
                {
                    bool firstLine = true;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (firstLine)
                        {
                            firstLine = false;
                            continue;
                        }

                        string[] values = line.Split(',');
                        Intern intern = new Intern
                        {
                            Id = int.Parse(values[0]),
                            Age = int.Parse(values[1]),
                            Name = values[2],
                            Email = values[3],
                            InternshipStart = values[4],
                            InternshipEnd = values[5]
                        };
                        interns.Add(intern);
                    }
                }
                data = new InternData { Interns = interns.ToArray() };

              
            }
            

            if (data.Interns != null)
            {
                int maxAge = 0;
                foreach (var intern in data.Interns)
                {
                    if (intern.Age > maxAge)
                    {
                        maxAge = intern.Age;
                    }
                }

                return maxAge;
            }
            else
            {
                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .csv file.");
                return 0;
            }
         
        }
        private static async Task<int> GetInternCountJson(string url, string[] args)
             {
            InternData data;

            byte[] content = null;
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot get file: " + ex.Message);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
           

            try
            {
                data = JsonSerializer.Deserialize<InternData>(content, options);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Cannot process the file: " + e.Message);
                return 0;
            }

            if (data.Interns == null)
            {
                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .json file.");
                return 0;
            }

            int ageThreshold = 0;
            int count = 0;
            int greaterThan = 1;
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--age-gt")
                {
                    greaterThan = 2;
                }
                else if (args[i] == "--age-lt")
                {
                    greaterThan = 0;
                }
                else
                {
                    ageThreshold = int.Parse(args[i]);
               
                    break;
                }
            }

            if (greaterThan != 1) {

                foreach (var intern in data.Interns)
                {
                    if (greaterThan == 2)
                    {
                        if (intern.Age > ageThreshold)
                        {
                            count++;
                        }
                    }
                    else if (greaterThan == 0)
                    {
                        if (intern.Age < ageThreshold)
                        {
                            count++;
                        }
                    }

                }
            }
            else
            {
                foreach (var intern in data.Interns)
                {
                    count++;
                }
            }
            return count;
        }

        private static async Task<int> GetMaxAgeJson(string url)
        {
            InternData data;

            byte[] content = null;
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot get file: " + ex.Message);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
      

            try
            {
                data = JsonSerializer.Deserialize<InternData>(content, options);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Cannot process the file: " + e.Message);
                return 0;
            }

            if (data.Interns == null)
            {
                Console.WriteLine("No data was obtained from the .json file.");
                return 0;
            }

            if (data.Interns != null)
            {
                int maxAge = 0;
                foreach (var intern in data.Interns)
                {
                    if (intern.Age > maxAge)
                    {
                        maxAge = intern.Age;
                    }
                }

                return maxAge;
            }
            else
            {
                Console.WriteLine("Error: Cannot process the file: no data was obtained from the .json file.");
                return 0;
            }

        }
        
        class InternData
        {
            public Intern[] Interns { get; set; }
        }

        class Intern
        {
            public int Id { get; set; }
            public int Age { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string InternshipStart { get; set; }
            public string InternshipEnd { get; set; }
        }


    }
}
    
