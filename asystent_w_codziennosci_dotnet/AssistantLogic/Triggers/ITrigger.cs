using AssistantLogic.Model;
using AssistantLogic.ViewModel;

namespace AssistantLogic.Triggers
{
    public interface ITrigger
    {
        TriggerAddEditVM VM
        {
            get;
        }
        
        string Description
        {
            get;
        }

        string ShortDescription
        {
            get;
        }

        string SubTypeName
        {
            get;
        }
    
        bool IsRun(PlanDayParameters parameters);
        
        bool Negation { get; }

    }
}
