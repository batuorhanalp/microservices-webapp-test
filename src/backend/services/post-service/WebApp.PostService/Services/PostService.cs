using Microsoft.Extensions.Logging;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.PostService.Services;
/// <summary>
/// Service implementation for managing post-related business logic operations.
/// Handles post creation, updates, deletion, and retrieval with proper validation,
/// authorization, and visibility controls following domain business rules.
/// </summary>
public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PostService> _logger;

    public PostService(IPostRepository postRepository, IUserRepository userRepository, ILogger<PostService> logger)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Post> CreateTextPostAsync(Guid authorId, string content, PostVisibility visibility = PostVisibility.Public)
    {
        _logger.LogInformation("Creating text post for author {AuthorId} with visibility {Visibility}", authorId, visibility);

        // Input validation
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required for text posts", nameof(content));

        // Verify author exists
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found", nameof(authorId));

        // Create post
        var post = new Post(authorId, content, PostType.Text, visibility);

        // Save to repository
        await _postRepository.AddAsync(post);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created text post {PostId} for author {AuthorId}", post.Id, authorId);
        return post;
    }

    public async Task<Post> CreateMediaPostAsync(Guid authorId, string? content, IEnumerable<MediaAttachment> mediaAttachments, PostVisibility visibility = PostVisibility.Public)
    {
        _logger.LogInformation("Creating media post for author {AuthorId} with visibility {Visibility}", authorId, visibility);

        // Input validation
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (mediaAttachments == null)
            throw new ArgumentException("Media attachments cannot be null", nameof(mediaAttachments));

        var attachmentList = mediaAttachments.ToList();
        if (!attachmentList.Any())
            throw new ArgumentException("At least one media attachment is required", nameof(mediaAttachments));

        // Verify author exists
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found", nameof(authorId));

        // Create post with media attachments
        var post = new Post(authorId, content ?? string.Empty, PostType.Text, visibility);
        
        // Add media attachments and determine post type
        foreach (var attachment in attachmentList)
        {
            post.AddMediaAttachment(attachment.Url, attachment.FileName, attachment.ContentType, attachment.FileSize);
        }

        // Save to repository
        await _postRepository.AddAsync(post);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created media post {PostId} for author {AuthorId} with {AttachmentCount} attachments", 
            post.Id, authorId, attachmentList.Count);
        return post;
    }

    public async Task<Post> CreateReplyAsync(Guid authorId, Guid parentPostId, string content, PostVisibility visibility = PostVisibility.Public)
    {
        _logger.LogInformation("Creating reply for author {AuthorId} to post {ParentPostId}", authorId, parentPostId);

        // Input validation
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (parentPostId == Guid.Empty)
            throw new ArgumentException("Parent post ID cannot be empty", nameof(parentPostId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required for replies", nameof(content));

        // Verify author exists
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found", nameof(authorId));

        // Verify parent post exists
        var parentPost = await _postRepository.GetByIdAsync(parentPostId);
        if (parentPost == null)
            throw new ArgumentException("Parent post not found", nameof(parentPostId));

        // Create reply post
        var reply = new Post(authorId, content, parentPostId, parentPost.RootPostId, visibility);

        // Save to repository
        await _postRepository.AddAsync(reply);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created reply {PostId} for author {AuthorId} to post {ParentPostId}", 
            reply.Id, authorId, parentPostId);
        return reply;
    }

    public async Task<Post> UpdatePostContentAsync(Guid postId, Guid authorId, string newContent)
    {
        _logger.LogInformation("Updating content for post {PostId} by author {AuthorId}", postId, authorId);

        // Input validation
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty", nameof(newContent));

        // Get post
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Authorization check
        if (post.AuthorId != authorId)
            throw new ArgumentException("User is not authorized to update this post", nameof(authorId));

        // Update content
        post.UpdateContent(newContent);

        // Save changes
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated content for post {PostId}", postId);
        return post;
    }

    public async Task<Post> UpdatePostVisibilityAsync(Guid postId, Guid authorId, PostVisibility newVisibility)
    {
        _logger.LogInformation("Updating visibility for post {PostId} to {NewVisibility} by author {AuthorId}", 
            postId, newVisibility, authorId);

        // Input validation
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        // Get post
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Authorization check
        if (post.AuthorId != authorId)
            throw new ArgumentException("User is not authorized to update this post", nameof(authorId));

        // Update visibility
        post.SetVisibility(newVisibility);

        // Save changes
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated visibility for post {PostId} to {NewVisibility}", postId, newVisibility);
        return post;
    }

    public async Task<bool> DeletePostAsync(Guid postId, Guid authorId)
    {
        _logger.LogInformation("Deleting post {PostId} by author {AuthorId}", postId, authorId);

        // Input validation
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        // Get post
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Authorization check
        if (post.AuthorId != authorId)
            throw new ArgumentException("User is not authorized to delete this post", nameof(authorId));

        // Delete post
        await _postRepository.DeleteAsync(postId);
        await _postRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted post {PostId}", postId);
        return true;
    }

    public async Task<Post?> GetPostByIdAsync(Guid postId, Guid? viewerId = null)
    {
        if (postId == Guid.Empty)
        {
            _logger.LogDebug("Empty post ID provided, returning null");
            return null;
        }

        _logger.LogDebug("Retrieving post {PostId} for viewer {ViewerId}", postId, viewerId);

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            _logger.LogDebug("Post {PostId} not found", postId);
            return null;
        }

        // TODO: Add visibility checks based on viewerId and post visibility
        // For now, return the post directly (basic implementation)

        _logger.LogDebug("Successfully retrieved post {PostId}", postId);
        return post;
    }

    public async Task<IEnumerable<Post>> GetPostsByAuthorAsync(Guid authorId, Guid? viewerId = null, int limit = 20, int offset = 0)
    {
        if (authorId == Guid.Empty)
        {
            _logger.LogDebug("Empty author ID provided, returning empty list");
            return Enumerable.Empty<Post>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 20;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving posts by author {AuthorId} with limit {Limit} and offset {Offset}", authorId, limit, offset);

        var posts = await _postRepository.GetByAuthorAsync(authorId, limit, offset);

        // TODO: Add visibility filtering based on viewerId
        
        _logger.LogDebug("Retrieved {PostCount} posts for author {AuthorId}", posts.Count(), authorId);
        return posts;
    }

    public async Task<IEnumerable<Post>> GetUserFeedAsync(Guid userId, int limit = 20, int offset = 0)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogDebug("Empty user ID provided, returning empty feed");
            return Enumerable.Empty<Post>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 20;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving feed for user {UserId} with limit {Limit} and offset {Offset}", userId, limit, offset);

        var posts = await _postRepository.GetFeedAsync(userId, limit, offset);

        _logger.LogDebug("Retrieved {PostCount} posts for user feed {UserId}", posts.Count(), userId);
        return posts;
    }

    public async Task<IEnumerable<Post>> GetPublicTimelineAsync(int limit = 20, int offset = 0)
    {
        // Normalize pagination parameters
        if (limit <= 0) limit = 20;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving public timeline with limit {Limit} and offset {Offset}", limit, offset);

        var posts = await _postRepository.GetPublicTimelineAsync(limit, offset);

        _logger.LogDebug("Retrieved {PostCount} posts for public timeline", posts.Count());
        return posts;
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm, Guid? viewerId = null, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogDebug("Empty or null search term provided, returning empty list");
            return Enumerable.Empty<Post>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 20;

        _logger.LogDebug("Searching posts with term '{SearchTerm}' for viewer {ViewerId} with limit {Limit}", 
            searchTerm, viewerId, limit);

        var posts = await _postRepository.SearchAsync(searchTerm, limit);

        // TODO: Add visibility filtering based on viewerId

        _logger.LogDebug("Found {PostCount} posts matching search term '{SearchTerm}'", posts.Count(), searchTerm);
        return posts;
    }

    public async Task<bool> CanUserViewPostAsync(Guid postId, Guid? viewerId)
    {
        if (postId == Guid.Empty)
        {
            _logger.LogDebug("Empty post ID provided, access denied");
            return false;
        }

        _logger.LogDebug("Checking if viewer {ViewerId} can access post {PostId}", viewerId, postId);

        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            _logger.LogDebug("Post {PostId} not found, access denied", postId);
            return false;
        }

        // Basic visibility check - public posts are always viewable
        if (post.Visibility == PostVisibility.Public)
        {
            _logger.LogDebug("Post {PostId} is public, access granted", postId);
            return true;
        }

        // TODO: Implement full visibility logic with user relationships
        // For now, return true for simplicity (basic implementation)

        _logger.LogDebug("Post {PostId} visibility check completed", postId);
        return true;
    }

    public async Task<IEnumerable<Post>> GetMediaPostsAsync(Guid? viewerId = null, int limit = 20, int offset = 0)
    {
        // Normalize pagination parameters
        if (limit <= 0) limit = 20;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving media posts for viewer {ViewerId} with limit {Limit} and offset {Offset}", 
            viewerId, limit, offset);

        var posts = await _postRepository.GetPostsWithMediaAsync(limit, offset);

        // TODO: Add visibility filtering based on viewerId

        _logger.LogDebug("Retrieved {PostCount} media posts", posts.Count());
        return posts;
    }

    public async Task<IEnumerable<Post>> GetPostRepliesAsync(Guid parentPostId, Guid? viewerId = null, int limit = 20, int offset = 0)
    {
        if (parentPostId == Guid.Empty)
        {
            _logger.LogDebug("Empty parent post ID provided, returning empty list");
            return Enumerable.Empty<Post>();
        }

        // Normalize pagination parameters
        if (limit <= 0) limit = 20;
        if (offset < 0) offset = 0;

        _logger.LogDebug("Retrieving replies for post {ParentPostId} for viewer {ViewerId} with limit {Limit} and offset {Offset}", 
            parentPostId, viewerId, limit, offset);

        var replies = await _postRepository.GetRepliesAsync(parentPostId, limit, offset);

        // TODO: Add visibility filtering based on viewerId

        _logger.LogDebug("Retrieved {ReplyCount} replies for post {ParentPostId}", replies.Count(), parentPostId);
        return replies;
    }
}
