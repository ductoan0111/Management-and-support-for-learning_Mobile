import type {
  AdminStudent,
  CreateAdminStudentRequest,
  PagedResult,
  StudentFilters,
} from "../types/adminStudent";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ??
  "https://localhost:7138";

export const fallbackStudents: AdminStudent[] = [
  {
    studentId: 1,
    userId: 101,
    studentCode: "SV001",
    fullName: "Nguyễn Văn A",
    email: "sv001@school.edu",
    phone: "0901234567",
    isActive: true,
    academicClassId: 1,
    classCode: "KTPM01",
    className: "KTPM01",
    majorId: 1,
    majorCode: "KTPM",
    majorName: "Kỹ thuật phần mềm",
    departmentId: 1,
    enrollmentYear: 2026,
    status: 1,
  },
  {
    studentId: 2,
    userId: 114,
    studentCode: "SV014",
    fullName: "Trần Thị B",
    email: "sv014@school.edu",
    phone: "0912345678",
    isActive: true,
    academicClassId: 2,
    classCode: "CNTT02",
    className: "CNTT02",
    majorId: 2,
    majorCode: "CNTT",
    majorName: "Công nghệ thông tin",
    departmentId: 1,
    enrollmentYear: 2026,
    status: 1,
  },
  {
    studentId: 3,
    userId: 127,
    studentCode: "SV027",
    fullName: "Lê Minh C",
    email: "sv027@school.edu",
    phone: null,
    isActive: false,
    academicClassId: 3,
    classCode: "HTTT01",
    className: "HTTT01",
    majorId: 3,
    majorCode: "HTTT",
    majorName: "Hệ thống thông tin",
    departmentId: 2,
    enrollmentYear: 2025,
    status: 0,
  },
];

type ApiPagedResult<T> = PagedResult<T> | {
  Items?: T[];
  Page?: number;
  PageSize?: number;
  TotalCount?: number;
  TotalPages?: number;
};

function normalizePagedResult<T>(payload: ApiPagedResult<T>): PagedResult<T> {
  return {
    items: "items" in payload ? payload.items : payload.Items ?? [],
    page: "page" in payload ? payload.page : payload.Page ?? 1,
    pageSize: "pageSize" in payload ? payload.pageSize : payload.PageSize ?? 20,
    totalCount:
      "totalCount" in payload ? payload.totalCount : payload.TotalCount ?? 0,
    totalPages:
      "totalPages" in payload ? payload.totalPages : payload.TotalPages ?? 1,
  };
}

export async function getStudents(
  filters: StudentFilters,
): Promise<PagedResult<AdminStudent>> {
  const params = new URLSearchParams({
    page: "1",
    pageSize: "50",
  });

  if (filters.search.trim()) {
    params.set("search", filters.search.trim());
  }

  if (filters.status) {
    params.set("status", filters.status);
  }

  const response = await fetch(`${API_BASE_URL}/api/admin/students?${params}`);
  if (!response.ok) {
    throw new Error("Không tải được danh sách sinh viên.");
  }

  return normalizePagedResult<AdminStudent>(await response.json());
}

export async function createStudent(
  request: CreateAdminStudentRequest,
): Promise<AdminStudent> {
  const response = await fetch(`${API_BASE_URL}/api/admin/students`, {
    body: JSON.stringify(request),
    headers: { "Content-Type": "application/json" },
    method: "POST",
  });

  if (!response.ok) {
    throw new Error("Không tạo được sinh viên.");
  }

  return response.json();
}
