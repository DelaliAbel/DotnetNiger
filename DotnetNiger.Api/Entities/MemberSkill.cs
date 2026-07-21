namespace DotnetNiger.Api.Entities;

public class MemberSkill
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Member Member { get; set; } = null!;
}
