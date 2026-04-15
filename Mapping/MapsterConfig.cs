using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model;
using Mapster;

namespace DoctorAppointmentSystem.Mapping
{
    public class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<ApplicationUser, UserDTO>
                .NewConfig();
            TypeAdapterConfig<Patient, PatientProfileDTO>
               .NewConfig();

        }
    }
}
