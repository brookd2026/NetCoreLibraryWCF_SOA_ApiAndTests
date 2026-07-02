using BookLibrary_WCFService.Models;
using Microsoft.EntityFrameworkCore;
using CoreWCF;
using BookLibrary_WCFService;
using System.Xml;

namespace CoreLibraryApiTesting
{
    public class LibraryServiceTests
    {
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

            using (var addContext = GetDbContext(dbName))
            {
                var service = new LibraryService(addContext);

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
        public async Task GetBookByIdAsync_ReturnsBook_WhenBookExists()
        {
            // Arrange
            var book = new Book { IsAvailable = true, Title = "First Computer Book" };
            var dbName = Guid.NewGuid().ToString();

            var addContext = GetDbContext(dbName);

            // Act
            await addContext.Books.AddAsync(book);
            await addContext.SaveChangesAsync();

            var getContext = GetDbContext(dbName);
            var getService = new LibraryService(getContext);
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
            var service = new LibraryService(context);

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
                var deleteService = new LibraryService(deleteContext);
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
            var service = new LibraryService(context);

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
                var service = new LibraryService(updatedContext);
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
            var service = new LibraryService(context);


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
