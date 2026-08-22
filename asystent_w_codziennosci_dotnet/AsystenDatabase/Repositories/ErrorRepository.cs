using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistantDatabase.Repositories
{
    public class ErrorRepository : IErrorRepository
    {
        private readonly DataContext context;

        public ErrorRepository(DataContext context)
        {
            this.context = context;
        }


        public void Create(ErrorDM error)
        {
            context.Errors.Add(error);
            context.SaveChanges();
        }
    }
}
