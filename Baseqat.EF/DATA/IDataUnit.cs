using Azure;
using Baseqat.EF.Models;
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
        // Privileges
        IBaseRepository<Privileges> Privileges { get; }
        IBaseRepository<Privileges_RoleBased> Privileges_RoleBased { get; }
        IBaseRepository<Privileges_UserBased> Privileges_UserBased { get; }

        // Course Management
        IBaseRepository<CourseCategory> CourseCategory { get; set; }
        IBaseRepository<Course> Course { get; }
        IBaseRepository<CourseSection> CourseSection { get; }
        IBaseRepository<CourseLesson> CourseLesson { get; }
        IBaseRepository<CourseEnrollment> CourseEnrollment { get; }
        IBaseRepository<CourseInstructor> CourseInstructor { get; }
        IBaseRepository<CourseRequirement> CourseRequirement { get; }
        IBaseRepository<CourseReview> CourseReview { get; }

        // Instructor Management
        IBaseRepository<Instructor> Instructor { get; }
        IBaseRepository<InstructorSkill> InstructorSkill { get; }
        IBaseRepository<StudentReview> StudentReview { get; }
        IBaseRepository<ContactRequest> ContactRequest { get; }

        int Complete();
        Task<int> CompleteAsync();
    }
}
