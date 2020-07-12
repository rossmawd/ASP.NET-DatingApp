namespace DatingApp.API.Data
{
    public interface IAuthRepository
    {
      Task<User> Register(User user, string password)
         
    }
}