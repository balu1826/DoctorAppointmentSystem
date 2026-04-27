using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model;
using AutoMapper;


namespace DoctorAppointmentSystem.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.FullName));
        }
    }
}
