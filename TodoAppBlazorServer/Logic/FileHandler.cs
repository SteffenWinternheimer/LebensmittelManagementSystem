using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LMS.Logic
{
    public class FileHandler
    {
        public const string STATISTICS_PATH = @"F:\TH-Bingen\_Bachelorarbeit\User_Data\statistics.json";
        public const string SHOPPING_LIST_PATH = @"F:\TH-Bingen\_Bachelorarbeit\User_Data\shoppingList.json";
        public const string FOOD_STOCK_PATH = @"F:\TH-Bingen\_Bachelorarbeit\User_Data\foodStock.json";
        public static void SaveStatistics(Dictionary<string, int> statistics)
        {
            string json = JsonConvert.SerializeObject(statistics);
            File.WriteAllText(STATISTICS_PATH, json);
        }

        public static Dictionary<string, int> LoadStatistics()
        {
            if (!File.Exists(STATISTICS_PATH)) return new Dictionary<string, int>();
            string jsonString = File.ReadAllText(STATISTICS_PATH);
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);
           
        }

        public static void SaveList(List<Product> listToSave, string listName)
        {
            string json = JsonConvert.SerializeObject(listToSave);
            File.WriteAllText("F:\\TH-Bingen\\_Bachelorarbeit\\User_Data\\" + listName + ".json", json);
        }

        public static List<Product> LoadList(string listName)
        {
            if (!File.Exists("F:\\TH-Bingen\\_Bachelorarbeit\\User_Data\\" + listName + ".json")) return new List<Product>();
            string fileContent = File.ReadAllText("F:\\TH-Bingen\\_Bachelorarbeit\\User_Data\\" + listName + ".json");
            JArray jArrayList = JArray.Parse(fileContent);
            List<Product> productList = new List<Product>();
            foreach (JObject jObject in jArrayList)
            {
                productList.Add(jObject.ToObject<Product>());
            }
            return productList;
        }
    }
}
