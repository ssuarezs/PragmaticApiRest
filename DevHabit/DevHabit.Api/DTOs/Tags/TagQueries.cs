using System.Linq.Expressions;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto()
    {
        return t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            UpdatedAtUtc = t.UpdatedAtUtc,
            CreatedAtUtc = t.CreatedAtUtc
        };
    }
}
