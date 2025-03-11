using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Northwind.EntityModels;

[Keyless]
public partial class EmployeeTerritory
{
    public int EmployeeId { get; set; }

    [Column("TerritoryID")]
    [StringLength(20)]
    public string TerritoryId { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey("TerritoryId")]
    public virtual Territory Territory { get; set; } = null!;
}
