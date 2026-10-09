using AutoMapper;
using BolsaTrabajo.Entities.Dtos;
using BolsaTrabajo.Entities.Models;

namespace BolsaTrabajo.BL.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Plaza <-> PlazaDto
        CreateMap<Plaza, PlazaDto>()
            .ForMember(destination => destination.NombreAgente, opts => opts.MapFrom(source => source.Agente != null ? source.Agente.NombreCompleto : null))
            .ForMember(destination => destination.TotalPostulaciones, opts => opts.MapFrom(source => source.Postulaciones.Count));

        CreateMap<PlazaDto, Plaza>()
            .ForMember(destination => destination.Agente, opts => opts.Ignore())
            .ForMember(destination => destination.Postulaciones, opts => opts.Ignore());

        // Postulacion -> PostulacionDto
        CreateMap<Postulacion, PostulacionDto>()
            .ForMember(destination => destination.TituloPlaza, opts => opts.MapFrom(source => source.Plaza != null ? source.Plaza.Titulo : string.Empty))
            .ForMember(destination => destination.Institucion, opts => opts.MapFrom(source => source.Plaza != null ? source.Plaza.Institucion : string.Empty))
            .ForMember(destination => destination.EstadoPlaza, opts => opts.MapFrom(source => source.Plaza != null ? source.Plaza.Estado : default))
            .ForMember(destination => destination.NombreCandidato, opts => opts.MapFrom(source => source.Candidato != null ? source.Candidato.NombreCompleto : string.Empty))
            .ForMember(destination => destination.EmailCandidato, opts => opts.MapFrom(source => source.Candidato != null ? source.Candidato.Email : string.Empty));

        // HojaDeVida <-> HojaDeVidaDto
        CreateMap<HojaDeVida, HojaDeVidaDto>()
            .ForMember(destination => destination.NombreCandidato, opts => opts.MapFrom(source => source.Usuario != null ? source.Usuario.NombreCompleto : null))
            .ForMember(destination => destination.EmailCandidato, opts => opts.MapFrom(source => source.Usuario != null ? source.Usuario.Email : null));

        CreateMap<HojaDeVidaDto, HojaDeVida>()
            .ForMember(destination => destination.Usuario, opts => opts.Ignore());

        // Usuario -> UsuarioDto
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(destination => destination.Telefono, opts => opts.MapFrom(source => source.PhoneNumber))
            .ForMember(destination => destination.Email, opts => opts.MapFrom(source => source.Email ?? string.Empty));
    }
}
