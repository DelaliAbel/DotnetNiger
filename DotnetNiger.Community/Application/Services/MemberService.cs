using DotnetNiger.Community.Application.Exceptions;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Domain.Enums;

namespace DotnetNiger.Community.Application.Services;

/// <summary>
/// Service de gestion des membres de l'équipe/communauté
/// </summary>
public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamMemberPersistence _memberRepository;
    private readonly ILogger<TeamMemberService> _logger;

    public TeamMemberService(ITeamMemberPersistence memberRepository, ILogger<TeamMemberService> logger)
    {
        _memberRepository = memberRepository;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les membres avec pagination
    /// </summary>
    public async Task<IEnumerable<TeamMember>> GetAllMembersAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            page = Math.Max(1, page);
            pageSize = Math.Min(pageSize, 100); // Cap at 100 for safety

            return await _memberRepository.GetPagedAsync(page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des membres");
            throw;
        }
    }

    /// <summary>
    /// Récupère les membres actifs
    /// </summary>
    public async Task<IEnumerable<TeamMember>> GetActiveMembersAsync()
    {
        try
        {
            return await _memberRepository.GetActiveMembersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des membres actifs");
            throw;
        }
    }

    /// <summary>
    /// Récupère un membre par son ID
    /// </summary>
    public async Task<TeamMember?> GetMemberByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("L'ID du membre est requis", nameof(id));

        try
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null)
                _logger.LogWarning("Membre non trouvé avec l'ID {MemberId}", id);
            return member;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du membre {MemberId}", id);
            throw;
        }
    }

    /// <summary>
    /// Crée un nouveau membre
    /// </summary>
    public async Task<TeamMember> CreateMemberAsync(TeamMember member)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        ValidateMember(member);

        try
        {
            member.Id = Guid.NewGuid();
            member.JoinedAt = DateTime.UtcNow;
            member.IsActive = false;
            member.MembershipStatus = ApprovalStatus.Pending;
            member.ReviewedAt = null;
            member.ReviewedByUserId = null;

            var createdMember = await _memberRepository.AddAsync(member);
            _logger.LogInformation("Membre créé avec succès: {MemberId}", createdMember.Id);
            return createdMember;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du membre");
            throw;
        }
    }

    /// <summary>
    /// Met à jour un membre existant
    /// </summary>
    public async Task<TeamMember> UpdateMemberAsync(Guid id, TeamMember member)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("L'ID du membre est requis", nameof(id));

        if (member == null)
            throw new ArgumentNullException(nameof(member));

        ValidateMember(member);

        try
        {
            var existingMember = await _memberRepository.GetByIdAsync(id);
            if (existingMember == null)
                throw new NotFoundException($"Membre avec l'ID {id} non trouve");

            // Mise à jour des propriétés
            existingMember.Name = member.Name;
            existingMember.Position = member.Position;
            existingMember.Order = member.Order;
            existingMember.BioOverride = member.BioOverride;
            existingMember.IsPublic = member.IsPublic;
            existingMember.IsActive = member.IsActive;
            existingMember.RoleDescription = member.RoleDescription;
            existingMember.UserId = member.UserId;

            var updatedMember = await _memberRepository.UpdateAsync(existingMember);
            _logger.LogInformation("Membre mis à jour avec succès: {MemberId}", id);
            return updatedMember;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du membre {MemberId}", id);
            throw;
        }
    }

    /// <summary>
    /// Supprime un membre
    /// </summary>
    public async Task<bool> DeleteMemberAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("L'ID du membre est requis", nameof(id));

        try
        {
            var exists = await _memberRepository.GetByIdAsync(id);
            if (exists == null)
                throw new NotFoundException($"Membre avec l'ID {id} non trouve");

            var deleted = await _memberRepository.DeleteAsync(id);
            if (deleted)
                _logger.LogInformation("Membre supprimé avec succès: {MemberId}", id);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du membre {MemberId}", id);
            throw;
        }
    }

    public async Task<TeamMember> ApproveMemberAsync(Guid id, Guid reviewerUserId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("L'ID du membre est requis", nameof(id));

        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("L'ID du validateur est requis", nameof(reviewerUserId));

        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null)
            throw new NotFoundException($"Membre avec l'ID {id} non trouve");

        member.MembershipStatus = ApprovalStatus.Approved;
        member.IsActive = true;
        member.ReviewedAt = DateTime.UtcNow;
        member.ReviewedByUserId = reviewerUserId;

        return await _memberRepository.UpdateAsync(member);
    }

    public async Task<TeamMember> RejectMemberAsync(Guid id, Guid reviewerUserId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("L'ID du membre est requis", nameof(id));

        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("L'ID du validateur est requis", nameof(reviewerUserId));

        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null)
            throw new NotFoundException($"Membre avec l'ID {id} non trouve");

        member.MembershipStatus = ApprovalStatus.Rejected;
        member.IsActive = false;
        member.IsPublic = false;
        member.ReviewedAt = DateTime.UtcNow;
        member.ReviewedByUserId = reviewerUserId;

        return await _memberRepository.UpdateAsync(member);
    }

    /// <summary>
    /// Valide un membre
    /// </summary>
    private void ValidateMember(TeamMember member)
    {
        if (string.IsNullOrWhiteSpace(member.Name))
            throw new ArgumentException("Le nom du membre est requis");

        if (member.UserId == Guid.Empty)
            throw new ArgumentException("L'ID utilisateur du membre est requis");

        if (member.Name.Length > 255)
            throw new ArgumentException("Le nom du membre ne doit pas dépasser 255 caractères");

        if (member.Order < 0)
            throw new ArgumentException("L'ordre doit être un nombre positif");
    }
}
