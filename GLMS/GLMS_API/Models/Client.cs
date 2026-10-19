using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GLMS_API.Models;

public partial class Client
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Surname is required.")]
    public string Surname { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Region is required.")]
    public string Region { get; set; } = null!;
    public string FullName => FirstName + " " + Surname;
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
