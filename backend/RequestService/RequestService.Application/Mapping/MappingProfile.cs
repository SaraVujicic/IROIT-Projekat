using System.Linq;
using AutoMapper;
using RequestService.Application.DTOs;
using RequestService.Domain.Entities;
using RequestService.Domain.ValueObjects;

namespace RequestService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Request, RequestDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.Name))
            .ForMember(d => d.TravelOrder, opt => opt.MapFrom(s => s.TravelOrders.FirstOrDefault()))
            .ForMember(d => d.Internship, opt => opt.MapFrom(s => s.Internships.FirstOrDefault()))
            .ForMember(d => d.Education, opt => opt.MapFrom(s => s.Educations.FirstOrDefault()));
            
        CreateMap<TravelOrder, TravelOrderDto>();
        CreateMap<Internship, InternshipDto>();
        CreateMap<Education, EducationDto>();
        CreateMap<MentorVO, MentorDto>();
    }
}
