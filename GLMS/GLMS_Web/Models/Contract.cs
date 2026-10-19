using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GLMS_Web.Models;

public partial class Contract
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a client.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Please select a status.")]
    public string Status { get; set; } = null!;

    [Required(ErrorMessage = "Please select a service level.")]
    public string ServiceLevel { get; set; } = null!;

    public string? FilePath { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}