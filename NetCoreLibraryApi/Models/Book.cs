using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookLibrary_WCFService.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsAvailable { get; set; }
    }
}