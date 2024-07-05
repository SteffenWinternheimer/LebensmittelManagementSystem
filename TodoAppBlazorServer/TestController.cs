using Microsoft.AspNetCore.Mvc;

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
            var response = new { message = "Hello from POST", receivedData = data };
            return Ok(response);
        }
    }
}