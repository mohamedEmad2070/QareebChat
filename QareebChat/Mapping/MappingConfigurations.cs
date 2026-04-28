using Mapster;
using QareebChat.Contracts.User;
using QareebChat.Entities;

namespace QareebChat.Mapping;

public class MappingConfigurations : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<ApplicationUser, GetProfileResponse>()
			.Map(dest => dest.Id, src => src.Id)
			.Map(dest => dest.FirstName, src => src.FirstName)
			.Map(dest => dest.LastName, src => src.LastName)
			.Map(dest => dest.UserName, src => src.UserName)
			.Map(dest => dest.Email, src => src.Email)
			.Map(dest => dest.ProfilePictureUrl, src => src.ProfilePictureUrl);
	}
}
