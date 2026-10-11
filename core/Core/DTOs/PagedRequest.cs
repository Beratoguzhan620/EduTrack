using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs;

public class PagedRequest
{
    public int Index { get; set; }
    public int Size { get; set; }

    public PagedRequest(int index = 0, int pageSize = 10)
    {
        Index = index;
        Size = pageSize;
    }
}
