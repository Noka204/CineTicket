﻿using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class LoaiPhim
{
    public int MaLoaiPhim { get; set; }

    public string TenLoaiPhim { get; set; } = null!;

    public virtual ICollection<Phim> Phims { get; set; } = new List<Phim>();
}
