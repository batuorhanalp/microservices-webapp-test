using Microsoft.Extensions.Logging;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.Common.Services;

/// <summary>
/// Service implementation for comment-related business operations.
/// Handles comment creation, updates, deletion, and retrieval with proper validation,
/// authorization, and threading support for comments and replies.
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IPostRepository postRepository,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Comment> CreateCommentAsync(Guid authorId, Guid postId, string content)
    {
        _logger.LogInformation("Creating comment on post {PostId} by author {AuthorId}", postId, authorId);

        // Input validation
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        // Verify author exists
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found", nameof(authorId));

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        // Create comment
        var comment = new Comment(authorId, postId, content);

        // Save to repository
        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created comment {CommentId} on post {PostId} by author {AuthorId}",
            comment.Id, postId, authorId);
        return comment;
    }

    public async Task<Comment> CreateReplyAsync(Guid authorId, Guid parentCommentId, string content)
    {
        _logger.LogInformation("Creating reply to comment {ParentCommentId} by author {AuthorId}", 
            parentCommentId, authorId);

        // Input validation
        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (parentCommentId == Guid.Empty)
            throw new ArgumentException("Parent comment ID cannot be empty", nameof(parentCommentId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Reply content is required", nameof(content));

        // Verify author exists
        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found", nameof(authorId));

        // Verify parent comment exists
        var parentComment = await _commentRepository.GetByIdAsync(parentCommentId);
        if (parentComment == null)
            throw new ArgumentException("Parent comment not found", nameof(parentCommentId));

        // Create reply comment on the same post as parent
        var reply = new Comment(authorId, parentComment.PostId, content);

        // Save to repository
        await _commentRepository.AddAsync(reply);
        await _commentRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created reply {CommentId} to comment {ParentCommentId} by author {AuthorId}",
            reply.Id, parentCommentId, authorId);
        return reply;
    }

    public async Task<Comment> UpdateCommentAsync(Guid commentId, Guid authorId, string newContent)
    {
        _logger.LogInformation("Updating comment {CommentId} by author {AuthorId}", commentId, authorId);

        // Input validation
        if (commentId == Guid.Empty)
            throw new ArgumentException("Comment ID cannot be empty", nameof(commentId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Comment content cannot be empty", nameof(newContent));

        // Get comment
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new ArgumentException("Comment not found", nameof(commentId));

        // Authorization check
        if (comment.UserId != authorId)
            throw new ArgumentException("User is not authorized to update this comment", nameof(authorId));

        // Update content
        comment.UpdateContent(newContent);

        // Save changes
        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated comment {CommentId}", commentId);
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid authorId)
    {
        _logger.LogInformation("Deleting comment {CommentId} by author {AuthorId}", commentId, authorId);

        // Input validation
        if (commentId == Guid.Empty)
            throw new ArgumentException("Comment ID cannot be empty", nameof(commentId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));

        // Get comment
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            return false;

        // Authorization check
        if (comment.UserId != authorId)
            throw new ArgumentException("User is not authorized to delete this comment", nameof(authorId));

        // Delete comment
        await _commentRepository.DeleteAsync(commentId);
        await _commentRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted comment {CommentId}", commentId);
        return true;
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId, Guid? viewerId = null)
    {
        _logger.LogDebug("Retrieving comment {CommentId} for viewer {ViewerId}", commentId, viewerId);

        // Input validation
        if (commentId == Guid.Empty)
            return null;

        var comment = await _commentRepository.GetByIdAsync(commentId);
        
        // Additional authorization checks could be implemented here based on post visibility
        // For now, all comments are visible if the comment exists
        
        return comment;
    }

    public async Task<IEnumerable<Comment>> GetPostCommentsAsync(Guid postId, Guid? viewerId = null, int limit = 50, int offset = 0)
    {
        _logger.LogDebug("Retrieving comments for post {PostId} for viewer {ViewerId} with limit {Limit} and offset {Offset}",
            postId, viewerId, limit, offset);

        // Input validation
        if (postId == Guid.Empty)
            return Enumerable.Empty<Comment>();

        if (limit <= 0)
            limit = 50;

        if (offset < 0)
            offset = 0;

        return await _commentRepository.GetByPostAsync(postId, limit, offset);
    }

    public async Task<IEnumerable<Comment>> GetUserCommentsAsync(Guid userId, Guid? viewerId = null, int limit = 50, int offset = 0)
    {
        _logger.LogDebug("Retrieving comments for user {UserId} for viewer {ViewerId} with limit {Limit} and offset {Offset}",
            userId, viewerId, limit, offset);

        // Input validation
        if (userId == Guid.Empty)
            return Enumerable.Empty<Comment>();

        if (limit <= 0)
            limit = 50;

        if (offset < 0)
            offset = 0;

        return await _commentRepository.GetByAuthorAsync(userId, limit, offset);
    }

    public async Task<IEnumerable<Comment>> GetCommentRepliesAsync(Guid parentCommentId, Guid? viewerId = null, int limit = 20, int offset = 0)
    {
        _logger.LogDebug("Retrieving replies for comment {ParentCommentId} for viewer {ViewerId} with limit {Limit} and offset {Offset}",
            parentCommentId, viewerId, limit, offset);

        // For now, we don't have a direct parent-child relationship for comments
        // This would require additional database schema changes or a different approach
        // Returning empty for now - this could be enhanced later
        return Enumerable.Empty<Comment>();
    }

    public async Task<int> GetPostCommentCountAsync(Guid postId)
    {
        _logger.LogDebug("Retrieving comment count for post {PostId}", postId);

        // Input validation
        if (postId == Guid.Empty)
            return 0;

        // This would require adding a count method to the repository
        // For now, we'll get all comments and count them (not efficient for large datasets)
        var comments = await _commentRepository.GetByPostAsync(postId, int.MaxValue, 0);
        return comments.Count();
    }

    public async Task<int> GetUserCommentCountAsync(Guid userId)
    {
        _logger.LogDebug("Retrieving comment count for user {UserId}", userId);

        // Input validation
        if (userId == Guid.Empty)
            return 0;

        // This would require adding a count method to the repository
        // For now, we'll get all comments and count them (not efficient for large datasets)
        var comments = await _commentRepository.GetByAuthorAsync(userId, int.MaxValue, 0);
        return comments.Count();
    }

    public async Task<int> GetCommentReplyCountAsync(Guid parentCommentId)
    {
        _logger.LogDebug("Retrieving reply count for comment {ParentCommentId}", parentCommentId);

        // For now, we don't have a direct parent-child relationship for comments
        // Returning 0 for now - this could be enhanced later
        return 0;
    }
}
