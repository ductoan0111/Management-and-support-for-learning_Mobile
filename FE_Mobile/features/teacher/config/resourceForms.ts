import type { Row } from "@/api/teacher";
export type Field = { key: string; label: string; type?: "text" | "multiline" | "number" | "date" | "datetime" | "url" | "switch" | "select"; required?: boolean; min?: number; max?: number; options?: {value: string; label: string}[] };
export type ResourceKind = "materials" | "assignments" | "announcements";
type Config = { title: string; id: string; fields: Field[]; defaults: Row };
export const resources: Record<ResourceKind, Config> = {
  materials: { title: "Tài liệu", id: "materialId", defaults: { title: "", description: "", materialType: "", fileUrl: "", externalUrl: "", isVisible: true }, fields: [
    {key: "title", label: "Tiêu đề", required: true}, {key: "description", label: "Mô tả", type: "multiline"}, {key: "materialType", label: "Loại tài liệu"},
    {key: "fileUrl", label: "Đường dẫn tệp", type: "url"}, {key: "externalUrl", label: "Liên kết tham khảo", type: "url"}, {key: "isVisible", label: "Hiển thị cho sinh viên", type: "switch"},
  ]},
  assignments: { title: "Bài tập", id: "assignmentId", defaults: { title: "", description: "", attachmentUrl: "", openAt: "", dueAt: "", maxScore: 10, allowLateSubmission: false, isPublished: true }, fields: [
    {key: "title", label: "Tiêu đề", required: true}, {key: "description", label: "Nội dung", type: "multiline"}, {key: "attachmentUrl", label: "Liên kết tệp đính kèm", type: "url"},
    {key: "openAt", label: "Mở từ (YYYY-MM-DD HH:mm)", type: "datetime"}, {key: "dueAt", label: "Hạn nộp (YYYY-MM-DD HH:mm)", type: "datetime", required: true},
    {key: "maxScore", label: "Điểm tối đa", type: "number", required: true, min: 0.01}, {key: "allowLateSubmission", label: "Cho phép nộp muộn", type: "switch"}, {key: "isPublished", label: "Công bố bài tập", type: "switch"},
  ]},
  announcements: { title: "Thông báo", id: "announcementId", defaults: { title: "", content: "", announcementType: 2, expiresAt: "", isActive: true }, fields: [
    {key: "title", label: "Tiêu đề", required: true}, {key: "content", label: "Nội dung", type: "multiline", required: true},
    {key: "announcementType", label: "Loại thông báo", type: "select", required: true, options: [{value:"1",label:"Hệ thống"},{value:"2",label:"Lớp học"},{value:"3",label:"Bài tập"},{value:"4",label:"Lịch thi"},{value:"5",label:"Điểm"}]},
    {key: "expiresAt", label: "Hết hạn (YYYY-MM-DD HH:mm)", type: "datetime"}, {key: "isActive", label: "Đang hiển thị", type: "switch"},
  ]},
};
