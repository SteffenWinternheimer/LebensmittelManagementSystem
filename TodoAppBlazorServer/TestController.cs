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
            Task<string> result = TodoService.GET_Request(data);
            result.Wait();
            JObject responseJSON = TodoService.ConvertJsonStringToJsonObject(result.Result);
            int status = Convert.ToInt32((string)responseJSON.SelectToken("$.status"));
            Product product = null;
            product = TodoService.ConvertJSONToProduct(responseJSON);
            TodoService.AddItem(new TodoItem(product.Product_Name));
            return Ok(status);
        }
    }
}