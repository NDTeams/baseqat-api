using Baseqat.EF.Models.Auth;
using Baseqat.EF.Repositories;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;

namespace Baseqat.EF.DATA
{
    public class UnitOfWork : IDataUnit
    {
        private readonly AppDbContext _context;


        // Privileges repositories (added)
        public IBaseRepository<Privileges> Privileges { get; private set; }
        public IBaseRepository<Privileges_RoleBased> Privileges_RoleBased { get; private set; }
        public IBaseRepository<Privileges_UserBased> Privileges_UserBased { get; private set; }

        //Courses 
        public IBaseRepository<CourseCategory> CourseCategory { get; private set; }





        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            // initialize privileges repositories
            Privileges = new BaseRepository<Privileges>(_context);
            Privileges_RoleBased = new BaseRepository<Privileges_RoleBased>(_context);
            Privileges_UserBased = new BaseRepository<Privileges_UserBased>(_context);

            //Courses 
            CourseCategory = new BaseRepository<CourseCategory>(_context);

        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
