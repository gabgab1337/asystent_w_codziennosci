using AssistantDatabase.Model;

namespace Assistant.Api.Authentication
{
    /// <summary>
    /// Role claim values. They are the <see cref="UserType"/> names, which is what the
    /// login endpoint writes into the token.
    /// </summary>
    public static class Roles
    {
        public const string Administrator = nameof(UserType.administrator);

        public const string Caregiver = nameof(UserType.caregiver);

        public const string AsdPerson = nameof(UserType.asdPerson);
    }
}
