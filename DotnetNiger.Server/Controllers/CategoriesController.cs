using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(new { Success = true, Data = categories });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await categoryService.GetByIdAsync(id);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var c = await categoryService.GetBySlugAsync(slug);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    [HttpPost]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.CreateAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { Success = true, Data = c });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var c = await categoryService.UpdateAsync(id, request.Name, request.Description);
        if (c is null) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Data = c });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await categoryService.DeleteAsync(id);
        if (!deleted) return NotFound(new { Success = false, Message = Messages.Category.NotFound });
        return Ok(new { Success = true, Message = Messages.Category.Deleted });
    }
}
