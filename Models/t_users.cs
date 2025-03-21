using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EmptyMVC.Models
{
    public class t_users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_patientid { get; set;}
        public string c_name { get; set;}
        public string c_email { get; set;}
        public string c_password { get; set;}
        public string c_confirmpassword { get; set;}
        public string c_gender { get; set;}
        public string c_mobile { get; set;}
        public string c_state { get; set;}
        public string c_city { get; set;}
        public string? c_image { get; set;}

        public IFormFile ProfilePicture { get; set; }
    }
}