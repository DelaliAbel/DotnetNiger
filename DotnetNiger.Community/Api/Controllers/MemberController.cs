// using DotnetNiger.Community.Application.Services;
// using Microsoft.AspNetCore.Mvc;
// using DotnetNiger.Community.Application.Services.Interfaces;

// namespace DotnetNiger.Community.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class MemberController : ControllerBase
// {
//     private readonly IMemberService _memberService;

//     public MemberController(IMemberService memberService)
//     {
//         _memberService = memberService;
//     }

//     [HttpGet("active")]
//     public async Task<IActionResult> GetActiveMembers()
//     {
//         var members = await _memberService.GetActiveMembersAsync();
//         return Ok(new { data = members });
//     }
// }
