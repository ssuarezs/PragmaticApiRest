﻿using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public sealed class TagController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        List<TagDto> tags = await dbContext
            .Tags
            .Select(TagQueries.ProjectToDto())
            .ToListAsync();

        var tagsCollectionDto = new TagsCollectionDto
        {
            Data = tags
        };

        return Ok(tagsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id)
    {
        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id)
            .Select(TagQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
        {
            return NotFound();
        }

        return Ok(tag);
    }
    
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(
        CreateTagDto createTagDto, 
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory)
    {
        ValidationResult validationResult = await validator.ValidateAsync(createTagDto);
        
        if (!validationResult.IsValid)
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());
            return BadRequest(problem);
        }
        
        Tag tag = createTagDto.ToEntity();
        
        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(
                detail: $"Tag with name {tag.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }
        
        dbContext.Tags.Add(tag);
        
        await dbContext.SaveChangesAsync();

        TagDto tagDto = tag.ToDto();
        
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tagDto);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, [FromBody] UpdateTagDto updateTagDto)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id);
        
        if (tag is null)
        {
            return NotFound();
        }
        
        tag.UpdateFromDto(updateTagDto);
        
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id);
        
        if (tag is null)
        {
            return NotFound();
        }
        
        dbContext.Tags.Remove(tag);
        
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
}
