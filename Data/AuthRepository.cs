using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class AuthRepository : IAuthRepository
  {
    // this is a 'field', it can use the _context syntax in conjunctin within the 
    // contructor below because it is a private field
    // it gives us access to our database (through an instance of DbContext) within this repository
    private readonly DataContext _context;

    public AuthRepository(DataContext context)
    {
      _context = context;
    }
    public async Task<User> Login(string username, string password)
    {
      var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
      //returns null if no user found, instead of throwing an exception like _context.Users.FirstAsync would
      if (user == null)
        return null;

      if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return null;

      return user; 

    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
      {

        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); 
        //now this same method is making a hash from the password entered by the user and the salt from the database
        for (int i = 0; i < computedHash.Length; i++)
        {
          if (computedHash[i] != passwordHash[i])
            return false;
        }
        return true;
      }
    }

    public async Task<User> Register(User user, string password)
    {
      byte[] passwordHash, passwordSalt; //declaring the two byte arrays
      createPasswordHash(password, out passwordHash, out passwordSalt); // the out keyword passes a reference instead of a value...so if they're updated elsewhere they'll also be updated here
      user.PasswordHash = passwordHash;
      user.PasswordSalt = passwordSalt;

      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync(); //saves the changes to the database

      return user;
    }

    private void createPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {

      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); //what's inside the brackets changes the password to a byte array.
      }
    }

    public async Task<bool> UserExists(string username)
    {
      if(await _context.Users.AnyAsync(x => x.Username == username))
        return true; 

      return false; 
    }
  }
}