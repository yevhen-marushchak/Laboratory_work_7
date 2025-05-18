using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.WebAPI.Models
{
    public class ReservationDto : IValidatableObject
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public bool IsEventPackage { get; set; }
        public string? AdditionalDetails { get; set; }

        [Required]
        public List<SelectedRoomDto> SelectedRooms { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsEventPackage && string.IsNullOrWhiteSpace(AdditionalDetails))
            {
                yield return new ValidationResult("Вкажіть додаткові відомості для заходу під ключ.",
                    new[] { nameof(AdditionalDetails) });
            }
        }
    }

    public class SelectedRoomDto
    {
        public int RoomId { get; set; }
        public int ActivityId { get; set; }
    }
}