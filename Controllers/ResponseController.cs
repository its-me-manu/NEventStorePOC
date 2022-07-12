using Microsoft.AspNetCore.Mvc;
using NEventStorePOC.Model;
using NEventStorePOC.Services;

namespace NEventStorePOC.Controllers;

[Controller]
[Route("api/[controller]")]
public class ResponseController: Controller
{
    private readonly MongoDBService _mongoDBService;

    public ResponseController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }
    

    [HttpGet]
    public async Task<List<SampleResponse>> Get()
    {

        return await _mongoDBService.GetAllAsync();
    }

    [HttpPost]
    public async Task<IActionResult> AddResponses(SampleResponse response)
    {
        await _mongoDBService.CreateAsync(response);
        return CreatedAtAction(nameof(Get), new {id = response.Id}, response);


    }

    [HttpPut("id")]
    public async Task<IActionResult> UpdateResponses(long id, SampleResponse response)
    {
        
        return Ok();
    }

    [HttpDelete("id")]
    public async Task<IActionResult> DeleteResponses(long id)
    {
        await _mongoDBService.DeleteAsync(id);
        return Ok();
    }
}