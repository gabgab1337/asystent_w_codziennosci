using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantLogic.InternalServices
{
    public class ErrorService : IErrorService
    {
        private IErrorRepository errorRepository;

        public ErrorService(IErrorRepository errorRepository)
        {
            this.errorRepository = errorRepository;
        }

        public void LogError(string errorMessage)
        {
            ErrorDM error = new ErrorDM();
            error.DateTime = DateTime.Now;
            error.Message = errorMessage;
            errorRepository.Create(error);
        }
    }
}
