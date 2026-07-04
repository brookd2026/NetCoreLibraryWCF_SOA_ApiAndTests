using BookLibrary_WCFService.Models;
using Microsoft.EntityFrameworkCore;
using CoreWCF;
using BookLibrary_WCFService;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CoreLibraryApiTesting
{
    public class LibraryServiceTests
    {
        // Helper to create a mocked HttpContextAccessor that supplies a real cancellation token
        private IHttpContextAccessor CreateMockHttpContextAccessor(CancellationToken token)
        {
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.RequestAborted).Returns(token);

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(a => a.HttpContext).Returns(mockContext.Object);

            return mockAccessor.Object;
        }

        // Helper method to create an isolated in-memory DB context for each test run
        private LibraryDbContext GetDbContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
              .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
              .Options;

            return new LibraryDbContext(options);
        }

        [Fact]
        public async Task AddBookAsync_ReturnsCount1_WhenNewDatabase()
        {
            // Arrange 
            var book = new BookDataContract { IsAvailable = true, Title = "My Computer Book" };
            var dbName = Guid.NewGuid().ToString();

            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);

            using (var addContext = GetDbContext(dbName))
            {
                var service = new LibraryService(addContext, mockAccessor);

                // Act & Assert
                await service.AddBookAsync(book);
                Assert.True(addContext.Books.Count() == 1);
            }

            using (var getContext = GetDbContext(dbName))
            {
                var savedBook = getContext.Books.Find(1);

                // Assert
                Assert.True(getContext.Books.Count() == 1);
                Assert.Equal("My Computer Book", savedBook?.Title);
            }
        }

        [Fact]
        public async Task GetBookByIdAsync_ThrowsOperationCanceledException_WhenTokenIsCancelled()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var addContext = GetDbContext(dbName))
            {
                await addContext.Books.AddAsync(new Book { Id = 1, Title = "Canceled Book Test" });
                await addContext.SaveChangesAsync();
            }

            // 1. Force the token to be immediately tripped/cancelled
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // 2. Inject the cancelled token into our mock setup
            var mockAccessor = CreateMockHttpContextAccessor(cts.Token);

            using (var getContext = GetDbContext(dbName))
            {
                var service = new LibraryService(getContext, mockAccessor);

                // Act & Assert
                // Verify that EF Core halts execution and throws an OperationCanceledException
                var operationCanceledException = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                {
                    await service.GetBookByIdAsync(1);
                });

                Assert.Equal("The operation was canceled.", operationCanceledException.Message);
            }
        }

        [Fact]
        public async Task GetBookByIdAsync_ReturnsBook_WhenBookExists()
        {
            // Arrange
            var book = new Book { IsAvailable = true, Title = "First Computer Book" };
            var dbName = Guid.NewGuid().ToString();

            var addContext = GetDbContext(dbName);

            // Act
            await addContext.Books.AddAsync(book);
            await addContext.SaveChangesAsync();
            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);

            var getContext = GetDbContext(dbName);
            var getService = new LibraryService(getContext, mockAccessor);
            var getResult = await getService.GetBookByIdAsync(1);

            // Assert
            Assert.Equal(1, addContext.Books.Count());
            Assert.Equal("First Computer Book", getResult.Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_ThrowsFaultException_WhenBookDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = GetDbContext();
            
            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);

            var service = new LibraryService(context, mockAccessor);

            var thrownException = await Assert.ThrowsAsync<FaultException<BookFault>>(async () =>
            {
                await service.GetBookByIdAsync(1);
            });

            Assert.Equal("Invalid book ID", thrownException.Reason.ToString());
            Assert.Equal("Book with ID 1 was not found.", thrownException.Detail.ErrorMessage);
        }

        [Fact]
        public async Task RemoveBookAsync_ReturnsId_WhenBookIsDeletedSuccessfully()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString(); ;

            var book = new Book { IsAvailable = true, Title = "My New Book" };

            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);

            // Act & Assert - Add
            using (var addContext = GetDbContext(dbName))
            {
                await addContext.AddAsync(book);
                await addContext.SaveChangesAsync();

                Assert.True(addContext.Books.Count() == 1);
            }

            // Act & Assert - Delete
            using (var deleteContext = GetDbContext(dbName))
            {
                var deleteService = new LibraryService(deleteContext, mockAccessor);
                var deletedResult = await deleteService.RemoveBookAsync(1);

                Assert.True(deletedResult.IsDeleted);
                Assert.Equal(0, deleteContext.Books.Count());
                Assert.Equal("My New Book", deletedResult.DeletedTitle);
            }
        }

        [Fact]
        public async Task RemoveBookAsync_ThrowsFaultException_WhenBookDoesNotExist()
        {
            // arrange 
            var guid = Guid.NewGuid().ToString();
            var context = GetDbContext(guid);
            
            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);
            
            var service = new LibraryService(context, mockAccessor);

            var thrownException = await Assert.ThrowsAsync<FaultException<BookFault>>(async () =>
            {
                await service.RemoveBookAsync(999);
            });

            Assert.Equal("Book with ID 999 was not found.", thrownException?.Detail.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsTrue_WhenBookIsUpdatedSuccessfully()
        {
            // Arrange 
            var dbName = Guid.NewGuid().ToString();
            var addBook = new Book { IsAvailable = false, Title = "What a book" };
            
            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);
            
            // Act & Assert - Add
            using (var addContext = GetDbContext(dbName))
            {
                await addContext.Books.AddAsync(addBook);
                await addContext.SaveChangesAsync();

                // Assert
                Assert.True(addContext.Books.Count() == 1);
            }

            var updatedBook = new BookDataContract { Id = 1, IsAvailable = true, Title = "Bad Book" };

            // Act
            using (var updatedContext = GetDbContext(dbName))
            {
                var service = new LibraryService(updatedContext, mockAccessor);
                var result = await service.UpdateBookAsync(updatedBook);
            }

            using (var findContext = GetDbContext(dbName))
            {
                var book = await findContext.Books.FindAsync(1);

                // Assert
                Assert.True(book?.IsAvailable);
                Assert.Equal("Bad Book", book?.Title);
            }
        }

        [Fact]
        public async Task UpdateBookAsync_ThrowsFaultException_WhenBookIsNotFound()
        {
            // Arrange
            var book = new BookDataContract { Id = 1, IsAvailable = true, Title = "UpdatedBook" };

            var context = GetDbContext();

            // Create a safe, uncancelled context accessor mock
            var mockAccessor = CreateMockHttpContextAccessor(CancellationToken.None);

            var service = new LibraryService(context, mockAccessor);


            // Act
            var thrownException = await Assert.ThrowsAsync<FaultException<BookFault>>(async () =>
            {
                await service.UpdateBookAsync(book);
            });


            // Assert
            Assert.Equal("Book with ID 1 was not found.", thrownException?.Detail.ErrorMessage);
        }
    }
}
