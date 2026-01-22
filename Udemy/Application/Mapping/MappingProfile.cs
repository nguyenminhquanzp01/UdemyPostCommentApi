namespace Udemy.Application.Mapping;

using AutoMapper;
using Udemy.Application.DTOs;
using Udemy.Domain.Models;

/// <summary>
/// AutoMapper profile for entity to DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class.
    /// </summary>
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ReverseMap();

        // Author mappings (ID and Name only)
        CreateMap<User, AuthorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        // Post mappings
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<Post, PostDetailsDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));

        // Comment mappings
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

        // Comment tree mappings
        CreateMap<Comment, CommentTreeDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));
    }
}
