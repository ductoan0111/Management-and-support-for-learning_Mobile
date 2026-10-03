import {
  BookMarked,
  BookOpen,
  GraduationCap,
  ListChecks,
  UserCog,
  Users,
} from "lucide-react";
import type { AdminReport, AdminStatistics } from "../../types/admin";
import { Reports } from "./Reports";

type DashboardProps = {
  isLoading: boolean;
  statistics: AdminStatistics | null;
  report: AdminReport | null;
};

export function Dashboard({ isLoading, statistics, report }: DashboardProps) {
  const cards = [
    {
      label: "Tài khoản",
      value: statistics?.totalUsers,
      meta: `${statistics?.activeUsers ?? 0} đang hoạt động`,
      icon: UserCog,
    },
    {
      label: "Sinh viên",
      value: statistics?.totalStudents,
      meta: `${statistics?.activeStudents ?? 0} đang học`,
      icon: Users,
    },
    {
      label: "Giảng viên",
      value: statistics?.totalTeachers,
      meta: `${statistics?.activeTeachers ?? 0} đang công tác`,
      icon: GraduationCap,
    },
    {
      label: "Môn học",
      value: statistics?.totalCourses,
      meta: "Danh mục đào tạo",
      icon: BookOpen,
    },
    {
      label: "Lớp học phần",
      value: statistics?.totalSections,
      meta: `${statistics?.openSections ?? 0} đang mở`,
      icon: BookMarked,
    },
    {
      label: "Đăng ký",
      value: statistics?.activeEnrollments,
      meta: `${statistics?.totalSemesters ?? 0} học kỳ`,
      icon: ListChecks,
    },
  ];

  return (
    <>
      <section className="dashboard-grid">
        {cards.map(({ icon: Icon, label, meta, value }, index) => (
          <article className={`metric-card tone-${index % 6}`} key={label}>
            <div className="metric-icon">
              <Icon size={20} aria-hidden="true" />
            </div>
            <span>{label}</span>
            <strong>{isLoading ? "..." : value ?? 0}</strong>
            <small>{meta}</small>
          </article>
        ))}
      </section>
      <Reports isLoading={isLoading} report={report} />
    </>
  );
}
