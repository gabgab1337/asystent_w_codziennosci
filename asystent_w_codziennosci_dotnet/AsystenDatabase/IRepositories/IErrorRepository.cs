using AssistantDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantDatabase.IRepositories
{
    public interface IErrorRepository
    {
        void Create(ErrorDM error);
    }
}
