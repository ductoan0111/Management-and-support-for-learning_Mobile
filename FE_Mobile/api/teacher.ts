import { authSession } from "@/features/auth/authSession";
import { request } from "./client";

export type Row = Record<string, string | number | boolean | null>;
export type Section = Row & { sectionId: number; sectionCode: string; courseName: string; semesterName: string; academicYear: string; enrolledCount: number; status: number };
export type Schedule = Row & { scheduleId: number; sectionId: number; courseName: string; dayOfWeek: number; startTime: string; endTime: string; effectiveFrom: string; effectiveTo: string };

export function teacherRequest<T>(path: string, method = "GET", body?: unknown) {
  const user = authSession.getUser();
  if (!user?.teacherId || user.roleCode.toUpperCase() !== "TEACHER") return Promise.reject(new Error("Vui lòng đăng nhập bằng tài khoản giảng viên."));
  return request<T>(`/api/teachers/${user.teacherId}${path}`, method, body);
}
export const getSections = () => teacherRequest<Section[]>("/sections");
export const getSchedule = () => teacherRequest<Schedule[]>("/schedule");
