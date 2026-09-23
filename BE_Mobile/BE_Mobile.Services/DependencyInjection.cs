using BE_Mobile.Data;
using BE_Mobile.Repositories.Implementations;
using BE_Mobile.Repositories.Interfaces;
using BE_Mobile.Services.Implementations;
using BE_Mobile.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BE_Mobile.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IAdminStudentRepository, AdminStudentRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminSectionManagementRepository, AdminSectionManagementRepository>();
        services.AddScoped<IAdminSectionManagementService, AdminSectionManagementService>();
        services.AddScoped<IAdminCourseRepository, AdminCourseRepository>();
        services.AddScoped<IAdminCourseService, AdminCourseService>();
        services.AddScoped<IAdminSemesterRepository, AdminSemesterRepository>();
        services.AddScoped<IAdminSemesterService, AdminSemesterService>();
        services.AddScoped<IAdminCourseSectionRepository, AdminCourseSectionRepository>();
        services.AddScoped<IAdminCourseSectionService, AdminCourseSectionService>();
        services.AddScoped<IAdminTeacherRepository, AdminTeacherRepository>();
        services.AddScoped<IAdminTeacherService, AdminTeacherService>();
        services.AddScoped<IAdminDepartmentRepository, AdminDepartmentRepository>();
        services.AddScoped<IAdminDepartmentService, AdminDepartmentService>();
        services.AddScoped<IAdminMajorRepository, AdminMajorRepository>();
        services.AddScoped<IAdminMajorService, AdminMajorService>();
        services.AddScoped<IAdminAcademicClassRepository, AdminAcademicClassRepository>();
        services.AddScoped<IAdminAcademicClassService, AdminAcademicClassService>();
        services.AddScoped<IStudentLearningRepository, StudentLearningRepository>();
        services.AddScoped<ITeacherSectionRepository, TeacherSectionRepository>();

        services.AddScoped<IAdminStudentService, AdminStudentService>();
        services.AddScoped<IStudentLearningService, StudentLearningService>();
        services.AddScoped<ITeacherSectionService, TeacherSectionService>();

        return services;
    }
}
