using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock
{
      public class ProfileService : IProfileService
      {
            private UserDto _User;

            public ProfileService()
            {
                  _User = new UserDto
                  {
                        Id = Guid.NewGuid(),
                        Username = "Abdoul Raouf",
                        Email = "AbdoulRaoufHarouna@gmail.com",
                        FullName = "Abdoul Raouf Harouna Moussa",
                        Bio = "je suis un developpeur passionné par la tech",
                        AvatarUrl = "./Images/upload/user.png",
                        Country = "Niger",
                        City = "Niamey",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 1, 15),
                        LastLoginAt = DateTime.Now,
                        Roles = new List<string> { "Admin", "SuperAdmin" },
                        SocialLinks = new List<SocialLinkDto>
                              {
                                    new() { Id = Guid.NewGuid(), Platform = "LinkedIn", Url = "https://linkedin.com/in/jeandupont" },
                                    new() { Id = Guid.NewGuid(), Platform = "GitHub", Url = "https://github.com/jeandupont" },
                                    new() { Id = Guid.NewGuid(), Platform = "Twitter", Url = "https://twitter.com/jeandupont" }
                              }

                  };
            }
            public async Task<UserDto> GetProfileAsync() => await Task.FromResult(_User);
            public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request) { 
                  var user = _User;
                  user.FullName = request.FullName;
                  user.Bio = request.Bio;
                  user.AvatarUrl = request.AvatarUrl;
                  user.Country = request.Country;
                  user.City = request.City;
                  return await Task.FromResult(user); 
            }

            public async Task<List<SocialLinkDto>> GetSocialLinksAsync()
                  => await Task.FromResult(_User.SocialLinks);

            public async Task<SocialLinkDto?> AddSocialLinkAsync(AddSocialLinkRequest request)
            {
                  var link = new SocialLinkDto
                  {
                        Id = Guid.NewGuid(),
                        Platform = request.Platform,
                        Url = request.Url
                  };

                  _User.SocialLinks.Add(link);
                  return await Task.FromResult(link);
            }

            public async Task<bool> DeleteSocialLinkAsync(Guid id)
            {
                  var removed = _User.SocialLinks.RemoveAll(link => link.Id == id) > 0;
                  return await Task.FromResult(removed);
            }
      }
}