﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PetService_Project.Models;

public partial class TNewsFile
{
    public int FId { get; set; }

    public int? FNewsId { get; set; }

    public string FFileName { get; set; }

    public string FFilePath { get; set; }

    public DateTime? FUploadDate { get; set; }

    public virtual TNews FNews { get; set; }
}