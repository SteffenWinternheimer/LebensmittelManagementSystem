using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

namespace LMS.Services;

public class TodoService : ITodoService
{
    private readonly IList<TodoItem> _todoItems;
    public static List<TodoItem> TodoItems = new List<TodoItem>();

    public TodoService()
    {
        _todoItems = new List<TodoItem> {
            new TodoItem("Wash Clothes"),
            new TodoItem("Clean Desk")
        };
    }
    public static void AddItem(TodoItem item)
    {
        TodoItems.Add(item);
    }


    public static async Task<string> GET_Request(string barcodeID)
    {
        using (var client = new HttpClient())
        {
            using (var response = await client.GetAsync("https://world.openfoodfacts.net/api/v2/product/" + barcodeID))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    public static JObject ConvertJsonStringToJsonObject(string jsonString)
    {
        return JObject.Parse(jsonString);
    }

    public static Product ConvertJSONToProduct(JObject jObject)
    {
        Product product = new Product();
        //product name - german
        string productName = (string)jObject.SelectToken("$.product.product_name_de");

        if (productName == null)
        {
            productName = (string)jObject.SelectToken("$.product.product_name_fr");
            productName = Translate(productName);
        }

        product.Product_Name = productName;
        return product;
    }

    public static string Translate(string word)
    {
        var toLanguage = "de";//English
        var fromLanguage = "fr";//Deutsch
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(word)}";

        var webClient = new WebClient
        {
            Encoding = System.Text.Encoding.UTF8
        };
        var result = webClient.DownloadString(url);
        try
        {
            result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
            return result;
        }
        catch
        {
            return "Error";
        }
    }

    public void Add(TodoItem item)
    {
        _todoItems.Add(item);
    }

    public IEnumerable<TodoItem> GetAllItems()
    {
        return TodoItems;
    }

    public IEnumerable<TodoItem> GetAll()
    {
        return _todoItems.ToList();
    }

    public void Delete(TodoItem item)
    {
        _todoItems.Remove(item);
    }

    public void Complete(TodoItem item)
    {
        item.Completed = true;
    }

    public void Uncomplete(TodoItem item)
    {
        item.Completed = false;
    }
}