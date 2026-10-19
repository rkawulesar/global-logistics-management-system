using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS_Web.Models;
//Code Attribution for data validation logic
//Link: https://www.c-sharpcorner.com/article/data-annotations-in-net/
public partial class ServiceRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a contract.")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Cost is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
    public decimal Cost { get; set; }

    [NotMapped]
    [Range(0.01, double.MaxValue, ErrorMessage = "USD amount must be greater than 0.")]
    public decimal? UsdAmount { get; set; }

    [Required(ErrorMessage = "Please select a status.")]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    public virtual Contract? Contract { get; set; } = null!;
}