using CoreWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BookLibrary_WCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IILibraryService" in both code and config file together.
    [ServiceContract]
    public interface ILibraryService
    {
        [OperationContract]
        Task<List<BookDataContract>> GetAllBooksAsync();

        [OperationContract]
        [FaultContract(typeof(BookFault))]
        Task<BookDataContract> GetBookByIdAsync(int id);

        [OperationContract]
        Task<SaveBookResult> AddBookAsync(BookDataContract book);

        [OperationContract]
        Task<DeleteBookResult> RemoveBookAsync(int id);

        [OperationContract]
        Task<bool> UpdateBookAsync(BookDataContract book);

        [OperationContract]
        Task<Stream> DownloadBookFileAsync(int bookId);
    }

    [DataContract]
    public class BookDataContract
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public bool IsAvailable { get; set; }
    }

    [DataContract]
    public class SaveBookResult
    {
        [DataMember]
        public bool IsCompleted { get; set; }

        [DataMember]
        public int NewId { get; set; }
    }

    [DataContract]
    public class DeleteBookResult
    {
        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public int DeletedId { get; set; }

        [DataMember]
        public string DeletedTitle { get; set; }
    }

}
