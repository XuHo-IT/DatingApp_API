using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
   public AutoMapperProfiles()
   {
    CreateMap<AppUser,MemberDto>()
      .ForMember(d=>d.Age,o=>o.MapFrom(s=>s.DateOfBirth.CalculateAge()))
      .ForMember(d =>d.PhotoUrl,
      o=>o.MapFrom(s=>s.Photos.FirstOrDefault(x=>x.IsMain)!.Url));
    CreateMap<Photo,PhotoDto>();  
    CreateMap<MemberUpdateDto,AppUser>();
    CreateMap<RegisterDto, AppUser>();
    CreateMap<DateTime, DateOnly>().ConvertUsing(src => DateOnly.FromDateTime(src));
    CreateMap<DateTimeOffset, DateOnly>().ConvertUsing(src => DateOnly.FromDateTime(src.DateTime));
    CreateMap<Messages, MessagesDto>()
    .ForMember(d => d.SenderPhotoUrl, 
    o => o.MapFrom(s=> s.Sender.Photos.FirstOrDefault(x=>x.IsMain)!.Url))
    .ForMember(d => d.RecipientPhotoUrl, 
    o => o.MapFrom(s=> s.Recipient.Photos.FirstOrDefault(x=>x.IsMain)!.Url));

   }
}
