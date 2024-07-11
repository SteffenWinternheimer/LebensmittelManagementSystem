﻿using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

namespace LMS.Services;

public class ProductService : IProductService
{
    public static List<Product> ProductList = new List<Product>();

    public static void AddItem(Product product)
    {
        ProductList.Add(product);
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
        Statistics.TotalScannedProducts++;
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