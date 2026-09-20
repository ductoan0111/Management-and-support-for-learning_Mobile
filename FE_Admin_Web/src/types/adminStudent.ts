export type AdminStudent = {
  studentId: number;
  userId: number;
  studentCode: string;
  fullName: string;
  email: string;
  phone?: string | null;
  dateOfBirth?: string | null;
  gender?: number | null;
  avatarUrl?: string | null;
  isActive: boolean;
  academicClassId?: number | null;
  classCode?: string | null;
  className?: string | null;
  majorId: number;
  majorCode: string;
  majorName: string;
  departmentId: number;
  enrollmentYear: number;
  status: number;
};

export type CreateAdminStudentRequest = {
  userId: number;
  studentCode: string;
  academicClassId?: number | null;
  majorId: number;
  enrollmentYear: number;
  status: number;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type StudentFilters = {
  search: string;
  status: string;
};
