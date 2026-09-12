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
        services.AddScoped<IStudentLearningRepository, StudentLearningRepository>();
        services.AddScoped<ITeacherSectionRepository, TeacherSectionRepository>();

        services.AddScoped<IAdminStudentService, AdminStudentService>();
        services.AddScoped<IStudentLearningService, StudentLearningService>();
        services.AddScoped<ITeacherSectionService, TeacherSectionService>();

        return services;
    }
}
