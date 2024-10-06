using LMS.Logic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using LMS.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;

namespace LMS.Services;

public class ProductService : IProductService
{
    public static List<Product> ProductList = new List<Product>();
    public static List<Product> ShoppingList = new List<Product>();
    public static List<Product> FoodStock = new List<Product>();

    public const string SHOPPING_LIST = "ShoppingList";
    public const string FOOD_STOCK = "FoodStock";

    public static void InitializeLists()
    {
        FoodStock = new List<Product>(LoadFoodStock());
        ShoppingList = new List<Product>(LoadShoppingList());
    }

    public static async Task<Product> GET_Request_Spoonacular(string barcodeID)
    {
        using (var client = new HttpClient())
        {
            // Erste API (Spoonacular)
            var response1 = await client.GetAsync("https://api.spoonacular.com/food/products/upc/" + barcodeID + "?apiKey=16ad760ce37d4c27923d46678fedaa1c");

            string result = await response1.Content.ReadAsStringAsync();
            if (result.Contains("failure"))
            {
                return null;
            }
            JObject jObject = ConvertJsonStringToJsonObject(result);

            Product product = new Product();
            //product name - german
            product.Product_Name = (string)jObject.SelectToken("$.title");
            product.Barcode = long.Parse(barcodeID);
            product.Quantity = 1;
            product.ExpiryDate = DateOnly.FromDateTime(DateTime.Today);
            return product;
        }
    }



    public static async Task<Product> GET_Request_OpenFoodFacts(string barcodeID)
    {
        using (var client = new HttpClient())
        {
            // Erste API (Spoonacular)
            var response2 = await client.GetAsync("https://world.openfoodfacts.net/api/v2/product/" + barcodeID);
            string result = await response2.Content.ReadAsStringAsync();
            if (result.Contains("502 Bad Gateway")) return null;
            JObject jObject = ConvertJsonStringToJsonObject(result);
            int status = Convert.ToInt32((string)jObject.SelectToken("$.status"));
            if (status == 0) return null;

            Product product = new Product();
            //product name - german
            string productName = (string)jObject.SelectToken("$.product.product_name_de");

            if (productName == null)
            {
                productName = (string)jObject.SelectToken("$.product.product_name_fr");
                productName = Translate(productName);
            }
            product.Product_Name = productName;
            product.Barcode = long.Parse(barcodeID);
            product.Quantity = 1;
            product.ExpiryDate = DateOnly.FromDateTime(DateTime.Today);
            return product;
        }
    }

    public static JObject ConvertJsonStringToJsonObject(string jsonString)
    {
        try
        {
            return JObject.Parse(jsonString);
        }
        catch (JsonReaderException ex)
        {
            // Log the exception and the invalid JSON string
            Console.WriteLine($"Invalid JSON string: {jsonString}");
            throw new Exception("Failed to parse JSON", ex);
        }
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
    
    public static void UpdateExpirationDateToProduct(string productName, DateOnly expirationDate, ListTypesEnum.ListTypes listType)
    {
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                ShoppingList.Where(product => product.Product_Name.Equals(productName)).ToList().ForEach(product => product.ExpiryDate = expirationDate);
                FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                //ShoppingListToDictionary();
                break;
            case ListTypesEnum.ListTypes.FoodStock:
                FoodStock.Where(product => product.Product_Name.Equals(productName)).ToList().ForEach(product => product.ExpiryDate = expirationDate);
                FileHandler.SaveList(FoodStock, FOOD_STOCK);
                //FoodStockToDictionary();
                break;
        }
    }

    public static List<Product> SortListByName(List<Product> listToSort)
    {
        listToSort.Sort(CompareProductsByName);
        return listToSort;
    }


    static int CompareProductsByName(Product product1, Product product2)
    {
        if (product1?.Product_Name == null || product2?.Product_Name == null) return 0;
        return product1.Product_Name.CompareTo(product2.Product_Name);
    }

    public static List<Product> SortListByExpirationDate(List<Product> listToSort)
    {
        listToSort.Sort(CompareProductsByExpirationDate);
        return listToSort;
    }

    static int CompareProductsByExpirationDate(Product product1, Product product2)
    {
        if (product1?.ExpiryDate == null || product2?.ExpiryDate == null) return 0;
        return product1.ExpiryDate.CompareTo(product2.ExpiryDate);
    }


    public static bool MoveProductFromFoodStockToShoppingList(long barcode)
    {
        Product productToMove = MoveProduct(barcode, ListTypesEnum.ListTypes.ShoppingList);
        if (productToMove == null) return false; 
        RemoveProduct(productToMove, ListTypesEnum.ListTypes.FoodStock);
        return true;
    }


    public static bool RemoveProduct(long barcode, ListTypesEnum.ListTypes listType)
    {
        Product productToRemove = null;
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                productToRemove = ShoppingList.Find(x => x.Barcode == barcode);
                if (productToRemove == null) return false;

                if (productToRemove.Quantity == 1)
                {
                    ShoppingList.Remove(productToRemove);
                    FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                    return true;
                }
                ShoppingList.Find(x => x.Equals(productToRemove)).Quantity--;
                FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                return true;

            case ListTypesEnum.ListTypes.FoodStock:
                productToRemove = FoodStock.Find(x => x.Barcode == barcode);
                if(productToRemove == null) return false;

                if (productToRemove.Quantity == 1)
                {
                    FoodStock.Remove(productToRemove);
                    FileHandler.SaveList(FoodStock, SHOPPING_LIST);
                    return true;
                }
                FoodStock.Find(x => x.Equals(productToRemove)).Quantity--;
                FileHandler.SaveList(FoodStock, SHOPPING_LIST);
                return true;
        }
        return false;
    }


    public static bool RemoveProduct(Product productToRemove, ListTypesEnum.ListTypes listType)
    {
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                if(productToRemove.Quantity == 1)
                {
                    ShoppingList.Remove(productToRemove);
                    FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                    return true;
                }
                ShoppingList.Find(x => x.Equals(productToRemove)).Quantity--;
                FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                return true;
                
            case ListTypesEnum.ListTypes.FoodStock:
                if (productToRemove.Quantity == 1)
                {
                    FoodStock.Remove(productToRemove);
                    FileHandler.SaveList(FoodStock, SHOPPING_LIST);
                    return true;
                }
                FoodStock.Find(x => x.Equals(productToRemove)).Quantity--;
                FileHandler.SaveList(FoodStock, SHOPPING_LIST);
                return true;
        }
        return false;
    }

    public static Product MoveProduct(long barcode, ListTypesEnum.ListTypes listToMoveProductTo)
    {
        Product productToMove = null;
        switch (listToMoveProductTo)
        {
            case ListTypesEnum.ListTypes.FoodStock:
                productToMove = ShoppingList.Find(x => x.Barcode == barcode);
                if (productToMove == null) return null;
                AddItemToFoodStock(productToMove);
                return productToMove;
            case ListTypesEnum.ListTypes.ShoppingList:
                productToMove = FoodStock.Find(x => x.Barcode == barcode);
                if (productToMove == null) return null;
                AddItemToShoppingList(FoodStock.Find(x => x.Barcode == barcode));
                return productToMove;
        }
        return null;
    }

    public static void MoveProduct(Product productToMove, ListTypesEnum.ListTypes listToMoveProductTo)
    {
        switch (listToMoveProductTo)
        {
            case ListTypesEnum.ListTypes.FoodStock:
                AddItemToFoodStock(ShoppingList.Find(x => x.Equals(productToMove)));
                break;
            case ListTypesEnum.ListTypes.ShoppingList:
                AddItemToShoppingList(FoodStock.Find(x => x.Equals(productToMove)));
                break;
        }
    }


    public static void AddItemToFoodStock(Product item)
    {
        Statistics.IncrementTotalScannedProducts();
        Statistics.IncrementTotalScannedProductsFoodStock();
        List<Product> productsFromDatabase = new List<Product>(FileHandler.GetProductsFromDatabase());

        if (!productsFromDatabase.Exists(x => x.Barcode == item.Barcode))
        {
            productsFromDatabase.Add(item);
            FileHandler.SaveProductToDatabase(item);
        }
        if (!FoodStock.Exists(x => x.Barcode == item.Barcode)) FoodStock.Add(productsFromDatabase.Find(x => x.Barcode == item.Barcode));
        else FoodStock.Find(x => x.Barcode == item.Barcode).Quantity++;

        FileHandler.SaveList(FoodStock, FOOD_STOCK);
    }

    public static void AddItemToShoppingList(Product item)
    {
        Statistics.IncrementTotalScannedProducts();
        Statistics.IncrementTotalScannedProductsShoppingList();
        List<Product> productsFromDatabase = new List<Product>(FileHandler.GetProductsFromDatabase());
        
        if(!productsFromDatabase.Exists(x => x.Barcode == item.Barcode))
        {
            productsFromDatabase.Add(item);
            FileHandler.SaveProductToDatabase(item);
        }
        if(!ShoppingList.Exists(x => x.Barcode == item.Barcode)) ShoppingList.Add(productsFromDatabase.Find(x => x.Barcode == item.Barcode));
        else ShoppingList.Find(x => x.Barcode == item.Barcode).Quantity++;
        
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


    public static string RenameProduct(string productName, string newProductName, ListTypesEnum.ListTypes listType)
    {
        List<Product> productList = new List<Product>();
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                productList = GetShoppingList().Where(product => product.Product_Name.Equals(productName)).ToList();
                FileHandler.SaveList(ShoppingList, SHOPPING_LIST);
                break;

            case ListTypesEnum.ListTypes.FoodStock:
                productList = GetFoodStock().Where(product => product.Product_Name.Equals(productName)).ToList();
                FileHandler.SaveList(FoodStock, FOOD_STOCK);
                break;
        }
        productList[0].Product_Name = newProductName;
        FileHandler.SaveProductToDatabase(productList[0]);
        return newProductName;
    }

    public static List<Product> SearchProduct(string searchString, ListTypesEnum.ListTypes listType)
    {
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                return ShoppingList.FindAll(x => x.Product_Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);

            case ListTypesEnum.ListTypes.FoodStock:
                return FoodStock.FindAll(x => x.Product_Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        return null;
    }


    public static long GetUPC(Product product, ListTypesEnum.ListTypes listType)
    {
        switch (listType)
        {
            case ListTypesEnum.ListTypes.ShoppingList:
                return ShoppingList.First(x => x.Equals(product)).Barcode;// GetShoppingList().Where(product => product.Product_Name.Equals(productName)).ToList();
            default:
                return FoodStock.First(x => x.Equals(product)).Barcode;// GetShoppingList().Where(product => product.Product_Name.Equals(productName)).ToList();
        }
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
        if (productList.Count == 0) return DateOnly.MinValue;
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