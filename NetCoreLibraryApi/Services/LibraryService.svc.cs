using BookLibrary_WCFService.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using CoreWCF;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary_WCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ILibraryService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ILibraryService.svc or ILibraryService.svc.cs at the Solution Explorer and start debugging.
    
    public class LibraryService : ILibraryService
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BookRepository";
        private LibraryDbContext _context;
  
        public LibraryService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<BookDataContract> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.Where(b => b.Id == id).FirstOrDefaultAsync();

            if (book == null)
            {
                var fault = new BookFault { ErrorMessage = $"Book with ID {id} was not found." };
                throw new FaultException<BookFault>(fault, new FaultReason("Invalid book ID"));
            }

            var bookDataContract = new BookDataContract() { Id = book.Id, Title = book.Title, IsAvailable = book.IsAvailable };
            return bookDataContract;
        }

        public async Task<SaveBookResult> AddBookAsync(BookDataContract book)
        {
            var databaseModel = new Book()
            {
                Id = book.Id,
                Title = book.Title,
                IsAvailable = book.IsAvailable
            };

            await _context.Books.AddAsync(databaseModel);
            int rowsChanged = await _context.SaveChangesAsync();
            var output = (rowsChanged > 0);
            var newId = databaseModel.Id;
            return new SaveBookResult { IsCompleted = output, NewId = newId };
        }

        public async Task<DeleteBookResult> RemoveBookAsync(int id)
        {
            var bookToRemove = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (bookToRemove == null)
            {
                var fault = new BookFault { ErrorMessage = $"Book with ID {id} was not found." };
                throw new FaultException<BookFault>(fault, new FaultReason("Invalid Book Id"));
            }

            _context.Books.Remove(bookToRemove);
            bool isDeleted = await _context.SaveChangesAsync() > 0;
            return new DeleteBookResult { DeletedId = id, DeletedTitle = bookToRemove.Title, IsDeleted = isDeleted };
        }

        public async Task<bool> UpdateBookAsync(BookDataContract book)
        {
            var bookToUpdate = await _context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);

            if (bookToUpdate == null)
            {
                var fault = new BookFault { ErrorMessage = $"Book with ID {book.Id} was not found." };
                throw new FaultException<BookFault>(fault, new FaultReason("Invalid Book Id"));
            }

            bookToUpdate.Title = book.Title;
            bookToUpdate.IsAvailable = book.IsAvailable;
            var rowsChanged = await _context.SaveChangesAsync();
            return rowsChanged > 0;
        }
    }
}
