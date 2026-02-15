using Azure;
using Baseqat.EF.Models.Auth;
using Baseqat.EF.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Baseqat.EF.DATA
{
    public interface IDataUnit : IDisposable
    {
        IBaseRepository<Privileges> Privileges { get; }
        IBaseRepository<Privileges_RoleBased> Privileges_RoleBased { get; }
        IBaseRepository<Privileges_UserBased> Privileges_UserBased { get; }

        int Complete();
        Task<int> CompleteAsync();
    }
}
