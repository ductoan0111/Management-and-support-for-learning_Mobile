export type UserRole = "student" | "teacher" | "admin";

export type AuthUser = {
  userId: number;
  roleId: number;
  roleCode: string;
  roleName: string;
  studentId: number | null;
  teacherId: number | null;
  identifier: string;
  fullName: string;
  email: string;
  phone?: string | null;
  avatarUrl?: string | null;
  token: string;
};

// Global in-memory session
let currentAuthUser: AuthUser | null = null;

export const authSession = {
  setUser(user: AuthUser | null) {
    currentAuthUser = user;
  },

  getUser(): AuthUser | null {
    return currentAuthUser;
  },

  getRole(): UserRole {
    if (!currentAuthUser) return "student";
    const code = currentAuthUser.roleCode.toUpperCase();
    if (code.includes("TEACH") || code.includes("GV")) return "teacher";
    if (code.includes("ADMIN")) return "admin";
    return "student";
  },

  getToken(): string | null {
    return currentAuthUser?.token ?? null;
  },

  isAuthenticated(): boolean {
    return currentAuthUser !== null;
  },

  logout() {
    currentAuthUser = null;
  },
};
