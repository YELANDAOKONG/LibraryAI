using System.Net;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Nerdbank.Streams;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace ConsoleWebMCP.Controllers;

[ApiController]
[Route("[controller]")]
public class McpController : ControllerBase
{
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<McpController> _logger;
 
    public McpController(IHttpContextAccessor httpContextAccessor, ILogger<McpController> logger)
    {
        _accessor = httpContextAccessor;
        _logger = logger;
    }
}