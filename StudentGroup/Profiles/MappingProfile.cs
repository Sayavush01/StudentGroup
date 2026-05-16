using AutoMapper;
using StudentGroup.Entities;
using StudentGroup.DTOs.EventDtos;
using StudentGroup.DTOs.OrganizerDtos;
using StudentGroup.DTOs.TicketDtos;
using StudentGroup.Data;
using StudentGroup.Models;
using StudentGroup.DTOs.UserDtos;

namespace StudentGroup.Profiles
{
    public class MappingProfile:Profile
    {

        public MappingProfile()
            {
                CreateMap<Event, EventGetdto>();
                CreateMap<EventCreateDto, Event>();
                CreateMap<EventUpdatedto, Event>();
                CreateMap<Organizer, OrganizerGetDto>();
                CreateMap<OrganizerCreate, Organizer>();
                CreateMap<OrganizerUpdateDto, Organizer>();
                CreateMap<Ticket, TicketGetDto>();
                CreateMap<TicketCreate, Ticket>();
                CreateMap<TicketUpdateDto, Ticket>();
            CreateMap<RegisterDto, AppUser>();
        }
    }
}
