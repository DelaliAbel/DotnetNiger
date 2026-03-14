using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Mappers;

public class CommunityRequestMapper : ICommunityRequestMapper
{
	public Post MapToPost(CreatePostRequest request, Guid authorId)
	{
		return new Post
		{
			Title = request.Title,
			Content = request.Content,
			Excerpt = request.Excerpt,
			CoverImageUrl = request.CoverImageUrl,
			AuthorId = authorId,
			PostType = string.IsNullOrWhiteSpace(request.PostType) ? "Blog" : request.PostType,
			IsPublished = request.IsPublished
		};
	}

	public void ApplyPostUpdates(Post post, UpdatePostRequest request)
	{
		post.Title = request.Title;
		post.Content = request.Content;
		post.Excerpt = request.Excerpt;
		post.CoverImageUrl = request.CoverImageUrl;
		post.PostType = request.PostType;
		post.SeoDescription = request.SeoDescription;
		post.IsPublished = request.IsPublished;
	}

	public Project MapToProject(CreateProjectRequest request, Guid ownerId)
	{
		return new Project
		{
			Name = request.Name,
			Description = request.Description,
			GitHubUrl = request.GitHubUrl,
			OwnerId = ownerId,
			IsFeatured = false,
			Stars = 0,
			ContributorsCount = 0,
			Language = string.IsNullOrWhiteSpace(request.Language) ? "C#" : request.Language,
			License = string.IsNullOrWhiteSpace(request.License) ? "MIT" : request.License
		};
	}

	public void ApplyProjectUpdates(Project project, UpdateProjectRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Name))
			project.Name = request.Name;
		if (!string.IsNullOrWhiteSpace(request.Description))
			project.Description = request.Description;
		if (!string.IsNullOrWhiteSpace(request.GitHubUrl))
			project.GitHubUrl = request.GitHubUrl;
		if (!string.IsNullOrWhiteSpace(request.Language))
			project.Language = request.Language;
		if (!string.IsNullOrWhiteSpace(request.License))
			project.License = request.License;
		if (request.IsFeatured.HasValue)
			project.IsFeatured = request.IsFeatured.Value;
	}

	public Resource MapToResource(CreateResourceRequest request, Guid createdBy)
	{
		return new Resource
		{
			Title = request.Title,
			Description = request.Description,
			Url = request.Url,
			ResourceType = request.ResourceType,
			Level = request.Level,
			CreatedBy = createdBy,
			IsApproved = false,
			ViewCount = 0
		};
	}

	public void ApplyResourceUpdates(Resource resource, UpdateResourceRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Title))
			resource.Title = request.Title;
		if (!string.IsNullOrWhiteSpace(request.Description))
			resource.Description = request.Description;
		if (!string.IsNullOrWhiteSpace(request.Url))
			resource.Url = request.Url;
		if (!string.IsNullOrWhiteSpace(request.ResourceType))
			resource.ResourceType = request.ResourceType;
		if (!string.IsNullOrWhiteSpace(request.Level))
			resource.Level = request.Level;
		if (request.IsApproved.HasValue)
			resource.IsApproved = request.IsApproved.Value;
	}

	public Event MapToEvent(CreateEventRequest request, Guid createdBy)
	{
		return new Event
		{
			Title = request.Title,
			Description = request.Description,
			Location = request.Location,
			EventType = string.IsNullOrWhiteSpace(request.EventType) ? "Physical" : request.EventType,
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			CoverImageUrl = request.CoverImageUrl,
			CreatedBy = createdBy,
			Capacity = request.Capacity <= 0 ? 500 : request.Capacity,
			MeetupLink = request.MeetupLink,
			IsPublished = request.IsPublished
		};
	}

	public void ApplyEventUpdates(Event @event, UpdateEventRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Title))
			@event.Title = request.Title;
		if (!string.IsNullOrWhiteSpace(request.Description))
			@event.Description = request.Description;
		if (!string.IsNullOrWhiteSpace(request.Location))
			@event.Location = request.Location;
		if (!string.IsNullOrWhiteSpace(request.EventType))
			@event.EventType = request.EventType;
		if (request.StartDate.HasValue)
			@event.StartDate = request.StartDate.Value;
		if (request.EndDate.HasValue)
			@event.EndDate = request.EndDate.Value;
		if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
			@event.CoverImageUrl = request.CoverImageUrl;
		if (request.Capacity.HasValue)
			@event.Capacity = request.Capacity.Value;
		if (!string.IsNullOrWhiteSpace(request.MeetupLink))
			@event.MeetupLink = request.MeetupLink;
		if (request.IsPublished.HasValue)
			@event.IsPublished = request.IsPublished.Value;
	}

	public Category MapToCategory(CreateCategoryRequest request)
	{
		return new Category
		{
			Name = request.Name,
			Description = request.Description ?? string.Empty
		};
	}

	public void ApplyCategoryUpdates(Category category, UpdateCategoryRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Name))
			category.Name = request.Name;
		if (request.Description is not null)
			category.Description = request.Description;
	}

	public Partner MapToPartner(CreatePartnerRequest request)
	{
		return new Partner
		{
			Name = request.Name,
			LogoUrl = request.LogoUrl ?? string.Empty,
			Website = request.Website ?? string.Empty,
			Description = request.Description ?? string.Empty,
			PartnerType = request.PartnerType ?? "Silver",
			Level = request.Level ?? "Gold",
			DisplayOrder = 0
		};
	}

	public void ApplyPartnerUpdates(Partner partner, UpdatePartnerRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Name))
			partner.Name = request.Name;
		if (!string.IsNullOrWhiteSpace(request.Description))
			partner.Description = request.Description;
		if (!string.IsNullOrWhiteSpace(request.Website))
			partner.Website = request.Website;
	}

	public Tag MapToTag(CreateTagRequest request)
	{
		return new Tag
		{
			Name = request.Name,
			PostCount = 0
		};
	}

	public Comment MapToComment(CreateCommentRequest request, Guid postId, Guid userId)
	{
		return new Comment
		{
			PostId = postId,
			UserId = userId,
			Content = request.Content,
			IsApproved = false
		};
	}

	public void ApplyCommentUpdates(Comment comment, UpdateCommentRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Content))
			comment.Content = request.Content;
	}
}
