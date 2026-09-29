import type { LookupState, SelectOption } from "../types/admin";

export const emptyLookups: LookupState = {
  users: [],
  roles: [],
  departments: [],
  majors: [],
  academicClasses: [],
  courses: [],
  semesters: [],
};

export const activeOptions: SelectOption[] = [
  { label: "Hoạt động", value: "true" },
  { label: "Ngưng", value: "false" },
];

export const currentOptions: SelectOption[] = [
  { label: "Hiện tại", value: "true" },
  { label: "Khác", value: "false" },
];

export const studentStatusOptions: SelectOption[] = [
  { label: "Tạm khóa", value: 0 },
  { label: "Đang học", value: 1 },
];

export const teacherStatusOptions: SelectOption[] = [
  { label: "Ngưng công tác", value: 0 },
  { label: "Đang công tác", value: 1 },
];

export const sectionStatusOptions: SelectOption[] = [
  { label: "Đóng", value: 0 },
  { label: "Mở", value: 1 },
  { label: "Kết thúc", value: 2 },
];

export const genderOptions: SelectOption[] = [
  { label: "Nữ", value: 0 },
  { label: "Nam", value: 1 },
  { label: "Khác", value: 2 },
];
