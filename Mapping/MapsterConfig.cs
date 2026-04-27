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
                .NewConfig()
    .RequireDestinationMemberSource(true)
  .Map(dest => dest.Name, src => src.FullName)
    .Map(dest => dest.Email, src => src.Email!);
            TypeAdapterConfig<Patient, PatientProfileDTO>
               .NewConfig();

        }
        private static string GetName(ApplicationUser src)
        {
            var value = src.FullName;   
            return value;
        }
    }
}
