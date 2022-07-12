using Microsoft.AspNetCore.Mvc;
using NEventStorePOC.Model;
using NEventStorePOC.Services;
using NEventStorePOC.Framework;
using Newtonsoft.Json;
using NEventStore;

namespace NEventStorePOC.Controllers;

[Controller]
[Route("api/[controller]")]
public class SQLCommandController: Controller
{
    private readonly SQLDBService _sqlDBService;

    public SQLCommandController(SQLDBService sqlDBService)
    {
        _sqlDBService = sqlDBService;
    }

    [HttpPost]
    public async Task<IActionResult> Save(CommandJSON commandJson)
    {
        commandJson.Command = GetSampleCommand();
        await _sqlDBService.AddMessageToDB(commandJson);
        
        return Ok();
    }

    [HttpGet]
    public async Task<List<EventStream>> Get(Guid id)
    {
        return await _sqlDBService.Get(id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(CommandJSON commandJson)
    {
        commandJson.Command = GetSampleCommand();
        await _sqlDBService.AppendMessageToDB(commandJson);
        
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AddSnapshot(Guid id)
    {
        await _sqlDBService.TakeSnapshot(id);
        return Ok();
    }

    [HttpPut("snapshot/{id}")]
    public async Task<IActionResult> LoadFromSnapshotForwardAndAppend(CommandJSON commandJson)
    {
        commandJson.Command = GetSampleCommand();
        await _sqlDBService.LoadFromSnapshotForwardAndAppend(commandJson);
        
        return Ok();
    }

    private Command GetSampleCommand()
    {
        AccountDetails account = new AccountDetails();
        using (StreamReader r = new StreamReader(".\\Model\\SampleJSON.json"))
        {
            string json = r.ReadToEnd();
            account = JsonConvert.DeserializeObject<AccountDetails>(json);
        }

        Command command = new Command{
            Id = new Guid(),
            Identity = "manu.radhakrishnan@experionglobal.com",
            IdentityName = "Manu Radhakrishnan",
            Version = 1,
            StationNumber= "TEST",
            ActiveTime = TimeSpan.FromSeconds(1),
            KeystrokeCount = 0,
            MouseClickCount = 0,
            ScheduledTime= DateTime.MinValue,
            Account = account
        };

        return command;
    }
}