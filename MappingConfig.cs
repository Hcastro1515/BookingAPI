using AlestheticApi.Models;
using AlestheticApi.Models.DTOs;
using AutoMapper;

namespace AlestheticApi;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Customer, CustomerDTO>().ReverseMap();
        CreateMap<Service, ServiceDTO>().ReverseMap();
        CreateMap<Employee, EmployeeDTO>().ReverseMap();
        CreateMap<Appointment, AppointmentDTO>().ReverseMap();
        CreateMap<User, UserDTO>().ReverseMap();
    }
}