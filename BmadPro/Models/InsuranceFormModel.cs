using System.ComponentModel.DataAnnotations;

namespace BmadPro.Models;

public class InsuranceFormModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Policy type is required")]
    [Display(Name = "Policy Type")]
    public string PolicyType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Coverage amount is required")]
    [Display(Name = "Coverage Amount")]
    public string CoverageAmount { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nominee name is required")]
    [Display(Name = "Nominee Name")]
    public string NomineeName { get; set; } = string.Empty;
}
