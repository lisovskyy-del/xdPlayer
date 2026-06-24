using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Application.Helpers;

public class LibraryFilter
{
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public bool? IsLiked { get; set; }
}