﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PetService_Project.Models;

public partial class TQtyStatus
{
    public int FId { get; set; }

    public DateTime? FDate { get; set; }

    public int? FHotelId { get; set; }

    public int? FSmallDogRoom { get; set; }

    public int? FMiddleDogRoom { get; set; }

    public int? FBigDogRoom { get; set; }

    public int? FCatRoom { get; set; }

    public virtual THotel FHotel { get; set; }
}