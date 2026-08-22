using System.ComponentModel;

namespace AssistantDatabase.Model
{
    public enum UserType
    {
        [Description("Administrator")]
        administrator,

        [Description("Opiekun")]
        caregiver,

        [Description("Osoba z ASD")]
        asdPerson
    }
}
