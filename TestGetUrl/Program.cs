using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using Newtonsoft.Json;
namespace TestGetUrl
{
    class Program
    {
        static string dbInUrl = "nuccore";
        static string term = "bovine coronavirus";
        static int retstrart = 0;
        static int minLength;
        static int maxLength;
        static void Main(string[] args)
        {
            Console.WriteLine("Введите базу данных: ");
            dbInUrl = Console.ReadLine();
            Console.WriteLine("Введите запрос: ");
            term = Console.ReadLine();
            Console.WriteLine("Введите мин длину: ");
           
            minLength = int.Parse(Console.ReadLine());
            Console.WriteLine("Введите мax длину: ");
            maxLength = int.Parse(Console.ReadLine());
            DeserializeQueryObject(dbInUrl, ConstructTermString(term,minLength,maxLength));
        }
        public static string ConstructTermString(string term, int? minLength = null, int? maxLength=null) 
        {
            if (minLength != null && maxLength != null)
            {
                term = term + $"[All Fields] AND (\"{minLength}\"[SLEN] : \"{maxLength}\"[SLEN])";
                return term;
            }
            else { return term; }
        }

        public static List<NuccoreObject> DeserializeQueryObject(string DataBaseName, string term) 
        {
            var urlEsearch = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db="
                + DataBaseName + "&term=" + term + "&usehistory=y&retmode=json";
            var responseFromEsearch = GetAnswer(urlEsearch);
            ESearchResult WebEnv = JsonConvert.DeserializeObject<ESearchResult>(responseFromEsearch);
            List<NuccoreObject> nuccoreObjects = new List<NuccoreObject>();
            int i = 1;
            int ElementCount;
            do { 
            var urlEsummary = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esummary.fcgi?db="
                + DataBaseName + "&term=" + term + "&usehistory=y&WebEnv="
                + WebEnv.Response.WebEnvironment + 
                "&query_key=1&retmode=text&rettype=docsum&retmax=500&retstart="+retstrart;//Paste ur url here  
            var responseFromEsummary = GetAnswer(urlEsummary);
                 ElementCount = WebEnv.Response.Count;

           XmlSerializer serializer = new XmlSerializer(typeof(eSummaryResult));
            eSummaryResult DeserializeResult;
            using (TextReader reader = new StringReader(responseFromEsummary))
            {
                DeserializeResult = (eSummaryResult)serializer.Deserialize(reader);
            }
           
           
                foreach (var element in DeserializeResult.DocSum)
                {
                    NuccoreObject nuccoreObject = new NuccoreObject();
                    nuccoreObject.Id = element.Id;
                    nuccoreObject.Name = element.Item[1].Value;
                    nuccoreObject.Accession = element.Item[0].Value;
                    nuccoreObject.Length = element.Item[8].Value;
                    nuccoreObjects.Add(nuccoreObject);
                }
                retstrart++;

                SaveNuccoreObjects(nuccoreObjects);
            } while (nuccoreObjects.Count < ElementCount );
            foreach (var item in nuccoreObjects)
            {
                Console.WriteLine("Номер последовательности: " + i);
                Console.WriteLine(item.Accession);
                Console.WriteLine(item.Length);
                Console.WriteLine(item.Name);
                i++;
            }
            return nuccoreObjects;
        }

        public static List<NuccoreObject> SaveNuccoreObjects(List<NuccoreObject> nuccoreObjects) 
        {
            //Подразумевается импорт в бд
            return nuccoreObjects;
        }
        public static string GetAnswer(string url) 
        {
            WebRequest request = WebRequest.Create(url);
            string responseText;
            request.Timeout = -1;
            WebResponse response =  request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream())) 
            {
                 responseText = reader.ReadToEnd();
            }
            response.Close();
            return responseText;

        }
    }
}
