namespace DotnetNiger.Community.Domain.Entities;

public class TeamMemberSkill
{
	public Guid Id { get; set; }
	public Guid TeamMemberId { get; set; }
	public string SkillName { get; set; } = string.Empty;
	public int Level { get; set; } = 0;

	// FK
	public TeamMember TeamMember { get; set; } = null!;
}
