using stockbridge_DAL.domainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace stockbridge_DAL.DTOs;

public class ClientFilterRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
    public int PageNumber { get; set; }

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; }

    public string? SearchQuery { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsRetainerAccount { get; set; }
    public bool? IsNonActive { get; set; }
    public bool? IsNonRetainer { get; set; }
    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;
}