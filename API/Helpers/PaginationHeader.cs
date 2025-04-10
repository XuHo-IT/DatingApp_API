using System;

namespace API.Helpers;

public class PaginationHeader(int curretnPage, int itemsPerPage, int totalItems, int totalPages)
{
  public int CurrentPage { get; set; } = curretnPage;

  public int ItemsPerPage { get; set; } = itemsPerPage;

  public int TotalItems { get; set; } = totalItems;

  public int ToatalPages { get; set; } = totalPages;
}
