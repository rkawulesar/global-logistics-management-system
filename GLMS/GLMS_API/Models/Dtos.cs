using System.ComponentModel.DataAnnotations;

namespace GLMS_API.Models;

public class UpdateContractStatusDto
{
    [Required] public string Status { get; set; } = null!;
}

public class CreateServiceRequestDto
{
    [Required] public int ContractId { get; set; }
    [Required][StringLength(255)] public string Description { get; set; } = null!;
    [Required][Range(0.01, double.MaxValue)] public decimal Cost { get; set; }
    [Required] public string Status { get; set; } = null!;
}

public class LoginDto
{
    [Required] public string Username { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}

public class ContractDropdownItem
{
    public int Id { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}