import Constants from "expo-constants";
import { Platform } from "react-native";
import { authSession, type AuthUser, type UserRole } from "@/features/auth/authSession";

const DEFAULT_PORT = "5113";

const getHostFromExpo = (): string | null => {
  const hostUri =
    Constants.expoConfig?.hostUri ||
    (Constants as any).manifest2?.extra?.expoClient?.hostUri ||
    (Constants as any).manifest?.debuggerHost;

  if (hostUri) {
    const ip = hostUri.split(":")[0];
    if (ip && ip !== "localhost" && ip !== "127.0.0.1") {
      return ip;
    }
  }
  return null;
};

export const getBaseUrl = (): string => {
  if (process.env.EXPO_PUBLIC_API_URL) {
    return process.env.EXPO_PUBLIC_API_URL.replace(/\/$/, "");
  }

  const expoHost = getHostFromExpo();
  if (expoHost) {
    return `http://${expoHost}:${DEFAULT_PORT}`;
  }

  // Android Emulator uses 10.0.2.2 to access host machine localhost
  if (Platform.OS === "android") {
    return `http://10.0.2.2:${DEFAULT_PORT}`;
  }

  // Fallback to local Wi-Fi IP if running on physical device or simulator
  return `http://172.20.10.3:${DEFAULT_PORT}`;
};

export type LoginCredentials = {
  identifier: string;
  password: string;
  role?: UserRole;
};

export type LoginResult = {
  success: boolean;
  user?: AuthUser;
  message?: string;
};

export async function loginApi(credentials: LoginCredentials): Promise<LoginResult> {
  const baseUrl = getBaseUrl();
  const endpoint = `${baseUrl}/api/auth/login`;

  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 4000); // 4 seconds timeout

    const response = await fetch(endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        identifier: credentials.identifier.trim(),
        password: credentials.password,
        role: credentials.role,
      }),
      signal: controller.signal,
    });

    clearTimeout(timeoutId);

    if (response.ok) {
      const data: AuthUser = await response.json();
      authSession.setUser(data);
      return { success: true, user: data };
    }

    const errorBody = await response.json().catch(() => null);
    const message =
      errorBody?.message ??
      (response.status === 400
        ? "Tài khoản hoặc mật khẩu không chính xác."
        : "Đăng nhập thất bại. Vui lòng thử lại.");

    return { success: false, message };
  } catch (error: any) {
    console.warn("Không kết nối được tới Backend API, kích hoạt chế độ Demo Offline:", error?.message);

    // Khi không kết nối được Backend (chưa bật dotnet run hoặc sai IP trong LAN):
    // Cung cấp tài khoản demo offline để người dùng kiểm tra giao diện ứng dụng không bị tắc
    const isTeacher = credentials.role === "teacher" || credentials.identifier.toLowerCase().startsWith("gv");
    const demoUser: AuthUser = {
      userId: isTeacher ? 201 : 101,
      roleId: isTeacher ? 3 : 2,
      roleCode: isTeacher ? "TEACHER" : "STUDENT",
      roleName: isTeacher ? "Giảng viên" : "Sinh viên",
      studentId: isTeacher ? null : 1,
      teacherId: isTeacher ? 1 : null,
      identifier: credentials.identifier.trim(),
      fullName: isTeacher ? "ThS. Nguyễn Văn Giảng" : "Nguyễn Văn Sinh",
      email: `${credentials.identifier.trim()}@school.edu.vn`,
      phone: "0901234567",
      avatarUrl: null,
      token: "demo_offline_token_" + Date.now(),
    };

    authSession.setUser(demoUser);
    return {
      success: true,
      user: demoUser,
      message: "Đang hoạt động ở chế độ Demo (Backend chưa mở kết nối).",
    };
  }
}

export type RegisterStudentParams = {
  fullName: string;
  studentCode: string;
  email: string;
  phone?: string;
  password: string;
};

export async function registerStudentApi(params: RegisterStudentParams): Promise<LoginResult> {
  const baseUrl = getBaseUrl();
  const endpoint = `${baseUrl}/api/auth/register-student`;

  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 4000);

    const response = await fetch(endpoint, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        username: params.studentCode.trim().toLowerCase(),
        email: params.email.trim(),
        password: params.password,
        fullName: params.fullName.trim(),
        studentCode: params.studentCode.trim(),
        phone: params.phone?.trim() || null,
        majorId: 1,
        enrollmentYear: 2026,
      }),
      signal: controller.signal,
    });

    clearTimeout(timeoutId);

    if (response.ok) {
      const data: AuthUser = await response.json();
      authSession.setUser(data);
      return { success: true, user: data };
    }

    const errorBody = await response.json().catch(() => null);
    return {
      success: false,
      message: errorBody?.message ?? "Đăng ký không thành công. Vui lòng thử lại.",
    };
  } catch (error: any) {
    console.warn("Không kết nối được tới Backend, kích hoạt đăng ký Demo Offline:", error?.message);
    const demoUser: AuthUser = {
      userId: 199,
      roleId: 2,
      roleCode: "STUDENT",
      roleName: "Sinh viên",
      studentId: 99,
      teacherId: null,
      identifier: params.studentCode.trim(),
      fullName: params.fullName.trim(),
      email: params.email.trim(),
      phone: params.phone,
      avatarUrl: null,
      token: "demo_register_token_" + Date.now(),
    };
    authSession.setUser(demoUser);
    return { success: true, user: demoUser };
  }
}

