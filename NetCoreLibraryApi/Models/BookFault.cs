using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace BookLibrary_WCFService
{
    [DataContract]
    public class BookFault
    {
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}