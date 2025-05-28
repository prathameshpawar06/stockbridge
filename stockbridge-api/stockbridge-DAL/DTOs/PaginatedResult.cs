using stockbridge_DAL.domainModels;
using System;
using System.Collections.Generic;

namespace stockbridge_DAL.DTOs;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalPages { get; set; }
}
