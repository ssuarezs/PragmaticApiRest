﻿using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public HabitType? Type { get; set; }
    public HabitStatus? Status { get; set; }
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    [FromHeader(Name = "Accept")]
    public string? Accept { get; set; }
}
