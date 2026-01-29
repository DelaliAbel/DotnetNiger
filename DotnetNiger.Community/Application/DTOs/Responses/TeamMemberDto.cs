using System;
using System.Collections.Generic;

namespace DotnetNiger.Community.Application.DTOs.Responses;

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string BioOverride { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public List<SocialLinkDto> SocialLinks { get; set; } = new();
    public List<string> Skills { get; set; } = new();
}
