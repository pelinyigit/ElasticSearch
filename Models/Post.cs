using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogCreator.Models
{
    public class Post
    {
        public int UserID { get; set; }
        public DateTime PostDate { get; set; }
        public string PostTest { get; set; }
        public string Text { get; internal set; }
    }
}