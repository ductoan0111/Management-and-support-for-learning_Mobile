import { authSession } from "@/features/auth/authSession";
import { request } from "./client";

export type StudentMaterial = {
  materialId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  title: string;
  description: string | null;
  materialType: string | null;
  fileUrl: string | null;
  externalUrl: string | null;
};

export function studentRequest<T>(path: string): Promise<T> {
  const user = authSession.getUser();
  if (!user?.studentId || user.roleCode.toUpperCase() !== "STUDENT") {
    return Promise.reject(new Error("Vui lòng đăng nhập bằng tài khoản sinh viên."));
  }
  return request<T>(`/api/students/${user.studentId}${path}`);
}
