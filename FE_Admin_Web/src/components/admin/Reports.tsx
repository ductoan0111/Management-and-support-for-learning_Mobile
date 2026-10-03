import { Download } from "lucide-react";
import type { AdminReport, ChartPoint } from "../../types/admin";

type ReportsProps = { isLoading: boolean; report: AdminReport | null };

const COLORS = ["#176d73", "#2f9aa0", "#c65f3c", "#e0a458", "#285fbd", "#7a5cc7", "#1d8059", "#b93a2f"];
const GRADE_COLORS = ["#b93a2f", "#d9774f", "#e0a458", "#7fb069", "#2f9aa0", "#176d73"];

function BarList({ data, emptyText }: { data: ChartPoint[]; emptyText: string }) {
  const max = Math.max(1, ...data.map((item) => item.value));
  if (!data.length) return <p className="chart-empty">{emptyText}</p>;
  return (
    <ul className="bar-list">
      {data.map((item, index) => (
        <li key={item.label}>
          <div className="bar-label">
            <span title={item.label}>{item.label}</span>
            <strong>{item.value}</strong>
          </div>
          <div className="bar-track">
            <div className="bar-fill" style={{ background: COLORS[index % COLORS.length], width: `${(item.value / max) * 100}%` }} />
          </div>
        </li>
      ))}
    </ul>
  );
}

function ColumnChart({ data }: { data: ChartPoint[] }) {
  const max = Math.max(1, ...data.map((item) => item.value));
  return (
    <div className="column-chart" role="img" aria-label="Phân bố điểm">
      {data.map((item, index) => (
        <div className="column" key={item.label}>
          <strong>{item.value}</strong>
          <div className="column-bar" style={{ background: GRADE_COLORS[index % GRADE_COLORS.length], height: `${Math.max((item.value / max) * 100, item.value ? 4 : 0)}%` }} />
          <span>{item.label}</span>
        </div>
      ))}
    </div>
  );
}

function Donut({ data }: { data: ChartPoint[] }) {
  const total = data.reduce((sum, item) => sum + item.value, 0);
  const radius = 54;
  const circumference = 2 * Math.PI * radius;
  let offset = 0;
  if (!total) return <p className="chart-empty">Chưa có dữ liệu.</p>;
  return (
    <div className="donut-wrap">
      <svg viewBox="0 0 140 140" width="160" height="160" role="img" aria-label="Sinh viên theo khoa">
        <g transform="rotate(-90 70 70)">
          {data.filter((item) => item.value > 0).map((item, index) => {
            const length = (item.value / total) * circumference;
            const circle = (
              <circle key={item.label} cx="70" cy="70" r={radius} fill="none" stroke={COLORS[index % COLORS.length]} strokeWidth="22"
                strokeDasharray={`${length} ${circumference - length}`} strokeDashoffset={-offset} />
            );
            offset += length;
            return circle;
          })}
        </g>
        <text x="70" y="68" textAnchor="middle" className="donut-total">{total}</text>
        <text x="70" y="86" textAnchor="middle" className="donut-caption">sinh viên</text>
      </svg>
      <ul className="legend">
        {data.map((item, index) => (
          <li key={item.label}><i style={{ background: COLORS[index % COLORS.length] }} />{item.label}<strong>{item.value}</strong></li>
        ))}
      </ul>
    </div>
  );
}

function csvCell(value: string | number) {
  const text = String(value);
  return /[",\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

export function exportReportCsv(report: AdminReport) {
  const rows: (string | number)[][] = [
    ["BÁO CÁO THỐNG KÊ"],
    [],
    ["Sinh viên theo khoa"], ["Khoa", "Số sinh viên"], ...report.studentsByDepartment.map((x) => [x.label, x.value]),
    [],
    ["Sinh viên theo ngành"], ["Ngành", "Số sinh viên"], ...report.studentsByMajor.map((x) => [x.label, x.value]),
    [],
    ["Lớp học phần theo học kỳ"], ["Học kỳ", "Số lớp", "Lượt đăng ký"], ...report.sectionsBySemester.map((x) => [x.label, x.sections, x.enrollments]),
    [],
    ["Phân bố điểm (thang 10)"], ["Khoảng điểm", "Số lượt"], ...report.gradeDistribution.map((x) => [x.label, x.value]),
    ["Điểm trung bình", report.averageScore],
  ];
  // BOM để Excel đọc đúng tiếng Việt UTF-8.
  const csv = "\uFEFF" + rows.map((row) => row.map(csvCell).join(",")).join("\r\n");
  const url = URL.createObjectURL(new Blob([csv], { type: "text/csv;charset=utf-8" }));
  const link = document.createElement("a");
  link.href = url;
  link.download = `bao-cao-thong-ke-${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);
}

export function Reports({ isLoading, report }: ReportsProps) {
  if (isLoading && !report) return <p className="chart-empty">Đang tải báo cáo...</p>;
  if (!report) return null;
  return (
    <section className="reports" aria-label="Báo cáo thống kê">
      <div className="reports-header">
        <div>
          <h2>Báo cáo thống kê</h2>
          <p className="page-description">Điểm trung bình toàn hệ thống: <strong>{report.averageScore}</strong> · {report.gradedEnrollments} lượt đã có điểm</p>
        </div>
        <button className="primary-button" type="button" onClick={() => exportReportCsv(report)}>
          <Download size={18} aria-hidden="true" />
          <span>Xuất Excel (CSV)</span>
        </button>
      </div>

      <div className="chart-grid">
        <article className="chart-card">
          <h3>Sinh viên theo khoa</h3>
          <Donut data={report.studentsByDepartment} />
        </article>
        <article className="chart-card">
          <h3>Phân bố điểm</h3>
          <ColumnChart data={report.gradeDistribution} />
        </article>
        <article className="chart-card">
          <h3>Sinh viên theo ngành</h3>
          <BarList data={report.studentsByMajor} emptyText="Chưa có ngành." />
        </article>
        <article className="chart-card">
          <h3>Lớp học phần theo học kỳ</h3>
          {report.sectionsBySemester.length ? (
            <div className="table-wrap">
              <table className="mini-table">
                <thead><tr><th>Học kỳ</th><th>Số lớp</th><th>Lượt đăng ký</th></tr></thead>
                <tbody>
                  {report.sectionsBySemester.map((item) => (
                    <tr key={item.label}><td>{item.label}</td><td>{item.sections}</td><td>{item.enrollments}</td></tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : <p className="chart-empty">Chưa có học kỳ.</p>}
        </article>
      </div>
    </section>
  );
}
