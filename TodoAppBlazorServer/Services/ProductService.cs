using LMS.Logic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using LMS.Models;

namespace LMS.Services;

public class ProductService : IProductService
{
    public static List<Product> ProductList = new List<Product>();
    public static List<Product> ShoppingList = new List<Product>();
    public static List<Product> FoodStock = new List<Product>();

    public static Dictionary<string, int> ProductQuantityShoppingList = new Dictionary<string, int>();
    public static Dictionary<string, int> ProductQuantityFoodStock = new Dictionary<string, int>();

    public const string SHOPPING_LIST = "ShoppingList";
    public const string FOOD_STOCK = "FoodStock";

    public static void InitializeLists()
    {
        FoodStock = new List<Product>(LoadFoodStock());
        ShoppingList = new List<Product>(LoadShoppingList());
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

    public static bool RemoveProduct(string productName, ListTypesEnum.ListTypes listType)
    {
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                ShoppingList.Remove(ShoppingList.Find(item => item.Product_Name.Equals(productName)));
                FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                ShoppingListToDictionary();
                return true;
            case ListTypesEnum.ListTypes.FoodStock:
                FoodStock.Remove(FoodStock.Find(item => item.Product_Name.Equals(productName)));
                FileHandler.SaveList(FoodStock, FOOD_STOCK);
                FoodStockToDictionary();
                return true;
        }
        return false;
    }

    public static void MoveProduct(string productName, ListTypesEnum.ListTypes listToMoveProductTo)
    {
        switch (listToMoveProductTo)
        {
            case ListTypesEnum.ListTypes.FoodStock:
                AddItemToFoodStock(new Product(productName));
                Console.WriteLine("Move Product to Food Stock");
                break;
            case ListTypesEnum.ListTypes.ShoppingList:
                AddItemToShoppingList(new Product(productName));
                Console.WriteLine("Move Product to Shopping List");
                break;
        }
    }


    public static void AddItemToFoodStock(Product item)
    {
        Statistics.IncrementTotalScannedProducts();
        Statistics.IncrementTotalScannedProductsFoodStock();
        FoodStock.Add(item);
        FileHandler.SaveList(FoodStock, FOOD_STOCK);
    }

    public static void AddItemToShoppingList(Product item)
    {
        Statistics.IncrementTotalScannedProducts();
        Statistics.IncrementTotalScannedProductsShoppingList();
        ShoppingList.Add(item);
        FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
    }

    public static IEnumerable<Product> LoadShoppingList()
    {
        return FileHandler.LoadList(SHOPPING_LIST);
    }

    public static IEnumerable<Product> LoadFoodStock()
    {
        return FileHandler.LoadList(FOOD_STOCK);
    }

    public static IEnumerable<Product> GetShoppingList()
    {
        return ShoppingList;
    }
    public static IEnumerable<Product> GetFoodStock()
    {
        return FoodStock;
    }

    public static DateOnly GetLatestExpirationDate(string productName, ListTypesEnum.ListTypes listType)
    {
        List<Product> productList = new List<Product>();
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                productList = GetShoppingList().Where(product => product.Product_Name.Equals(productName)).ToList();
                break;

            case ListTypesEnum.ListTypes.FoodStock:
                productList = GetFoodStock().Where(product => product.Product_Name.Equals(productName)).ToList();
                break;
        }

        DateOnly latestExpirationDate = productList[0].ExpiryDate;
        foreach (var product in productList)
        {
            if (product.ExpiryDate > latestExpirationDate)
            {
                latestExpirationDate = product.ExpiryDate;
            }
        }
        return latestExpirationDate;
    }


    public static void ShoppingListToDictionary()
    {
        ProductQuantityShoppingList.Clear();
        //find diplicates in shopping list (Product_Name)
        var duplicates = ShoppingList.GroupBy(x => x.Product_Name)
            .Where(group => group.Count() > 0)
            .Select(group => group.Key);
        //show quantity of duplicates and product name of each duplicate
        foreach ( var item in duplicates) {
            int quantity = ShoppingList.Count(x => x.Product_Name == item);
            ProductQuantityShoppingList.Add(item.ToString(), quantity);
        }
    }

    public static void FoodStockToDictionary()
    {
        ProductQuantityFoodStock.Clear();
        //find diplicates in shopping list (Product_Name)
        var duplicates = FoodStock.GroupBy(x => x.Product_Name)
            .Where(group => group.Count() > 0)
            .Select(group => group.Key);
        //show quantity of duplicates and product name of each duplicate
        foreach (var item in duplicates)
        {
            int quantity = FoodStock.Count(x => x.Product_Name == item);
            ProductQuantityFoodStock.Add(item.ToString(), quantity);
        }
    }

    public void Add(Product item)
    {
        ProductList.Add(item);
    }

    public IEnumerable<Product> GetAllItems()
    {
        return ProductList;
    }

    public IEnumerable<Product> GetAll()
    {
        return ProductList.ToList();
    }

    public void Delete(Product item)
    {
        ProductList.Remove(item);
    }

}