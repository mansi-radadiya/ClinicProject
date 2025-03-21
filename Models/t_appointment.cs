using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmptyMVC.Models
{
    public class t_appointment
    {
        public int c_appointmentid { get; set; }

        [Required]
        public int c_patientid { get; set; }

        [Required]
        public int c_departmentid { get; set; }

        [Required]
        public DateOnly c_date { get; set; }

        [Required]
        public TimeOnly c_time { get; set; }

        public string? c_patientName { get; set; }
        public string? c_departmentName { get; set; }
    }
}