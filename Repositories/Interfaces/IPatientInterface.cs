using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmptyMVC.Models;

namespace EmptyMVC.Repositories.Interfaces
{
    public interface IPatientInterface
    {
        Task<int> Register(t_users patient);
        Task<t_users> Login(t_users patient);
        Task<t_users> GetUserById(int patientId);
        Task<bool> UpdateProfile(t_users patient);
    }
}