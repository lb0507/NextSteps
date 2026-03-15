using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataLibrary.Models
{
    public class User : IValidatableObject

    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Please input a name.")]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Please input an email address.")]
        [EmailAddress(ErrorMessage = "Please input a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please input a password.")]
        public string Password { get; set; } = "";

        [NotMapped]
        [Required(ErrorMessage = "Please verify the password.")]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword && ConfirmPassword != "placeholder")
            {
                yield return new ValidationResult(
                    "Passwords do not match.",
                    new[] { nameof(ConfirmPassword) }
                );
            }
        }


    }
}
