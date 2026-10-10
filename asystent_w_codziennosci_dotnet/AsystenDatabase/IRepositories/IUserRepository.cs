using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface IUserRepository
    {
        void Add(UserDM user);
        UserDM? IsLogIn(string userName, string password);
        UserDM GetById(int id);
        UserDM GetProtegeById(int? id);
        UserType GetUserType(int userId);
        void Update(UserDM user);

        /// <summary>
        /// Replaces <paramref name="user"/>.Password with the SHA-256 hash <see cref="Add"/> stores.
        /// Does not save; persist with <see cref="Update"/>.
        /// </summary>
        void AssignHashedPassword(UserDM user, string password);

        void DeleteAsdPerson(UserDM asd);
        bool ExistLogin(string username);
        bool DeleteCaregiver(UserDM caregiver);
        bool DeleteAdmin(UserDM caregiver);
        List<UserDM> ReadAllUsers();
        List<UserDM> ReadAllForAdmin();
        List<UserDM> ReadAllForCaregiver(int caregiverId);
        void SetColorsPalete(int userId, string colorPaleteName);
    }
}