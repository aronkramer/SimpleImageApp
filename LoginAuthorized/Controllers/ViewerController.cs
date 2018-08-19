using LoginAuthorized.Data;
using LoginAuthorized.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using LoginAuthorized.Models;

namespace LoginAuthorized.Controllers
{
    public class ViewerController : Controller
    {
        public ActionResult SignUp()
        {
            if (Request.IsAuthenticated)
            {
                return Redirect("/home/index");
            }
            return View();
        }

        public ActionResult Delete()
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            db.Delete();
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        [HttpPost]
        public ActionResult SignUp(User user, string Password)
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(Password, salt);
            db.AddUser(user, Password);
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return Redirect("/home/index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            var salt = db.GetSaltById(email);
            var hash = db.GetHashById(email);
            if(salt == null)
            {
                return RedirectToAction("login");
            }
            var input = PasswordHelper.PasswordMatch(password, salt, hash);
            if (input)
            {
                FormsAuthentication.SetAuthCookie(email, true);
            }
            
            return Redirect("/home/index");
        }

        public ActionResult ViewImage(int id)
        {
            ImageViewModel vm = new ImageViewModel();
            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }
            if (!PermissionToView(id))
            {
                vm.AllowedToViewImage = false;
                vm.Image = new Images { Id = id };
            }
            else
            {
                vm.AllowedToViewImage = true;
                dbManager db = new dbManager(Settings.Default.ConStr);
                db.IncrementView(id);
                var image = db.ViewImage(id);
                if (image == null)
                {
                    return RedirectToAction("index");
                }
                vm.Image = image;
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult ViewImage(int id, string password)
        {
            dbManager db = new dbManager(Settings.Default.ConStr);
            var image = db.ViewImage(id);
            if (image == null)
            {
                return Redirect("/home/index");
            }
            if (password != image.Password)
            {
                TempData["message"] = "invalid password";
            }
            else
            {
                List<int> allowedIds;
                if (Session["allowedIds"] == null)
                {
                    allowedIds = new List<int>();
                    Session["allowedIds"] = allowedIds;
                }
                else
                {
                    allowedIds = (List<int>)Session["allowedIds"];
                }
                allowedIds.Add(id);
            }
            return Redirect($"/viewer/viewimage?id={id}");
        }

        private bool PermissionToView(int id)
        {
            if (Session["allowedIds"] == null)
            {
                return false;
            }
            var ids = (List<int>)Session["allowedIds"];
            return ids.Contains(id);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/");
        }
    }
}