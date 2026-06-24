CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [PostCount] int NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ContactMessages] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Subject] nvarchar(200) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Events] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Location] nvarchar(200) NOT NULL,
    [EventType] nvarchar(50) NOT NULL,
    [Category] nvarchar(100) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [CoverImageUrl] nvarchar(max) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [OrganizerName] nvarchar(max) NOT NULL,
    [Capacity] int NOT NULL,
    [RegisteredCount] int NOT NULL,
    [IsPublished] bit NOT NULL,
    [IsArchived] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [MeetupLink] nvarchar(max) NOT NULL,
    [RejectionReason] nvarchar(max) NULL,
    [SubmittedAt] datetime2 NULL,
    [PublishedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Members] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Bio] nvarchar(max) NOT NULL,
    [AvatarUrl] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Country] nvarchar(100) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [IsTeamMember] bit NOT NULL,
    [Position] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [NewsletterSubscriptions] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [UnsubscribeToken] nvarchar(200) NULL,
    [IsActive] bit NOT NULL,
    [SubscribedAt] datetime2 NOT NULL,
    [UnsubscribedAt] datetime2 NULL,
    CONSTRAINT [PK_NewsletterSubscriptions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Partners] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [LogoUrl] nvarchar(max) NOT NULL,
    [WebsiteUrl] nvarchar(max) NOT NULL,
    [PartnerType] nvarchar(50) NOT NULL,
    [SortOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Partners] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Posts] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Excerpt] nvarchar(max) NOT NULL,
    [CoverImageUrl] nvarchar(max) NOT NULL,
    [AuthorId] uniqueidentifier NOT NULL,
    [AuthorName] nvarchar(max) NOT NULL,
    [AuthorAvatar] nvarchar(max) NOT NULL,
    [PostType] nvarchar(50) NOT NULL,
    [IsPublished] bit NOT NULL,
    [ViewCount] int NOT NULL,
    [PublishedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Projects] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [GithubUrl] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [Technologies] nvarchar(500) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [AuthorName] nvarchar(max) NOT NULL,
    [IsFeatured] bit NOT NULL,
    [IsPublished] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Resources] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [ResourceType] nvarchar(50) NOT NULL,
    [Level] nvarchar(50) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [ViewCount] int NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Resources] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Tags] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Slug] nvarchar(100) NOT NULL,
    [UsageCount] int NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [EventMedias] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_EventMedias] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventMedias_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id])
);
GO

CREATE TABLE [EventRegistrations] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [UserName] nvarchar(max) NOT NULL,
    [AvatarUrl] nvarchar(500) NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    [IsAttended] bit NOT NULL,
    [RegistrationStatus] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_EventRegistrations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventRegistrations_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id])
);
GO

CREATE TABLE [Speakers] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Role] nvarchar(100) NOT NULL,
    [AvatarUrl] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_Speakers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Speakers_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Certificates] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CertificateUrl] nvarchar(500) NOT NULL,
    [CertificateType] nvarchar(100) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [SubmissionDate] datetime2 NOT NULL,
    [ReviewedNotes] nvarchar(max) NULL,
    [ReviewedAt] datetime2 NULL,
    CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Certificates_Members_UserId] FOREIGN KEY ([UserId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MemberSkills] (
    [Id] uniqueidentifier NOT NULL,
    [MemberId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_MemberSkills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MemberSkills_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SocialLinks] (
    [Id] uniqueidentifier NOT NULL,
    [MemberId] uniqueidentifier NOT NULL,
    [Platform] nvarchar(50) NOT NULL,
    [Url] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SocialLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SocialLinks_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Comments] (
    [Id] uniqueidentifier NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [AuthorName] nvarchar(max) NOT NULL,
    [AuthorAvatar] nvarchar(max) NOT NULL,
    [PostId] uniqueidentifier NULL,
    [EventId] uniqueidentifier NULL,
    [ParentCommentId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [Comments] ([Id]),
    CONSTRAINT [FK_Comments_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]),
    CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id])
);
GO

CREATE TABLE [PostCategories] (
    [PostId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_PostCategories] PRIMARY KEY ([PostId], [CategoryId]),
    CONSTRAINT [FK_PostCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PostCategories_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ResourceCategories] (
    [ResourceId] uniqueidentifier NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ResourceCategories] PRIMARY KEY ([ResourceId], [CategoryId]),
    CONSTRAINT [FK_ResourceCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ResourceCategories_Resources_ResourceId] FOREIGN KEY ([ResourceId]) REFERENCES [Resources] ([Id])
);
GO

CREATE TABLE [EventTags] (
    [EventId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_EventTags] PRIMARY KEY ([EventId], [TagId]),
    CONSTRAINT [FK_EventTags_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]),
    CONSTRAINT [FK_EventTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PostTags] (
    [PostId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_PostTags] PRIMARY KEY ([PostId], [TagId]),
    CONSTRAINT [FK_PostTags_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PostTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ResourceTags] (
    [ResourceId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ResourceTags] PRIMARY KEY ([ResourceId], [TagId]),
    CONSTRAINT [FK_ResourceTags_Resources_ResourceId] FOREIGN KEY ([ResourceId]) REFERENCES [Resources] ([Id]),
    CONSTRAINT [FK_ResourceTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
GO

CREATE INDEX [IX_Certificates_UserId_Status] ON [Certificates] ([UserId], [Status]);
GO

CREATE INDEX [IX_Comments_EventId] ON [Comments] ([EventId]);
GO

CREATE INDEX [IX_Comments_ParentCommentId] ON [Comments] ([ParentCommentId]);
GO

CREATE INDEX [IX_Comments_PostId] ON [Comments] ([PostId]);
GO

CREATE INDEX [IX_EventMedias_EventId] ON [EventMedias] ([EventId]);
GO

CREATE UNIQUE INDEX [IX_EventRegistrations_EventId_UserId] ON [EventRegistrations] ([EventId], [UserId]);
GO

CREATE INDEX [IX_Events_IsPublished_EndDate] ON [Events] ([IsPublished], [EndDate]);
GO

CREATE UNIQUE INDEX [IX_Events_Slug] ON [Events] ([Slug]);
GO

CREATE INDEX [IX_EventTags_TagId] ON [EventTags] ([TagId]);
GO

CREATE INDEX [IX_MemberSkills_MemberId] ON [MemberSkills] ([MemberId]);
GO

CREATE UNIQUE INDEX [IX_NewsletterSubscriptions_Email] ON [NewsletterSubscriptions] ([Email]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Partners_Slug] ON [Partners] ([Slug]);
GO

CREATE INDEX [IX_PostCategories_CategoryId] ON [PostCategories] ([CategoryId]);
GO

CREATE UNIQUE INDEX [IX_Posts_Slug] ON [Posts] ([Slug]);
GO

CREATE INDEX [IX_PostTags_TagId] ON [PostTags] ([TagId]);
GO

CREATE UNIQUE INDEX [IX_Projects_Slug] ON [Projects] ([Slug]);
GO

CREATE INDEX [IX_ResourceCategories_CategoryId] ON [ResourceCategories] ([CategoryId]);
GO

CREATE UNIQUE INDEX [IX_Resources_Slug] ON [Resources] ([Slug]);
GO

CREATE INDEX [IX_ResourceTags_TagId] ON [ResourceTags] ([TagId]);
GO

CREATE INDEX [IX_SocialLinks_MemberId] ON [SocialLinks] ([MemberId]);
GO

CREATE INDEX [IX_Speakers_EventId] ON [Speakers] ([EventId]);
GO

CREATE UNIQUE INDEX [IX_Tags_Slug] ON [Tags] ([Slug]);
GO
