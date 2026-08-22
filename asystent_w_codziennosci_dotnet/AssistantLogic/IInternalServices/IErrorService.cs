using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantLogic.IInternalServices
{
    public interface IErrorService
    {
        void LogError(string errorMessage);
    }
}
