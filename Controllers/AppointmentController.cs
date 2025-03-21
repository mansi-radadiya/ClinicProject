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
    public class AppointmentController : Controller
    {
       private readonly ILogger<AppointmentController> _logger;
        private readonly IAppointmentInterface _appointmentRepo;

        public AppointmentController(ILogger<AppointmentController> logger, IAppointmentInterface appointmentRepo)
        {
            _logger = logger;
            _appointmentRepo = appointmentRepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            return View();
        }

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] t_appointment appointment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                    });
                }

                var status = await _appointmentRepo.BookAppointment(appointment);
                switch (status)
                {
                    case 1:
                        return Ok(new { success = true, message = "Appointment created successfully!" });
                    case -1:
                        return BadRequest(new { success = false, message = "Failed to create appointment. Please try again." });
                    case -2:
                        return BadRequest(new { success = false, message = "This department is already booked for the selected date and time. Please choose a different time slot." });
                    default:
                        return BadRequest(new { success = false, message = "Failed to create appointment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }
        #endregion

        #region Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] t_appointment appointment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                    });
                }

                var status = await _appointmentRepo.UpdateAppointment(appointment);
                switch (status)
                {
                    case 1:
                        return Ok(new { success = true, message = "Appointment updated successfully!" });
                    case -1:
                        return BadRequest(new { success = false, message = "Failed to update appointment. Please try again." });
                    case -2:
                        return BadRequest(new { success = false, message = "This department is already booked for the selected date and time. Please choose a different time slot." });
                    default:
                        return BadRequest(new { success = false, message = "Failed to update appointment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var status = await _appointmentRepo.DeleteAppointment(id);
                switch (status)
                {
                    case 1:
                        return Ok(new { success = true, message = "Appointment deleted successfully!" });
                    case -1:
                        return BadRequest(new { success = false, message = "Failed to delete appointment. Please try again." });
                    default:
                        return BadRequest(new { success = false, message = "Failed to delete appointment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }
        #endregion

        #region GetAppointment
        [HttpGet]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentRepo.GetAppointmentById(id);
                if (appointment == null)
                {
                    return NotFound(new { success = false, message = "Appointment not found." });
                }
                return Ok(new { success = true, data = appointment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }
        #endregion

        #region GetAppointmentsByPatient
        [HttpGet]
        public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsByPatientId(patientId);
                return Ok(new { success = true, data = appointments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient appointments");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }
        #endregion

        #region GetDepartments
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _appointmentRepo.GetAllDepartments();
                return Ok(new { success = true, departments = departments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
            #endregion
        }
    }
}