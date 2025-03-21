using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EmptyMVC.Models;
using EmptyMVC.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmptyMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly IPatientInterface _patientRepo;

        public PatientController (IPatientInterface patientRepo)
        {
            _patientRepo = patientRepo;
        }

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
         public async Task<JsonResult> Register(t_users user)
        {
            try
            {
                if (user.ProfilePicture != null && user.ProfilePicture.Length > 0)
                {
                    var fileName = user.c_email + Path.GetExtension(user.ProfilePicture.FileName);
                    var filePath = Path.Combine("wwwroot/profile_images", fileName);

                    Directory.CreateDirectory(Path.Combine("wwwroot/profile_images"));
                    user.c_image = fileName;

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await user.ProfilePicture.CopyToAsync(stream);
                    }
                }

                int result = await _patientRepo.Register(user);

                if (result == 0)
                {
                    return Json(new { success = false, message = "Email already exists. Please use a different email." });
                }
                else if (result == 1)
                {
                    return Json(new { success = true, message = "Registration Successful!!", redirectUrl = Url.Action("Login", "User") });
                }
                else
                {
                    return Json(new { success = false, message = "Registration failed due to an unexpected error." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion
    
        #region Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
         public async Task<IActionResult> Login(t_users patient)
        {
            var userData = await  _patientRepo.Login(patient);
            if (userData != null)
            {
            HttpContext.Session.SetInt32("userid", userData.c_patientid);
            HttpContext.Session.SetString("userName", userData.c_name);
            HttpContext.Session.SetString("userEmail", userData.c_email);
            HttpContext.Session.SetString("userGender", userData.c_gender);
            HttpContext.Session.SetString("userMobile", userData.c_mobile);
            HttpContext.Session.SetString("userState", userData.c_state);
            HttpContext.Session.SetString("userCity", userData.c_city);
            HttpContext.Session.SetString("userImage", userData.c_image);
            return Json(new { success = true, message = "Registration Successful!!",userData, redirectUrl = Url.Action("List", "Appointment")});
            }
            return Json(new { success = false, message = "Invalid email or password" });
        }
        #endregion
    
        #region UpdateProfile
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var patientId = HttpContext.Session.GetInt32("userid");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Patient");
            }

            var patient = await _patientRepo.GetUserById(patientId.Value);
            if (patient == null)
            {
                return RedirectToAction("Login", "Patient");
            }

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(t_users patient, IFormFile ProfilePicture)
        {
            if (ModelState.IsValid)
            {
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    var fileName = patient.c_email + Path.GetExtension(ProfilePicture.FileName);
                    var filePath = Path.Combine("wwwroot/profile_images", fileName);

                    Directory.CreateDirectory(Path.Combine("wwwroot/profile_images"));
                    patient.c_image = fileName;

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilePicture.CopyToAsync(stream);
                    }
                }

                var result = await _patientRepo.UpdateProfile(patient);
                if (result)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("UpdateProfile", "Patient");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update profile.");
                }
            }

            return View(patient);
        }
        #endregion
        
        #region Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Patient");
        }
        #endregion
    }
}