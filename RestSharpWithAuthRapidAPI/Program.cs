using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

// HOST
// morning-star.p.rapidapi.com

// KEY
// 5149b58dcamshd89fcbd19484eb2p100c57jsnc25e08335a51

// API Requests
//Request URL: https://morning-star.p.rapidapi.com/market/get-summary
//Request Method: GET

// HEADERS
/*
 
 req.headers({
    "x-rapidapi-host": "morning-star.p.rapidapi.com",
    "x-rapidapi-key": "5149b58dcamshd89fcbd19484eb2p100c57jsnc25e08335a51"
});
 */

namespace RestSharpWithAuthRapidAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            DataGetter runProgram = new DataGetter();

            runProgram.ScrapeContent();
        }
    }


    class DataGetter
    {
        public void ScrapeContent()
        {
            RestClient client = new RestClient("https://morning-star.p.rapidapi.com/market/get-summary");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "morning-star.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "5149b58dcamshd89fcbd19484eb2p100c57jsnc25e08335a51");
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);

            string content = response.Content.ToString();
            // I think the content is coming back empty due to an API change on 9/6/19
            // Most likely issue is the content string is empty - this causes null reference a few lines down

            Console.WriteLine(content);
            Console.WriteLine("API Endpoint Content Converted to String");
            dynamic contentAsJsonObject = JsonConvert.DeserializeObject(content);
            Console.WriteLine("content converted into a JSON object");

            // ERROR LINE BELOW - Cannot perform runtime binding on a null reference
            JArray USAStockDataAsJArray = contentAsJsonObject["MarketRegions"]["USA"];
            Console.WriteLine("JSON object converted into JArray");

            // database table name = USAstockData
            string dataBaseString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = Parsing; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";

            using (SqlConnection db = new SqlConnection(dataBaseString))
            {
                db.Open();

                Console.WriteLine("Database connection to USAstockData successfully opened");

                foreach (JToken stock in USAStockDataAsJArray)
                {
                    SqlCommand insertStatement = new SqlCommand("INSERT INTO db.USAStockData (StockRecord, Symbol, LastPrice, PercentChange, MarketChange) VALUES (@stockRecord, @symbol, @lastPrice, @percentChange, @marketChange)", db);

                    insertStatement.Parameters.AddWithValue("@stockRecord", DateTime.Now);
                    insertStatement.Parameters.AddWithValue("@symbol", stock["Exchange"].ToString());
                    insertStatement.Parameters.AddWithValue("@lastPrice", stock["Price"].ToString());
                    insertStatement.Parameters.AddWithValue("@percentChange", stock["PercentChange"].ToString());
                    insertStatement.Parameters.AddWithValue("@marketChange", stock["PriceChange"].ToString());

                    insertStatement.ExecuteNonQuery();
                }
                Console.WriteLine("Database Updated");
                db.Close();
            }
        }
    }
}
