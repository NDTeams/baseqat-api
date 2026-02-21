using Baseqat.EF.Models.Auth;
using Baseqat.EF.Repositories;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;

namespace Baseqat.EF.DATA
{
    public class UnitOfWork : IDataUnit
    {
        private readonly AppDbContext _context;

        // Privileges repositories
        public IBaseRepository<Privileges> Privileges { get; private set; }
        public IBaseRepository<Privileges_RoleBased> Privileges_RoleBased { get; private set; }
        public IBaseRepository<Privileges_UserBased> Privileges_UserBased { get; private set; }

        // Course Management
        public IBaseRepository<CourseCategory> CourseCategory { get; set; }
        public IBaseRepository<Course> Course { get; private set; }
        public IBaseRepository<CourseSection> CourseSection { get; private set; }
        public IBaseRepository<CourseLesson> CourseLesson { get; private set; }
        public IBaseRepository<CourseEnrollment> CourseEnrollment { get; private set; }
        public IBaseRepository<CourseInstructor> CourseInstructor { get; private set; }
        public IBaseRepository<CourseRequirement> CourseRequirement { get; private set; }
        public IBaseRepository<CourseReview> CourseReview { get; private set; }

        // Instructor Management
        public IBaseRepository<Instructor> Instructor { get; private set; }
        public IBaseRepository<InstructorSkill> InstructorSkill { get; private set; }
        public IBaseRepository<StudentReview> StudentReview { get; private set; }
        public IBaseRepository<ContactRequest> ContactRequest { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            // Initialize privileges repositories
            Privileges = new BaseRepository<Privileges>(_context);
            Privileges_RoleBased = new BaseRepository<Privileges_RoleBased>(_context);
            Privileges_UserBased = new BaseRepository<Privileges_UserBased>(_context);

            // Initialize course management repositories
            CourseCategory = new BaseRepository<CourseCategory>(_context);
            Course = new BaseRepository<Course>(_context);
            CourseSection = new BaseRepository<CourseSection>(_context);
            CourseLesson = new BaseRepository<CourseLesson>(_context);
            CourseEnrollment = new BaseRepository<CourseEnrollment>(_context);
            CourseInstructor = new BaseRepository<CourseInstructor>(_context);
            CourseRequirement = new BaseRepository<CourseRequirement>(_context);
            CourseReview = new BaseRepository<CourseReview>(_context);

            // Initialize instructor management repositories
            Instructor = new BaseRepository<Instructor>(_context);
            InstructorSkill = new BaseRepository<InstructorSkill>(_context);
            StudentReview = new BaseRepository<StudentReview>(_context);
            ContactRequest = new BaseRepository<ContactRequest>(_context);
        }

        public UnitOfWork()
        {
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
