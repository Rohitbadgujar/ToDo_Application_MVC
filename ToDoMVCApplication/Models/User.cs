using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoMVCApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        public String UserName { get; set; }
        public String Name { get; set; }
        public String EmailId { get; set; }
        public String Password { get; set; }
        public bool DeleteInd { get; set; }
    }
}