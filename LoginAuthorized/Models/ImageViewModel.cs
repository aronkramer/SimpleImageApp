using LoginAuthorized.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginAuthorized.Models
{
    public class ImageViewModel
    {
        public string Message { get; set; }
        public bool AllowedToViewImage { get; set; }
        public Images Image { get; set; }
    }
}