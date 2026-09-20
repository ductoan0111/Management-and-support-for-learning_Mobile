import {
  BookOpen,
  GraduationCap,
  LayoutDashboard,
  Loader2,
  Plus,
  RefreshCw,
  Search,
  Users,
  X,
} from "lucide-react";
import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  createStudent,
  fallbackStudents,
  getStudents,
} from "./api/adminStudents";
import type {
  AdminStudent,
  CreateAdminStudentRequest,
  StudentFilters,
} from "./types/adminStudent";

const navItems = [
  { label: "Sinh viên", active: true, icon: Users },
  { label: "Giảng viên", active: false, icon: GraduationCap },
  { label: "Lớp học", active: false, icon: BookOpen },
  { label: "Thống kê", active: false, icon: LayoutDashboard },
];

const initialForm: CreateAdminStudentRequest = {
  userId: 0,
  studentCode: "",
  academicClassId: null,
  majorId: 0,
  enrollmentYear: new Date().getFullYear(),
  status: 1,
};

function isActiveStudent(student: AdminStudent) {
  return student.isActive && student.status !== 0;
}

function statusLabel(student: AdminStudent) {
  return isActiveStudent(student) ? "Đang học" : "Tạm khóa";
}

function normalizeText(value: string | number | null | undefined) {
  return String(value ?? "").toLowerCase().trim();
}

function filterStudents(students: AdminStudent[], filters: StudentFilters) {
  const search = normalizeText(filters.search);

  return students.filter((student) => {
    const matchesSearch =
      !search ||
      normalizeText(student.studentCode).includes(search) ||
      normalizeText(student.fullName).includes(search) ||
      normalizeText(student.email).includes(search);

    const matchesStatus =
      !filters.status || String(student.status) === filters.status;

    return matchesSearch && matchesStatus;
  });
}

export default function App() {
  const [students, setStudents] = useState<AdminStudent[]>(fallbackStudents);
  const [filters, setFilters] = useState<StudentFilters>({
    search: "",
    status: "",
  });
  const [form, setForm] = useState<CreateAdminStudentRequest>(initialForm);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [notice, setNotice] = useState<string | null>(null);

  const visibleStudents = useMemo(
    () => filterStudents(students, filters),
    [students, filters],
  );

  const activeCount = students.filter(isActiveStudent).length;
  const inactiveCount = students.length - activeCount;

  const loadStudents = async () => {
    setIsLoading(true);
    setNotice(null);

    try {
      const result = await getStudents(filters);
      setStudents(result.items);
    } catch (error) {
      setStudents(fallbackStudents);
      setNotice(
        error instanceof Error
          ? `${error.message} Đang hiển thị dữ liệu mẫu.`
          : "Đang hiển thị dữ liệu mẫu.",
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadStudents();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!form.userId || !form.studentCode.trim() || !form.majorId) {
      setNotice("Vui lòng nhập User ID, mã sinh viên và mã ngành.");
      return;
    }

    setIsSaving(true);
    setNotice(null);

    try {
      await createStudent({
        ...form,
        studentCode: form.studentCode.trim().toUpperCase(),
        academicClassId: form.academicClassId || null,
      });
      setForm(initialForm);
      setIsDialogOpen(false);
      await loadStudents();
      setNotice("Đã thêm sinh viên thành công.");
    } catch (error) {
      setNotice(
        error instanceof Error
          ? error.message
          : "Chưa kết nối được backend admin.",
      );
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">SS</div>
          <div>
            <strong>Study Support</strong>
            <span>Admin Web</span>
          </div>
        </div>

        <nav className="nav-list" aria-label="Admin navigation">
          {navItems.map(({ active, icon: Icon, label }) => (
            <button
              className={`nav-item ${active ? "active" : ""}`}
              key={label}
              type="button"
            >
              <Icon size={18} aria-hidden="true" />
              <span>{label}</span>
            </button>
          ))}
        </nav>
      </aside>

      <main className="main">
        <header className="topbar">
          <div>
            <p className="eyebrow">Quản trị hệ thống</p>
            <h1>Quản lý sinh viên</h1>
          </div>
          <div className="topbar-actions">
            <button
              className="icon-button"
              disabled={isLoading}
              onClick={loadStudents}
              title="Tải lại"
              type="button"
            >
              <RefreshCw size={18} aria-hidden="true" />
            </button>
            <button
              className="primary-button"
              onClick={() => setIsDialogOpen(true)}
              type="button"
            >
              <Plus size={18} aria-hidden="true" />
              <span>Thêm sinh viên</span>
            </button>
          </div>
        </header>

        <section className="summary-grid" aria-label="Tổng quan sinh viên">
          <SummaryCard label="Tổng sinh viên" value={students.length} />
          <SummaryCard label="Đang học" value={activeCount} tone="success" />
          <SummaryCard label="Tạm khóa" value={inactiveCount} tone="warning" />
        </section>

        {notice ? <div className="notice">{notice}</div> : null}

        <section className="panel">
          <div className="panel-toolbar">
            <label className="search-box">
              <Search size={18} aria-hidden="true" />
              <input
                onChange={(event) =>
                  setFilters((current) => ({
                    ...current,
                    search: event.target.value,
                  }))
                }
                placeholder="Tìm theo tên, mã sinh viên hoặc email"
                type="search"
                value={filters.search}
              />
            </label>

            <select
              aria-label="Lọc trạng thái"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  status: event.target.value,
                }))
              }
              value={filters.status}
            >
              <option value="">Tất cả trạng thái</option>
              <option value="1">Đang học</option>
              <option value="0">Tạm khóa</option>
            </select>
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Mã sinh viên</th>
                  <th>Họ tên</th>
                  <th>Email</th>
                  <th>Lớp</th>
                  <th>Ngành</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  <tr>
                    <td colSpan={6}>
                      <div className="table-state">
                        <Loader2 className="spin" size={20} aria-hidden="true" />
                        <span>Đang tải dữ liệu...</span>
                      </div>
                    </td>
                  </tr>
                ) : null}

                {!isLoading && visibleStudents.length === 0 ? (
                  <tr>
                    <td colSpan={6}>
                      <div className="table-state">Không có sinh viên phù hợp.</div>
                    </td>
                  </tr>
                ) : null}

                {!isLoading
                  ? visibleStudents.map((student) => (
                      <tr key={student.studentId}>
                        <td>
                          <strong>{student.studentCode}</strong>
                        </td>
                        <td>{student.fullName}</td>
                        <td>{student.email}</td>
                        <td>{student.className ?? "-"}</td>
                        <td>{student.majorName}</td>
                        <td>
                          <span
                            className={`status ${
                              isActiveStudent(student) ? "active" : "inactive"
                            }`}
                          >
                            {statusLabel(student)}
                          </span>
                        </td>
                      </tr>
                    ))
                  : null}
              </tbody>
            </table>
          </div>
        </section>
      </main>

      {isDialogOpen ? (
        <div className="dialog-backdrop" role="presentation">
          <form className="dialog-card" onSubmit={handleSubmit}>
            <div className="dialog-header">
              <h2>Thêm sinh viên</h2>
              <button
                className="icon-button"
                onClick={() => setIsDialogOpen(false)}
                title="Đóng"
                type="button"
              >
                <X size={18} aria-hidden="true" />
              </button>
            </div>

            <FormNumberField
              label="User ID"
              min={1}
              onChange={(value) =>
                setForm((current) => ({ ...current, userId: value }))
              }
              placeholder="101"
              value={form.userId}
            />
            <label>
              Mã sinh viên
              <input
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    studentCode: event.target.value,
                  }))
                }
                placeholder="SV001"
                value={form.studentCode}
              />
            </label>
            <FormNumberField
              label="Mã lớp học"
              min={1}
              onChange={(value) =>
                setForm((current) => ({
                  ...current,
                  academicClassId: value || null,
                }))
              }
              placeholder="1"
              value={form.academicClassId ?? 0}
            />
            <FormNumberField
              label="Mã ngành"
              min={1}
              onChange={(value) =>
                setForm((current) => ({ ...current, majorId: value }))
              }
              placeholder="1"
              value={form.majorId}
            />
            <FormNumberField
              label="Năm nhập học"
              min={2000}
              onChange={(value) =>
                setForm((current) => ({ ...current, enrollmentYear: value }))
              }
              placeholder="2026"
              value={form.enrollmentYear}
            />

            <div className="dialog-actions">
              <button
                className="secondary-button"
                onClick={() => setIsDialogOpen(false)}
                type="button"
              >
                Hủy
              </button>
              <button className="primary-button" disabled={isSaving} type="submit">
                {isSaving ? (
                  <Loader2 className="spin" size={18} aria-hidden="true" />
                ) : (
                  <Plus size={18} aria-hidden="true" />
                )}
                <span>Lưu</span>
              </button>
            </div>
          </form>
        </div>
      ) : null}
    </div>
  );
}

type SummaryCardProps = {
  label: string;
  value: number;
  tone?: "default" | "success" | "warning";
};

function SummaryCard({ label, tone = "default", value }: SummaryCardProps) {
  return (
    <article className={`summary-card ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

type FormNumberFieldProps = {
  label: string;
  min: number;
  onChange: (value: number) => void;
  placeholder: string;
  value: number;
};

function FormNumberField({
  label,
  min,
  onChange,
  placeholder,
  value,
}: FormNumberFieldProps) {
  return (
    <label>
      {label}
      <input
        min={min}
        onChange={(event) => onChange(Number(event.target.value))}
        placeholder={placeholder}
        type="number"
        value={value || ""}
      />
    </label>
  );
}
