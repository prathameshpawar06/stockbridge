using AutoMapper;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Define the mapping between Client and ClientDTO
        CreateMap<Client, ClientDTO>();
        CreateMap<BasicTab, Client>();
        CreateMap<ClientContact, Contact>().ReverseMap();
        CreateMap<Location, ClientLocation>().ReverseMap();
        CreateMap<Entities, ClientEntity>().ReverseMap();
        CreateMap<ClientStaff, StaffModel>().ReverseMap();
        //For Timesheet
        CreateMap<TimeDatumRequest, TimeSheet>().ReverseMap();

        //For Staff 
        CreateMap<Staff, StaffViewModel>().ReverseMap();
        CreateMap<Staff,StaffRequest>().ReverseMap();
        //For Carrier   
        CreateMap<Carrier, CarrierModel>().ReverseMap();
        CreateMap<Carrier, CarrierRequest>().ReverseMap();
        //For broker
        CreateMap<Broker, BrokerModel>().ReverseMap();
        CreateMap<Broker, BrokerRequest>().ReverseMap();

        //For Policy
        CreateMap<TemplatePrincipal, TemplatePrincipalModel>().ReverseMap();
        CreateMap<TemplatePrincipal, ReqTemplatePrincipalModel>().ReverseMap();
        CreateMap<TemplateMajor, ReqTemplateMajor>().ReverseMap();
        CreateMap<TemplateMajorColDef, ReqTemplateMajorColDef>().ReverseMap();
        CreateMap<TemplateMinorDef, ReqTemplateMinorDef>().ReverseMap();

        CreateMap<PolicyRequest, Policy>().ReverseMap()
             .ForMember(dest => dest.PolicyMajors, opt => opt.Ignore());
        CreateMap<Policy, PolicyModel>().ReverseMap();

        //Major & Minor
        CreateMap<PolicyMajorModel, PolicyMajor>().ReverseMap();
        CreateMap<PolicyMajorColDefModel, PolicyMajorColDef>().ReverseMap();
        CreateMap<PolicyMinorDefModel, PolicyMinorDef>().ReverseMap();


        

    }
}
