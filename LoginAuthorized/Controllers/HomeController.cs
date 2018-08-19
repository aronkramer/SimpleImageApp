using LoginAuthorized.Data;
using LoginAuthorized.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using LoginAuthorized.Models;

namespace LoginAuthorized.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Uploading(Images image, HttpPostedFileBase fileImage)
        {
            string NewImage = Guid.NewGuid() + Path.GetExtension(fileImage.FileName);
            fileImage.SaveAs(Path.Combine(Server.MapPath("/uploadedimages"), NewImage));
            image.FileName = NewImage;
            dbManager db = new dbManager(Settings.Default.ConStr);
            var user = db.GetUserByEmail(User.Identity.Name);
            image.UserId = user.Id;
            db.AddImage(image);
            return View(image);
        }

        public ActionResult MyAccount()
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            return View(db.GetUsersImages(User.Identity.Name));
        }

        [HttpPost]
        public ActionResult DeleteImage(int id)
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            var userId = db.GetUsersImages(User.Identity.Name).Select(i => i.Id);
            if(!userId.Contains(id))
            {
                return Redirect("/home/myaccount");
            }
            db.DeleteImage(id);
            return Redirect("/home/myaccount");
        }
    }
}

/*Homework 5/03:

Building on the previous homework (Image share with password),
 make changes to the application so that only logged in users 
can actually upload images.

This will require creating signup and login pages.
 When a user comes to the home page (where they can
 upload images) they should be redirected to a login page.
 Once they log in they should get redirected to the home
 page where they can upload images. (Non logged in users
 can still see images uploaded by other users).

Finally, there should also be a page called "My Account"
 where they user can view a table with all their images
 they've uploaded, the view count, as well as a delete 
button. When delete is clicked, that image should get 
deleted from the database.*/