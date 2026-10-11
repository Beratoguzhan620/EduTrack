using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Results;

public class Paged<T> 
{
    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IList<T> Items { get; set; } = new List<T>();
    public bool HasPrevious => Index > 0;
    public bool HasNext => Index < Pages - 1;
}
