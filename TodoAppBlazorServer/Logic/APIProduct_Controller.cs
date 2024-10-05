using LMS.Services;
using LMS.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace LMS.Logic
{
    [Route("/")]
    [ApiController]
    public class APIProduct_Controller : ControllerBase
    {
        [HttpGet("getdata")]
        public IActionResult GetData()
        {
            return Ok(new { data = new { foo = "blablub" } });
        }

        //[HttpGet("getdata2")]
        //public IActionResult GetData2()
        //{
        //    return Ok(new { data = new { foo = "blablub2" } });
        //}

        //[HttpPost("postdata")]
        //public IActionResult PostData([FromBody] string data)
        //{
        //    var response = new { receivedData = data };
        //    Task<string> result = ProductService.GET_Request(data);
        //    result.Wait();
        //    JObject responseJSON = ProductService.ConvertJsonStringToJsonObject(result.Result);
        //    int status = Convert.ToInt32((string)responseJSON.SelectToken("$.status"));
        //    Product product = null;
        //    product = ProductService.ConvertJSONToProduct(responseJSON);
        //    ProductService.AddItem(new Product(product.Product_Name));
        //    return Ok(status);
        //}

        [HttpPost("PostProductToShoppingList")]
        public IActionResult PostProductToShoppingList([FromBody] string data)
        {
            var response = new { receivedData = data };
            Task<Product> product = ProductService.GET_Request_Spoonacular(data);
            if(product.Result == null) product = ProductService.GET_Request_OpenFoodFacts(data);

            if (product.Result == null)
            {
                Product emptyProduct = new Product("None", long.Parse(data), DateOnly.FromDateTime(DateTime.Today));
                ProductService.AddItemToShoppingList(emptyProduct);
                return BadRequest("Product not found!");
            }
            ProductService.AddItemToShoppingList(product.Result);
            return Ok(product.Result.Product_Name);
        }

        [HttpPost("PostProductToFoodStock")]
        public IActionResult PostProductToFoodStock([FromBody] string data)
        {
            var response = new { receivedData = data };
            Task<Product> product = ProductService.GET_Request_Spoonacular(data);
            if (product.Result == null) product = ProductService.GET_Request_OpenFoodFacts(data);

            if (product.Result == null)
            {
                Product emptyProduct = new Product("None", long.Parse(data), DateOnly.FromDateTime(DateTime.Today));
                ProductService.AddItemToFoodStock(emptyProduct);
                return BadRequest("Product not found!");
            }
            ProductService.AddItemToFoodStock(product.Result);
            return Ok(product.Result.Product_Name);
        }

        [HttpPost("MoveProductFromFoodStockToShoppingList")]
        public IActionResult MoveProductFromFoodStockToShoppingList([FromBody] string data)
        {
            var response = new { receivedData = data };
            bool IsSuccess = ProductService.MoveProductFromFoodStockToShoppingList(long.Parse(data));
            if(IsSuccess)
            {
                return Ok("Product moved successfully!");
            }
            return BadRequest("Product not found in Food Stock!");
        }


        [HttpPost("DeleteProductFromFoodStock")]
        public IActionResult DeleteProductFromFoodStock([FromBody] string data)
        {
            var response = new { receivedData = data };
            bool IsSuccess = ProductService.RemoveProduct(long.Parse(data), Models.ListTypesEnum.ListTypes.FoodStock);
            if (IsSuccess)
            {
                return Ok("Product removed successfully!");
            }
            return BadRequest("Product not found in Food Stock!");
        }
    }
}