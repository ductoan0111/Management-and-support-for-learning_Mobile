import { authSession } from "@/features/auth/authSession";
import { request } from "./client";

type QueryValue = string | number | boolean | null | undefined;

function withQuery(path: string, params?: Record<string, QueryValue>) {
  if (!params) return path;
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== null && value !== undefined && value !== "") {
      search.set(key, String(value));
    }
  }
  const query = search.toString();
  return query ? `${path}?${query}` : path;
}

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type StudentProfile = {
  studentId: number;
  userId: number;
  studentCode: string;
  username: string;
  fullName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string | null;
  gender: number | null;
  avatarUrl: string | null;
  isActive: boolean;
  academicClassId: number | null;
  classCode: string | null;
  className: string | null;
  majorId: number;
  majorCode: string;
  majorName: string;
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  enrollmentYear: number;
  status: number;
};

export type UpdateStudentProfileParams = {
  fullName: string;
  phone: string | null;
  dateOfBirth: string | null;
  gender: number | null;
  avatarUrl: string | null;
};

export type StudentDashboard = {
  student: StudentDashboardProfile;
  deadlines: StudentAssignmentDeadline[];
  exams: StudentDashboardExam[];
};

export type StudentDashboardProfile = {
  studentId: number;
  studentCode: string;
  fullName: string;
  email: string;
  majorName: string;
  className: string | null;
  gpa: number | null;
};

export type StudentAssignmentDeadline = {
  studentId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  assignmentId: number;
  title: string;
  dueAt: string;
  maxScore: number;
  submissionStatus: string;
  submittedAt: string | null;
  score: number | null;
};

export type StudentDashboardExam = {
  studentId: number;
  courseCode: string;
  courseName: string;
  examId: number;
  examName: string;
  examType: number;
  examDate: string;
  startTime: string;
  durationMinutes: number;
  room: string | null;
};

export type StudentSection = {
  studentId: number;
  enrollmentId: number;
  sectionId: number;
  sectionCode: string;
  sectionName: string | null;
  courseId: number;
  courseCode: string;
  courseName: string;
  credits: number;
  semesterId: number;
  semesterCode: string;
  semesterName: string;
  academicYear: string;
  enrollmentStatus: number;
  finalScore10: number | null;
  letterGrade: string | null;
};

export type StudentSchedule = {
  studentId: number;
  sectionId: number;
  sectionCode: string;
  courseCode: string;
  courseName: string;
  scheduleId: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  room: string | null;
  building: string | null;
  effectiveFrom: string;
  effectiveTo: string;
  note: string | null;
};

export type StudentExamSchedule = {
  studentId: number;
  sectionId: number;
  sectionCode: string;
  courseCode: string;
  courseName: string;
  examId: number;
  examName: string;
  examType: number;
  examDate: string;
  startTime: string;
  durationMinutes: number;
  room: string | null;
  note: string | null;
  createdAt: string;
};

export type StudentAssignment = {
  studentId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  assignmentId: number;
  title: string;
  description: string | null;
  attachmentUrl: string | null;
  openAt: string | null;
  dueAt: string;
  maxScore: number;
  allowLateSubmission: boolean;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string | null;
  submissionId: number | null;
  textContent: string | null;
  fileUrl: string | null;
  submittedAt: string | null;
  isLate: boolean | null;
  submissionRawStatus: number | null;
  submissionStatus: string;
  score: number | null;
  feedback: string | null;
  gradedAt: string | null;
};

export type SubmitAssignmentParams = {
  textContent: string | null;
  fileUrl: string | null;
};

export type AssignmentSubmission = {
  submissionId: number;
  assignmentId: number;
  studentId: number;
  textContent: string | null;
  fileUrl: string | null;
  submittedAt: string;
  isLate: boolean;
  status: number;
  score: number | null;
  feedback: string | null;
  gradedByUserId: number | null;
  gradedAt: string | null;
};

export type StudentSubmission = {
  submissionId: number;
  assignmentId: number;
  assignmentTitle: string;
  sectionId: number;
  courseCode: string;
  courseName: string;
  dueAt: string;
  textContent: string | null;
  fileUrl: string | null;
  submittedAt: string;
  isLate: boolean;
  status: number;
  score: number | null;
  feedback: string | null;
  gradedAt: string | null;
};

export type StudentMaterial = {
  materialId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  uploadedByUserId: number;
  uploadedByFullName: string;
  title: string;
  description: string | null;
  materialType: string | null;
  fileUrl: string | null;
  externalUrl: string | null;
  isVisible: boolean;
  createdAt: string;
  updatedAt: string | null;
};

export type StudentGrade = {
  gradeComponentId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  componentName: string;
  weightPercent: number;
  maxScore: number;
  displayOrder: number;
  studentGradeId: number | null;
  score: number | null;
  note: string | null;
  gradedByUserId: number | null;
  gradedAt: string | null;
  updatedAt: string | null;
};

export type StudentSectionScore = {
  studentId: number;
  sectionId: number;
  courseCode: string;
  courseName: string;
  credits: number;
  weightedScore10: number;
  gpa4: number | null;
};

export type StudentGpa = {
  studentId: number;
  gpa: number | null;
};

export type StudyGoal = {
  goalId: number;
  studentId: number;
  title: string;
  description: string | null;
  goalType: number;
  targetValue: number | null;
  currentValue: number | null;
  startDate: string;
  endDate: string | null;
  status: number;
  createdAt: string;
  updatedAt: string | null;
};

export type StudyGoalWriteParams = {
  title: string;
  description: string | null;
  goalType: number;
  targetValue: number | null;
  currentValue: number | null;
  startDate: string;
  endDate: string | null;
  status: number;
};

export type StudyTask = {
  studyTaskId: number;
  studentId: number;
  courseId: number | null;
  courseCode: string | null;
  courseName: string | null;
  title: string;
  description: string | null;
  startAt: string | null;
  dueAt: string | null;
  reminderAt: string | null;
  priority: number;
  status: number;
  createdAt: string;
  updatedAt: string | null;
  completedAt: string | null;
};

export type StudyTaskWriteParams = {
  courseId: number | null;
  title: string;
  description: string | null;
  startAt: string | null;
  dueAt: string | null;
  reminderAt: string | null;
  priority: number;
  status: number;
};

export type StudentAnnouncement = {
  announcementId: number;
  createdByUserId: number;
  createdByFullName: string;
  sectionId: number | null;
  sectionCode: string | null;
  courseCode: string | null;
  courseName: string | null;
  title: string;
  content: string;
  announcementType: number;
  publishedAt: string;
  expiresAt: string | null;
  isActive: boolean;
  isRead: boolean;
  readAt: string | null;
};

export function studentRequest<T>(path: string, method = "GET", body?: unknown): Promise<T> {
  const user = authSession.getUser();
  if (!user?.studentId || user.roleCode.toUpperCase() !== "STUDENT") {
    return Promise.reject(new Error("Vui lòng đăng nhập bằng tài khoản sinh viên."));
  }
  return request<T>(`/api/students/${user.studentId}${path}`, method, body);
}

export const getStudentProfile = () => studentRequest<StudentProfile>("/profile");
export const updateStudentProfile = (params: UpdateStudentProfileParams) => studentRequest<StudentProfile>("/profile", "PUT", params);
export const getStudentDashboard = () => studentRequest<StudentDashboard>("/dashboard");
export const getStudentSections = (params?: { semesterId?: number; status?: number }) => studentRequest<StudentSection[]>(withQuery("/sections", params));
export const getStudentSchedule = (params?: { from?: string; to?: string; sectionId?: number }) => studentRequest<StudentSchedule[]>(withQuery("/schedule", params));
export const getStudentExams = (params?: { from?: string; to?: string; sectionId?: number; examType?: number }) => studentRequest<StudentExamSchedule[]>(withQuery("/exams", params));
export const getStudentAssignments = (params?: { sectionId?: number; submissionStatus?: number; fromDueAt?: string; toDueAt?: string }) => studentRequest<StudentAssignment[]>(withQuery("/assignments", params));
export const getStudentAssignment = (assignmentId: number) => studentRequest<StudentAssignment>(`/assignments/${assignmentId}`);
export const submitStudentAssignment = (assignmentId: number, params: SubmitAssignmentParams) => studentRequest<AssignmentSubmission>(`/assignments/${assignmentId}/submissions`, "POST", params);
export const getStudentSubmissions = (params?: { assignmentId?: number; sectionId?: number; status?: number }) => studentRequest<StudentSubmission[]>(withQuery("/submissions", params));
export const getStudentMaterials = (params?: { sectionId?: number; search?: string }) => studentRequest<StudentMaterial[]>(withQuery("/materials", params));
export const getStudentGrades = (params?: { sectionId?: number }) => studentRequest<StudentGrade[]>(withQuery("/grades", params));
export const getStudentSectionScores = (params?: { sectionId?: number }) => studentRequest<StudentSectionScore[]>(withQuery("/scores", params));
export const getStudentGpa = () => studentRequest<StudentGpa>("/gpa");
export const getStudyGoals = (params?: { status?: number; goalType?: number }) => studentRequest<StudyGoal[]>(withQuery("/goals", params));
export const getStudyGoal = (goalId: number) => studentRequest<StudyGoal>(`/goals/${goalId}`);
export const createStudyGoal = (params: StudyGoalWriteParams) => studentRequest<StudyGoal>("/goals", "POST", params);
export const updateStudyGoal = (goalId: number, params: StudyGoalWriteParams) => studentRequest<StudyGoal>(`/goals/${goalId}`, "PUT", params);
export const deleteStudyGoal = (goalId: number) => studentRequest<void>(`/goals/${goalId}`, "DELETE");
export const getStudyTasks = (params?: { status?: number; courseId?: number; fromDueAt?: string; toDueAt?: string }) => studentRequest<StudyTask[]>(withQuery("/tasks", params));
export const getStudyTask = (studyTaskId: number) => studentRequest<StudyTask>(`/tasks/${studyTaskId}`);
export const createStudyTask = (params: StudyTaskWriteParams) => studentRequest<StudyTask>("/tasks", "POST", params);
export const updateStudyTask = (studyTaskId: number, params: StudyTaskWriteParams) => studentRequest<StudyTask>(`/tasks/${studyTaskId}`, "PUT", params);
export const deleteStudyTask = (studyTaskId: number) => studentRequest<void>(`/tasks/${studyTaskId}`, "DELETE");
export const getStudentAnnouncements = (params?: { isRead?: boolean; announcementType?: number; sectionId?: number; activeOnly?: boolean; page?: number; pageSize?: number }) => studentRequest<PagedResult<StudentAnnouncement>>(withQuery("/announcements", params));
export const markStudentAnnouncementAsRead = (announcementId: number) => studentRequest<void>(`/announcements/${announcementId}/read`, "POST");
