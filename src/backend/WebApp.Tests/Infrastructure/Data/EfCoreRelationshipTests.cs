using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApp.Common.Data;
using WebApp.Common.Entities;

namespace WebApp.Tests.Infrastructure.Data;

public class EfCoreRelationshipTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public EfCoreRelationshipTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void ApplicationDbContext_Should_Initialize_Without_ModelValidation_Errors()
    {
        // This test will fail if there are relationship configuration issues
        // such as the Post.ParentPost and Post.RootPost self-referencing relationship problem
        
        // Act & Assert - This should not throw an InvalidOperationException
        var exception = Record.Exception(() =>
        {
            // Force model creation and validation
            var model = _context.Model;
            
            // Try to access the Post entity to trigger model validation
            var postEntityType = model.FindEntityType(typeof(Post));
            postEntityType.Should().NotBeNull();
            
            // Ensure the context can be used for operations
            _context.Database.EnsureCreated();
        });

        // Assert that no exception was thrown during model creation
        exception.Should().BeNull("EF Core model should be valid without relationship configuration errors");
    }

    [Fact]
    public void Post_SelfReferencing_Relationships_Should_Be_Configured_Correctly()
    {
        // Arrange & Act
        var model = _context.Model;
        var postEntityType = model.FindEntityType(typeof(Post));
        
        // Assert
        postEntityType.Should().NotBeNull();
        
        // Check that ParentPost relationship is configured
        var parentPostNavigation = postEntityType.FindNavigation("ParentPost");
        parentPostNavigation.Should().NotBeNull("ParentPost navigation should exist");
        
        // Check that RootPost relationship is configured  
        var rootPostNavigation = postEntityType.FindNavigation("RootPost");
        rootPostNavigation.Should().NotBeNull("RootPost navigation should exist");
        
        // Verify that both relationships have proper foreign key configurations
        var parentPostProperty = postEntityType.FindProperty("ParentPostId");
        parentPostProperty.Should().NotBeNull("ParentPostId property should exist");
        var parentPostForeignKey = postEntityType.FindForeignKeys(parentPostProperty!).FirstOrDefault();
        parentPostForeignKey.Should().NotBeNull("ParentPost foreign key should be configured");
        parentPostForeignKey!.IsRequired.Should().BeFalse("ParentPost relationship should be optional");
        
        var rootPostProperty = postEntityType.FindProperty("RootPostId");
        rootPostProperty.Should().NotBeNull("RootPostId property should exist");
        var rootPostForeignKey = postEntityType.FindForeignKeys(rootPostProperty!).FirstOrDefault();
        rootPostForeignKey.Should().NotBeNull("RootPost foreign key should be configured");  
        rootPostForeignKey!.IsRequired.Should().BeFalse("RootPost relationship should be optional");
        
        // Verify that the relationships are configured as many-to-one, not one-to-one
        parentPostNavigation.ForeignKey.IsUnique.Should().BeFalse("ParentPost should be many-to-one relationship");
        rootPostNavigation.ForeignKey.IsUnique.Should().BeFalse("RootPost should be many-to-one relationship");
    }

    [Fact]
    public void Post_Can_Create_SelfReferencing_Reply_Chain()
    {
        // Arrange
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _context.Users.Add(user);
        _context.SaveChanges();
        
        // Create root post
        var rootPost = new Post(user.Id, "This is the root post");
        _context.Posts.Add(rootPost);
        _context.SaveChanges();
        
        // Create reply to root post
        var replyPost = new Post(user.Id, "This is a reply", parentPostId: rootPost.Id, rootPostId: rootPost.Id);
        _context.Posts.Add(replyPost);
        _context.SaveChanges();
        
        // Create reply to reply
        var nestedReply = new Post(user.Id, "This is a reply to the reply", parentPostId: replyPost.Id, rootPostId: rootPost.Id);
        
        // Act & Assert - This should work without throwing relationship configuration errors
        var exception = Record.Exception(() =>
        {
            _context.Posts.Add(nestedReply);
            _context.SaveChanges();
        });
        
        exception.Should().BeNull("Creating nested replies should not cause relationship errors");
        
        // Verify the chain is correctly established
        var savedNestedReply = _context.Posts
            .Include(p => p.ParentPost)
            .Include(p => p.RootPost)
            .First(p => p.Id == nestedReply.Id);
            
        savedNestedReply.ParentPost.Should().NotBeNull();
        savedNestedReply.ParentPost!.Id.Should().Be(replyPost.Id);
        savedNestedReply.RootPost.Should().NotBeNull();
        savedNestedReply.RootPost!.Id.Should().Be(rootPost.Id);
    }

    [Fact]
    public void Post_Can_Have_Optional_Parent_And_Root_References()
    {
        // Arrange
        var user = new User("test@example.com", "testuser", "Test User", "hashedpassword");
        _context.Users.Add(user);
        _context.SaveChanges();
        
        // Act - Create a standalone post (no parent or root)
        var standalonePost = new Post(user.Id, "This is a standalone post");
        
        var exception = Record.Exception(() =>
        {
            _context.Posts.Add(standalonePost);
            _context.SaveChanges();
        });
        
        // Assert
        exception.Should().BeNull("Creating posts without parent/root references should work");
        
        var savedPost = _context.Posts
            .Include(p => p.ParentPost)
            .Include(p => p.RootPost)
            .First(p => p.Id == standalonePost.Id);
            
        savedPost.ParentPost.Should().BeNull("ParentPost should be optional");
        savedPost.RootPost.Should().BeNull("RootPost should be optional");
        savedPost.ParentPostId.Should().BeNull("ParentPostId should be null");
        savedPost.RootPostId.Should().BeNull("RootPostId should be null");
    }

    [Fact]
    public void DbContext_Model_Validation_Should_Not_Throw_For_Post_Relationships()
    {
        // This test specifically targets the error message we're seeing:
        // "The dependent side could not be determined for the one-to-one relationship between 'Post.ParentPost' and 'Post.RootPost'"
        
        // Act - Force model validation by accessing entity configurations
        var exception = Record.Exception(() =>
        {
            var model = _context.Model;
            var postEntity = model.FindEntityType(typeof(Post));
            
            // This will trigger model validation and should reveal relationship configuration issues
            var foreignKeys = postEntity!.GetForeignKeys();
            
            foreach (var fk in foreignKeys)
            {
                // Access properties that would trigger validation
                var principal = fk.PrincipalEntityType;
                var dependent = fk.DeclaringEntityType;
                var isUnique = fk.IsUnique;
                var isRequired = fk.IsRequired;
                
                // Log the relationship details for debugging
                System.Console.WriteLine($"FK: {fk.Properties.First().Name} -> {principal.ClrType.Name}.{fk.PrincipalKey.Properties.First().Name}, IsUnique: {isUnique}, IsRequired: {isRequired}");
            }
        });
        
        // Assert
        exception.Should().BeNull("Post entity relationships should be properly configured without validation errors");
    }
}
