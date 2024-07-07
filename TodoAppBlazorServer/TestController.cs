using LMS.Services;
using LMS.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace LMS
{
    [Route("/")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("getdata")]
        public IActionResult GetData()
        {
            return Ok(new { data = new { foo = "blablub" } });
        }

        [HttpGet("getdata2")]
        public IActionResult GetData2()
        {
            return Ok(new { data = new { foo = "blablub2" } });
        }

        [HttpPost("postdata")]
        public IActionResult PostData([FromBody] string data)
        {
            var response = new { receivedData = data };
            Task<string> result = ProductService.GET_Request(data);
            result.Wait();
            JObject responseJSON = ProductService.ConvertJsonStringToJsonObject(result.Result);
            int status = Convert.ToInt32((string)responseJSON.SelectToken("$.status"));
            Product product = null;
            product = ProductService.ConvertJSONToProduct(responseJSON);
            ProductService.AddItem(new Product(product.Product_Name));
            return Ok(status);
        }
    }
}