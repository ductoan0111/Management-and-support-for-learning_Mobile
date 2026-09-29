import {
  BookMarked,
  BookOpen,
  Building2,
  GraduationCap,
  ListChecks,
  School,
  UserCog,
  Users,
} from "lucide-react";
import { BooleanBadge, CurrentBadge, StatusBadge } from "../components/admin/Badges";
import {
  activeOptions,
  currentOptions,
  genderOptions,
  sectionStatusOptions,
  studentStatusOptions,
  teacherStatusOptions,
} from "./adminOptions";
import type { ResourceConfig } from "../types/admin";
import { labelFromLookup, optionsFromRecords } from "../utils/adminDisplay";

const currentYear = new Date().getFullYear();
const today = new Date().toISOString().slice(0, 10);

export const resourceConfigs: ResourceConfig[] = [
  {
    key: "users",
    title: "Tài khoản",
    navLabel: "Tài khoản",
    description: "Quản lý tài khoản đăng nhập và trạng thái sử dụng.",
    path: "/api/admin/users",
    icon: UserCog,
    idKey: "userId",
    canDelete: false,
    columns: [
      { key: "username", label: "Tên đăng nhập" },
      { key: "fullName", label: "Họ tên" },
      { key: "email", label: "Email" },
      {
        key: "roleCode",
        label: "Vai trò",
        render: (row) => <span className="badge info">{row.roleCode}</span>,
      },
      {
        key: "isActive",
        label: "Trạng thái",
        render: (row) => <BooleanBadge value={row.isActive} />,
      },
    ],
    fields: [
      {
        key: "username",
        label: "Tên đăng nhập",
        kind: "text",
        required: true,
        createOnly: true,
      },
      {
        key: "password",
        label: "Mật khẩu",
        kind: "password",
        required: true,
        createOnly: true,
        min: 12,
        placeholder: "Tối thiểu 12 ký tự",
      },
      {
        key: "roleId",
        label: "Vai trò",
        kind: "select",
        required: true,
        createOnly: true,
        valueType: "number",
        getOptions: (lookups) =>
          lookups.roles.map((role) => ({
            label: `${role.roleCode} - ${role.roleName}`,
            value: Number(role.roleId),
          })),
      },
      { key: "email", label: "Email", kind: "text", required: true },
      { key: "fullName", label: "Họ tên", kind: "text", required: true },
      { key: "phone", label: "Số điện thoại", kind: "text", optional: true },
      { key: "isActive", label: "Hoạt động", kind: "checkbox" },
    ],
    defaultValues: {
      username: "",
      password: "",
      roleId: "",
      email: "",
      fullName: "",
      phone: "",
      isActive: true,
    },
    filters: [
      {
        key: "roleId",
        label: "Vai trò",
        getOptions: (lookups) =>
          lookups.roles.map((role) => ({
            label: String(role.roleCode),
            value: Number(role.roleId),
          })),
      },
      { key: "isActive", label: "Trạng thái", getOptions: () => activeOptions },
    ],
  },
  {
    key: "students",
    title: "Sinh viên",
    navLabel: "Sinh viên",
    description: "Quản lý hồ sơ sinh viên, lớp hành chính và ngành học.",
    path: "/api/admin/students",
    icon: Users,
    idKey: "studentId",
    columns: [
      { key: "studentCode", label: "Mã SV" },
      { key: "fullName", label: "Họ tên" },
      { key: "email", label: "Email" },
      {
        key: "academicClassId",
        label: "Lớp",
        render: (row, lookups) =>
          row.className ||
          labelFromLookup(
            lookups,
            "academicClasses",
            "academicClassId",
            row.academicClassId,
            ["classCode", "className"],
          ),
      },
      {
        key: "majorId",
        label: "Ngành",
        render: (row, lookups) =>
          row.majorName ||
          labelFromLookup(lookups, "majors", "majorId", row.majorId, [
            "majorCode",
            "majorName",
          ]),
      },
      { key: "enrollmentYear", label: "Năm vào" },
      {
        key: "status",
        label: "Trạng thái",
        render: (row) => (
          <StatusBadge value={row.status} labels={{ 0: "Tạm khóa", 1: "Đang học" }} />
        ),
      },
    ],
    fields: [
      {
        key: "userId",
        label: "User ID",
        kind: "number",
        required: true,
        min: 1,
        createOnly: true,
      },
      { key: "studentCode", label: "Mã sinh viên", kind: "text", required: true },
      {
        key: "academicClassId",
        label: "Lớp hành chính",
        kind: "select",
        optional: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.academicClasses, "academicClassId", [
            "classCode",
            "className",
          ]),
      },
      {
        key: "majorId",
        label: "Ngành",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.majors, "majorId", ["majorCode", "majorName"]),
      },
      {
        key: "enrollmentYear",
        label: "Năm nhập học",
        kind: "number",
        required: true,
        min: 1900,
      },
      {
        key: "status",
        label: "Trạng thái",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: () => studentStatusOptions,
      },
      { key: "fullName", label: "Họ tên", kind: "text", optional: true, editOnly: true },
      { key: "email", label: "Email", kind: "text", optional: true, editOnly: true },
      { key: "phone", label: "Số điện thoại", kind: "text", optional: true, editOnly: true },
      { key: "dateOfBirth", label: "Ngày sinh", kind: "date", optional: true, editOnly: true },
      {
        key: "gender",
        label: "Giới tính",
        kind: "select",
        optional: true,
        editOnly: true,
        valueType: "number",
        getOptions: () => genderOptions,
      },
      { key: "avatarUrl", label: "Ảnh đại diện", kind: "text", optional: true, editOnly: true },
      { key: "isActive", label: "Tài khoản hoạt động", kind: "checkbox", editOnly: true },
    ],
    defaultValues: {
      userId: "",
      studentCode: "",
      academicClassId: "",
      majorId: "",
      enrollmentYear: currentYear,
      status: 1,
      fullName: "",
      email: "",
      phone: "",
      dateOfBirth: "",
      gender: "",
      avatarUrl: "",
      isActive: true,
    },
    filters: [
      { key: "status", label: "Trạng thái", getOptions: () => studentStatusOptions },
      {
        key: "majorId",
        label: "Ngành",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.majors, "majorId", ["majorCode", "majorName"]),
      },
      {
        key: "academicClassId",
        label: "Lớp",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.academicClasses, "academicClassId", [
            "classCode",
            "className",
          ]),
      },
    ],
  },
  {
    key: "teachers",
    title: "Giảng viên",
    navLabel: "Giảng viên",
    description: "Quản lý hồ sơ giảng viên và khoa phụ trách.",
    path: "/api/admin/teachers",
    icon: GraduationCap,
    idKey: "teacherId",
    columns: [
      { key: "teacherCode", label: "Mã GV" },
      {
        key: "userId",
        label: "Tài khoản",
        render: (row, lookups) =>
          labelFromLookup(lookups, "users", "userId", row.userId, [
            "username",
            "fullName",
          ]),
      },
      {
        key: "departmentId",
        label: "Khoa",
        render: (row, lookups) =>
          labelFromLookup(lookups, "departments", "departmentId", row.departmentId, [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "academicTitle", label: "Học hàm" },
      { key: "specialization", label: "Chuyên môn" },
      {
        key: "status",
        label: "Trạng thái",
        render: (row) => (
          <StatusBadge value={row.status} labels={{ 0: "Ngưng", 1: "Đang dạy" }} />
        ),
      },
    ],
    fields: [
      {
        key: "userId",
        label: "Tài khoản",
        kind: "select",
        required: true,
        valueType: "number",
        disabledOnEdit: true,
        getOptions: (lookups) =>
          optionsFromRecords(lookups.users, "userId", ["username", "fullName"]),
      },
      { key: "teacherCode", label: "Mã giảng viên", kind: "text", required: true },
      {
        key: "departmentId",
        label: "Khoa",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "academicTitle", label: "Học hàm", kind: "text", optional: true },
      { key: "specialization", label: "Chuyên môn", kind: "textarea", optional: true },
      {
        key: "status",
        label: "Trạng thái",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: () => teacherStatusOptions,
      },
    ],
    defaultValues: {
      userId: "",
      teacherCode: "",
      departmentId: "",
      academicTitle: "",
      specialization: "",
      status: 1,
    },
    filters: [
      {
        key: "departmentId",
        label: "Khoa",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "status", label: "Trạng thái", getOptions: () => teacherStatusOptions },
    ],
  },
  {
    key: "departments",
    title: "Khoa",
    navLabel: "Khoa",
    description: "Quản lý danh mục khoa và trạng thái hoạt động.",
    path: "/api/admin/departments",
    icon: Building2,
    idKey: "departmentId",
    columns: [
      { key: "departmentCode", label: "Mã khoa" },
      { key: "departmentName", label: "Tên khoa" },
      { key: "description", label: "Mô tả" },
      {
        key: "isActive",
        label: "Trạng thái",
        render: (row) => <BooleanBadge value={row.isActive} />,
      },
    ],
    fields: [
      { key: "departmentCode", label: "Mã khoa", kind: "text", required: true },
      { key: "departmentName", label: "Tên khoa", kind: "text", required: true },
      { key: "description", label: "Mô tả", kind: "textarea", optional: true },
      { key: "isActive", label: "Hoạt động", kind: "checkbox" },
    ],
    defaultValues: {
      departmentCode: "",
      departmentName: "",
      description: "",
      isActive: true,
    },
    filters: [{ key: "isActive", label: "Trạng thái", getOptions: () => activeOptions }],
  },
  {
    key: "majors",
    title: "Ngành",
    navLabel: "Ngành",
    description: "Quản lý ngành đào tạo theo từng khoa.",
    path: "/api/admin/majors",
    icon: School,
    idKey: "majorId",
    columns: [
      { key: "majorCode", label: "Mã ngành" },
      { key: "majorName", label: "Tên ngành" },
      {
        key: "departmentId",
        label: "Khoa",
        render: (row, lookups) =>
          labelFromLookup(lookups, "departments", "departmentId", row.departmentId, [
            "departmentCode",
            "departmentName",
          ]),
      },
      {
        key: "isActive",
        label: "Trạng thái",
        render: (row) => <BooleanBadge value={row.isActive} />,
      },
    ],
    fields: [
      {
        key: "departmentId",
        label: "Khoa",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "majorCode", label: "Mã ngành", kind: "text", required: true },
      { key: "majorName", label: "Tên ngành", kind: "text", required: true },
      { key: "description", label: "Mô tả", kind: "textarea", optional: true },
      { key: "isActive", label: "Hoạt động", kind: "checkbox" },
    ],
    defaultValues: {
      departmentId: "",
      majorCode: "",
      majorName: "",
      description: "",
      isActive: true,
    },
    filters: [
      {
        key: "departmentId",
        label: "Khoa",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "isActive", label: "Trạng thái", getOptions: () => activeOptions },
    ],
  },
  {
    key: "academicClasses",
    title: "Lớp hành chính",
    navLabel: "Lớp hành chính",
    description: "Quản lý lớp theo ngành, khóa tuyển sinh và năm tốt nghiệp.",
    path: "/api/admin/academic-classes",
    icon: BookMarked,
    idKey: "academicClassId",
    columns: [
      { key: "classCode", label: "Mã lớp" },
      { key: "className", label: "Tên lớp" },
      {
        key: "majorId",
        label: "Ngành",
        render: (row, lookups) =>
          labelFromLookup(lookups, "majors", "majorId", row.majorId, [
            "majorCode",
            "majorName",
          ]),
      },
      { key: "intakeYear", label: "Khóa" },
      { key: "graduationYear", label: "Tốt nghiệp" },
      {
        key: "isActive",
        label: "Trạng thái",
        render: (row) => <BooleanBadge value={row.isActive} />,
      },
    ],
    fields: [
      {
        key: "majorId",
        label: "Ngành",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.majors, "majorId", ["majorCode", "majorName"]),
      },
      { key: "classCode", label: "Mã lớp", kind: "text", required: true },
      { key: "className", label: "Tên lớp", kind: "text", required: true },
      {
        key: "intakeYear",
        label: "Năm tuyển sinh",
        kind: "number",
        required: true,
        min: 1900,
      },
      {
        key: "graduationYear",
        label: "Năm tốt nghiệp",
        kind: "number",
        optional: true,
        min: 1900,
      },
      { key: "isActive", label: "Hoạt động", kind: "checkbox" },
    ],
    defaultValues: {
      majorId: "",
      classCode: "",
      className: "",
      intakeYear: currentYear,
      graduationYear: "",
      isActive: true,
    },
    filters: [
      {
        key: "majorId",
        label: "Ngành",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.majors, "majorId", ["majorCode", "majorName"]),
      },
      { key: "isActive", label: "Trạng thái", getOptions: () => activeOptions },
    ],
  },
  {
    key: "courses",
    title: "Môn học",
    navLabel: "Môn học",
    description: "Quản lý môn học, số tín chỉ và khoa phụ trách.",
    path: "/api/admin/courses",
    icon: BookOpen,
    idKey: "courseId",
    columns: [
      { key: "courseCode", label: "Mã môn" },
      { key: "courseName", label: "Tên môn" },
      { key: "credits", label: "Tín chỉ" },
      {
        key: "departmentId",
        label: "Khoa",
        render: (row, lookups) =>
          labelFromLookup(lookups, "departments", "departmentId", row.departmentId, [
            "departmentCode",
            "departmentName",
          ]),
      },
      {
        key: "isActive",
        label: "Trạng thái",
        render: (row) => <BooleanBadge value={row.isActive} />,
      },
    ],
    fields: [
      {
        key: "departmentId",
        label: "Khoa",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "courseCode", label: "Mã môn", kind: "text", required: true },
      { key: "courseName", label: "Tên môn", kind: "text", required: true },
      {
        key: "credits",
        label: "Số tín chỉ",
        kind: "number",
        required: true,
        min: 1,
        max: 15,
      },
      { key: "description", label: "Mô tả", kind: "textarea", optional: true },
      { key: "isActive", label: "Hoạt động", kind: "checkbox" },
    ],
    defaultValues: {
      departmentId: "",
      courseCode: "",
      courseName: "",
      credits: 3,
      description: "",
      isActive: true,
    },
    filters: [
      {
        key: "departmentId",
        label: "Khoa",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.departments, "departmentId", [
            "departmentCode",
            "departmentName",
          ]),
      },
      { key: "isActive", label: "Trạng thái", getOptions: () => activeOptions },
    ],
  },
  {
    key: "semesters",
    title: "Học kỳ",
    navLabel: "Học kỳ",
    description: "Quản lý học kỳ, năm học và khoảng thời gian áp dụng.",
    path: "/api/admin/semesters",
    icon: ListChecks,
    idKey: "semesterId",
    columns: [
      { key: "semesterCode", label: "Mã học kỳ" },
      { key: "semesterName", label: "Tên học kỳ" },
      { key: "academicYear", label: "Năm học" },
      { key: "startDate", label: "Bắt đầu" },
      { key: "endDate", label: "Kết thúc" },
      {
        key: "isCurrent",
        label: "Hiện tại",
        render: (row) => <CurrentBadge value={row.isCurrent} />,
      },
    ],
    fields: [
      { key: "semesterCode", label: "Mã học kỳ", kind: "text", required: true },
      { key: "semesterName", label: "Tên học kỳ", kind: "text", required: true },
      {
        key: "academicYear",
        label: "Năm học",
        kind: "text",
        required: true,
        placeholder: "2026-2027",
      },
      { key: "startDate", label: "Ngày bắt đầu", kind: "date", required: true },
      { key: "endDate", label: "Ngày kết thúc", kind: "date", required: true },
      { key: "isCurrent", label: "Học kỳ hiện tại", kind: "checkbox" },
    ],
    defaultValues: {
      semesterCode: "",
      semesterName: "",
      academicYear: `${currentYear}-${currentYear + 1}`,
      startDate: today,
      endDate: today,
      isCurrent: false,
    },
    filters: [{ key: "isCurrent", label: "Hiện tại", getOptions: () => currentOptions }],
  },
  {
    key: "courseSections",
    title: "Lớp học phần",
    navLabel: "Lớp học phần",
    description: "Quản lý lớp học phần theo môn học và học kỳ.",
    path: "/api/admin/course-sections",
    icon: BookOpen,
    idKey: "sectionId",
    columns: [
      { key: "sectionCode", label: "Mã lớp HP" },
      { key: "sectionName", label: "Tên lớp" },
      {
        key: "courseId",
        label: "Môn học",
        render: (row, lookups) =>
          labelFromLookup(lookups, "courses", "courseId", row.courseId, [
            "courseCode",
            "courseName",
          ]),
      },
      {
        key: "semesterId",
        label: "Học kỳ",
        render: (row, lookups) =>
          labelFromLookup(lookups, "semesters", "semesterId", row.semesterId, [
            "semesterCode",
            "semesterName",
          ]),
      },
      { key: "maxStudents", label: "Sĩ số" },
      {
        key: "status",
        label: "Trạng thái",
        render: (row) => (
          <StatusBadge value={row.status} labels={{ 0: "Đóng", 1: "Mở", 2: "Kết thúc" }} />
        ),
      },
    ],
    fields: [
      {
        key: "courseId",
        label: "Môn học",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.courses, "courseId", ["courseCode", "courseName"]),
      },
      {
        key: "semesterId",
        label: "Học kỳ",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.semesters, "semesterId", [
            "semesterCode",
            "semesterName",
          ]),
      },
      { key: "sectionCode", label: "Mã lớp học phần", kind: "text", required: true },
      { key: "sectionName", label: "Tên lớp", kind: "text", optional: true },
      { key: "maxStudents", label: "Sĩ số tối đa", kind: "number", optional: true, min: 1 },
      {
        key: "status",
        label: "Trạng thái",
        kind: "select",
        required: true,
        valueType: "number",
        getOptions: () => sectionStatusOptions,
      },
    ],
    defaultValues: {
      courseId: "",
      semesterId: "",
      sectionCode: "",
      sectionName: "",
      maxStudents: "",
      status: 1,
    },
    filters: [
      {
        key: "courseId",
        label: "Môn học",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.courses, "courseId", ["courseCode", "courseName"]),
      },
      {
        key: "semesterId",
        label: "Học kỳ",
        getOptions: (lookups) =>
          optionsFromRecords(lookups.semesters, "semesterId", [
            "semesterCode",
            "semesterName",
          ]),
      },
      { key: "status", label: "Trạng thái", getOptions: () => sectionStatusOptions },
    ],
  },
];
