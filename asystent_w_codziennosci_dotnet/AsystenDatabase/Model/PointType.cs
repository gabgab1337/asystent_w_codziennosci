using System.ComponentModel;

namespace AssistantDatabase.Model
{
    public enum PointType
    {
        [Description("Standardowe punkt")]
        standard,

        [Description("Zalezne od pogody")]
        dressingUp,

        [Description("Gotowanie")]
        asdPerson
    }
}
