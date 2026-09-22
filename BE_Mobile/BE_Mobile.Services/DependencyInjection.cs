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

        // Auth
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();

        // Admin
        services.AddScoped<IAdminStudentRepository, AdminStudentRepository>();
        services.AddScoped<IAdminStudentService, AdminStudentService>();

        // Student
        services.AddScoped<IStudentLearningRepository, StudentLearningRepository>();
        services.AddScoped<IStudentLearningService, StudentLearningService>();

        // Teacher: 6 nhóm chức năng riêng biệt
        // 1. Lớp học & Lịch dạy & Hồ sơ
        services.AddScoped<ITeacherSectionRepository, TeacherSectionRepository>();
        services.AddScoped<ITeacherSectionService, TeacherSectionService>();

        // 2. Bài tập & Chấm bài
        services.AddScoped<ITeacherAssignmentRepository, TeacherAssignmentRepository>();
        services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();

        // 3. Tài liệu
        services.AddScoped<ITeacherMaterialRepository, TeacherMaterialRepository>();
        services.AddScoped<ITeacherMaterialService, TeacherMaterialService>();

        // 4. Điểm
        services.AddScoped<ITeacherGradeRepository, TeacherGradeRepository>();
        services.AddScoped<ITeacherGradeService, TeacherGradeService>();

        // 5. Thông báo
        services.AddScoped<ITeacherAnnouncementRepository, TeacherAnnouncementRepository>();
        services.AddScoped<ITeacherAnnouncementService, TeacherAnnouncementService>();

        // 6. Sinh viên trong lớp
        services.AddScoped<ITeacherStudentRepository, TeacherStudentRepository>();
        services.AddScoped<ITeacherStudentService, TeacherStudentService>();

        return services;
    }
}
