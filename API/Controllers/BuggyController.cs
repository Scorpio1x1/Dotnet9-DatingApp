using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    [HttpGet("auth")]
    public IActionResult getAuth()
    {
        return Unauthorized();
    }

    [HttpGet("not-found")]
    public IActionResult getNotFound()
    {
        return NotFound();
    }

    [HttpGet("bad-request")]
    public IActionResult getBadRequest()
    {
        return BadRequest("This was not a good request");
    }

    [HttpGet("server-error")]
    public IActionResult getServerError()
    {
        throw new Exception("This was a server error");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-secret")]
    public ActionResult<string> GetSecretAdmin()
    {
        return Ok("Only admins should see this");
    }

}
