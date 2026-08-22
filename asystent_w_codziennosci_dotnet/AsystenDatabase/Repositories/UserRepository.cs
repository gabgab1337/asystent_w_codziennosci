using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using System.Security.Cryptography;
using System.Text;

namespace AssistantDatabase.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;

        public UserRepository(DataContext context)
        {
            this.context = context;
        }

        public void Add(UserDM user)
        {
            user.Password = HashPassword(user.Username);
            user.JoiningDate = DateTime.Now;
            context.Users.Add(user);
            context.SaveChanges();
        }
        public UserDM? IsLogIn(string userName, string password)
        {
            UserDM? user = context.Users.FirstOrDefault(u => u.Username == userName && u.Password == HashPassword(password));
            return user;
        }

        public UserDM GetById(int id)
        {
            return context.Users.Find(id);
        }
        public UserDM GetProtegeById(int? id)
        {
            return context.Users.Find(id);
        }
        public UserType GetUserType(int userId)
        {
            return context.Users.Find(userId).Type;
        }

        public void Update(UserDM user)
        {
            context.Users.Update(user);
            context.SaveChanges();
        }
        public List<UserDM> ReadAllUsers()
        {
            return context.Users.ToList();
        }
        public List<UserDM> ReadAllForAdmin()
        {
            return context.Users.Where(
                u => u.Type == UserType.administrator 
                || u.Type == UserType.caregiver
                ).ToList();
        }

        public List<UserDM> ReadAllForCaregiver(int caregiverId)
        {
            return context.Users.Where(
                u => u.Type == UserType.asdPerson 
                && u.CaregiverId.HasValue 
                && u.CaregiverId.Value == caregiverId
                ).ToList();
        }
        public bool ExistLogin(string username)
        {
            UserDM? user = context.Users.FirstOrDefault(u => u.Username == username);
            return user != null;
        }
        
        public void DeleteAsdPerson(UserDM asd)
        {
            var tasks = context.Tasks.Where(t => t.AsdPerson == asd).ToList();
            context.Tasks.RemoveRange(tasks);
            context.Users.Remove(asd);
            context.SaveChanges();
        }
        
        public bool DeleteCaregiver(UserDM caregiver)
        {
            var tasks = context.Tasks.Where(t => t.Caregiver == caregiver).ToList();
            context.Tasks.RemoveRange(tasks);

            var asdList = ReadAllForCaregiver(caregiver.Id);
            context.Users.RemoveRange(asdList);
            context.Users.Remove(caregiver);
            context.SaveChanges();
            return true;
        }

        public bool DeleteAdmin(UserDM admin)
        {
            if (admin != null)
            {
                context.Users.Remove(admin);
                context.SaveChanges();
            }
            return true;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        
        public bool VerifyPassword(string password, string hashedPassword)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                string hashedInputPassword = builder.ToString();

                return hashedPassword == hashedInputPassword;
            }
        }

        public void SetColorsPalete(int userId, string colorPaleteName)
        {
            var user = context.Users.Find(userId);
            if(user != null)
            {
                user.ColorsPalete = colorPaleteName;
                context.SaveChanges();
            }
        }
    }
}
