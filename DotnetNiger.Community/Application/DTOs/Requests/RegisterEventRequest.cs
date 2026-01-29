using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class RegisterEventRequest
{
    [Required]
    public Guid EventId { get; set; }
}
