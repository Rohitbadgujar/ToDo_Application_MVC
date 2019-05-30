using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoMVCApplication.Models
{
    public class Task
    {
        public int TaskId { get; set; }
        public String TaskName { get; set; }
        public String TaskDescription { get; set; }
        public bool DeleteInd { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public bool ImportantInd { get; set; }

    }
}