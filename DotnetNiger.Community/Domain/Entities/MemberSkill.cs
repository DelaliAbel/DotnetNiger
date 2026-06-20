namespace DotnetNiger.Community.Domain.Entities;

public class MemberSkill
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Member Member { get; set; } = null!;
}
