import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";

export type AdminPrimitive = string | number | boolean | null | undefined;

export type AdminRecord = Record<string, AdminPrimitive>;

export type PagedResult<T = AdminRecord> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type AdminUser = {
  userId: number;
  roleId: number;
  roleCode: string;
  username: string;
  email: string;
  fullName: string;
  phone?: string | null;
  isActive: boolean;
};

export type AdminRole = {
  roleId: number;
  roleCode: string;
  roleName: string;
  description?: string | null;
};

export type AdminSession = {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  user: AdminUser;
};

export type AdminStatistics = {
  totalUsers: number;
  activeUsers: number;
  totalStudents: number;
  activeStudents: number;
  totalTeachers: number;
  activeTeachers: number;
  totalCourses: number;
  totalSections: number;
  openSections: number;
  totalSemesters: number;
  activeEnrollments: number;
};

export type ChartPoint = { label: string; value: number };
export type AdminReport = {
  studentsByDepartment: ChartPoint[];
  studentsByMajor: ChartPoint[];
  sectionsBySemester: { label: string; sections: number; enrollments: number }[];
  gradeDistribution: ChartPoint[];
  averageScore: number;
  gradedEnrollments: number;
};

export type AdminQuery = {
  search?: string;
  page?: number;
  pageSize?: number;
  [key: string]: string | number | boolean | undefined;
};

export type LookupKey =
  | "users"
  | "roles"
  | "departments"
  | "majors"
  | "academicClasses"
  | "courses"
  | "semesters";

export type LookupState = Record<LookupKey, AdminRecord[]>;

export type SelectOption = {
  label: string;
  value: string | number | boolean;
};

export type FieldKind =
  | "text"
  | "number"
  | "textarea"
  | "checkbox"
  | "select"
  | "date"
  | "password";

export type AdminFormState = Record<string, string | number | boolean | null>;

export type ResourceField = {
  key: string;
  label: string;
  kind: FieldKind;
  required?: boolean;
  optional?: boolean;
  min?: number;
  max?: number;
  placeholder?: string;
  valueType?: "string" | "number" | "boolean";
  createOnly?: boolean;
  editOnly?: boolean;
  disabledOnEdit?: boolean;
  getOptions?: (lookups: LookupState) => SelectOption[];
};

export type ResourceColumn = {
  key: string;
  label: string;
  render?: (row: AdminRecord, lookups: LookupState) => ReactNode;
};

export type ResourceFilter = {
  key: string;
  label: string;
  getOptions: (lookups: LookupState) => SelectOption[];
};

export type ResourceConfig = {
  key: string;
  title: string;
  navLabel: string;
  description: string;
  path: string;
  icon: LucideIcon;
  idKey: string;
  canDelete?: boolean;
  columns: ResourceColumn[];
  fields: ResourceField[];
  defaultValues: AdminFormState;
  filters?: ResourceFilter[];
};

export type DialogState = {
  mode: "create" | "edit";
  row: AdminRecord | null;
};

export type Notice = {
  tone: "success" | "error" | "info";
  message: string;
};
