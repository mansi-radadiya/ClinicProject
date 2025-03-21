using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmptyMVC.Models;

namespace EmptyMVC.Repositories.Interfaces
{
    public interface IAppointmentInterface
    {
        Task<t_appointment?> GetAppointmentById(int id);

        Task<int> BookAppointment(t_appointment appointment);

        Task<int> UpdateAppointment(t_appointment appointment);

        Task<int> DeleteAppointment(int id);

        Task<List<t_appointment>> GetAppointmentsByPatientId(int patientId);

        Task<List<t_department>> GetAllDepartments();
    }
}