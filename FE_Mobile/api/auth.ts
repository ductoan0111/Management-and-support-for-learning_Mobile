import { request } from "./client";
import { authSession, type AuthUser, type UserRole } from "@/features/auth/authSession";
export { getBaseUrl } from "./client";

export type LoginCredentials = { identifier: string; password: string; role?: UserRole };
export type LoginResult = { success: boolean; user?: AuthUser; message?: string };

export async function loginApi(credentials: LoginCredentials): Promise<LoginResult> {
  try {
    const user = await request<AuthUser>("/api/auth/login", "POST", { ...credentials, identifier: credentials.identifier.trim() });
    if (credentials.role === "teacher" && (user.roleCode.toUpperCase() !== "TEACHER" || !user.teacherId)) {
      return { success: false, message: "Tài khoản chưa có hồ sơ giảng viên hoặc không có quyền giảng viên." };
    }
    authSession.setUser(user);
    return { success: true, user };
  } catch (error) { return { success: false, message: error instanceof Error ? error.message : "Đăng nhập thất bại." }; }
}

export type RegisterStudentParams = { fullName: string; studentCode: string; email: string; phone?: string; password: string };
export async function registerStudentApi(params: RegisterStudentParams): Promise<LoginResult> {
  try {
    const user = await request<AuthUser>("/api/auth/register-student", "POST", {
      ...params, username: params.studentCode.trim().toLowerCase(), studentCode: params.studentCode.trim(),
      email: params.email.trim(), fullName: params.fullName.trim(), phone: params.phone?.trim() || null,
      majorId: 1, enrollmentYear: new Date().getFullYear(),
    });
    authSession.setUser(user);
    return { success: true, user };
  } catch (error) { return { success: false, message: error instanceof Error ? error.message : "Đăng ký thất bại." }; }
}
