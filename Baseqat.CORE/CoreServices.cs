using Baseqat.CORE.Helpers;
using Baseqat.CORE.Services;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Baseqat.CORE
{
    public static class CoreServices
    {
        public static IServiceCollection
           AddCoreServices(this IServiceCollection services
           , IConfiguration configuration)
        {

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddTransient<IDataUnit, UnitOfWork>();
            services.AddScoped<UsersHelper>();

            //Add Identity Services with custom ApplicationUser  
            //when you need to use AddIdentity must install Microsoft.AspNetCore.Identity package
            // in CORE project and also add reference to EF.DATA project
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            

            //Add DbContext from Baseqt.EF.DATA
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration
                    .GetConnectionString("AppCon"),
                    b => b.MigrationsAssembly
                    (typeof(AppDbContext).Assembly.FullName));
            });


            return services;
        }

        public static async Task SeedDefaultUserAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var supperRole = IntitalRoles.SuperAdmin;

            // تأكد أولاً من وجود الدور في قاعدة البيانات
            if (!await roleManager.RoleExistsAsync(supperRole))
            {
                await roleManager.CreateAsync(new IdentityRole(supperRole));
            }

            var defaultUser = IntitalUsers.SuperUser;
            var user = await userManager.FindByEmailAsync(defaultUser.Email);

            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "Admin@2030");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, supperRole);
                }
            }
        }

        public static async Task SeedDefaultPrivilegesAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. تعريف الصلاحيات للأقسام الجديدة (مدرب، مستشار، عميل، موظفين)
            var lst = new List<Privileges>
            {
                // إدارة النظام
                new Privileges { priv_name = "ادارة المستخدمين", isEnabled = true, priv_cat = "System", priv_key = Guid.NewGuid() },
                new Privileges { priv_name = "ادارة المجموعات", isEnabled = true, priv_cat = "System", priv_key = Guid.NewGuid() },
        
                // صلاحيات خاصة بالمدربين والمستشارين
                new Privileges { priv_name = "إدارة الدورات التدريبية", isEnabled = true, priv_cat = "Trainer", priv_key = Guid.NewGuid() },
                new Privileges { priv_name = "تقديم الاستشارات", isEnabled = true, priv_cat = "Consultant", priv_key = Guid.NewGuid() },
        
                // صلاحيات العملاء والموظفين
                new Privileges { priv_name = "طلب خدمة جديدة", isEnabled = true, priv_cat = "Client", priv_key = Guid.NewGuid() },
                new Privileges { priv_name = "متابعة طلبات العملاء", isEnabled = true, priv_cat = "BaseqatEmployee", priv_key = Guid.NewGuid() },
                new Privileges { priv_name = "إصدار الشهادات والتقارير", isEnabled = true, priv_cat = "Reports", priv_key = Guid.NewGuid() }
            };

            // 2. فحص وإضافة الصلاحيات في جدول Privileges
            var flst = db.Privileges.ToList();
            foreach (var field in lst)
            {
                var isex = flst.FirstOrDefault(a => a.priv_name == field.priv_name);
                if (isex == null)
                {
                    db.Privileges.Add(field);
                    await db.SaveChangesAsync();
                }
            }

            // 3. توزيع الصلاحيات على الأدوار باستخدام المنطق القديم (التعيين المباشر)
            var roles = roleManager.Roles.ToList();
            var lsrpv = db.Privileges.ToList();

            foreach (var r in roles)
            {
                foreach (var pv in lsrpv)
                {
                    var isExpv = db.Privileges_RoleBased.FirstOrDefault(a => a.PrivilegesId == pv.Id && a.RoleId == r.Id);

                    if (isExpv == null)
                    {
                        var nw = new Privileges_RoleBased { RoleId = r.Id, PrivilegesId = pv.Id };

                        // تحديد من هو الـ SuperAdmin للتحكم الكامل
                        var isSuper = (r.Name == IntitalRoles.SuperAdmin || r.Name == IntitalRoles.Admin);

                        // تطبيق المنطق القديم في التعيين
                        nw.is_displayed = true;
                        nw.is_insert = isSuper ? true : false;
                        nw.is_update = isSuper ? true : false;
                        nw.is_delete = isSuper ? true : false;
                        nw.is_print = isSuper ? true : false;

                        // إضافات مخصصة للأدوار الأخرى إذا أردت (اختياري)
                        if (!isSuper)
                        {
                            if (r.Name == IntitalRoles.BaseqatEmployee && (pv.priv_cat == "Reports" || pv.priv_cat == "Client"))
                            {
                                nw.is_insert = true;
                                nw.is_update = true;
                                nw.is_print = true;
                            }
                            else if (r.Name == pv.priv_cat) // إذا كان اسم الدور يطابق قسم الصلاحية (مدرب أو مستشار)
                            {
                                nw.is_insert = true;
                                nw.is_update = true;
                            }
                        }

                        db.Privileges_RoleBased.Add(nw);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
