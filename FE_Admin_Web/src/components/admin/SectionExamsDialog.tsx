import { useEffect, useState, type FormEvent } from "react";
import { AlarmClock, Pencil, Plus, RefreshCw, Save, Trash2, X } from "lucide-react";
import {
  AdminApiError,
  deleteSectionExam,
  listSectionExams,
  saveSectionExam,
} from "../../api/admin";
import type { AdminExam, AdminPrimitive, AdminRecord, SaveAdminExam } from "../../types/admin";
import { ActionModal } from "./ActionModal";

const examTypes = [
  { value: 1, label: "Kiểm tra" },
  { value: 2, label: "Giữa kỳ" },
  { value: 3, label: "Cuối kỳ" },
  { value: 4, label: "Khác" },
];

type ExamForm = {
  examName: string;
  examType: string;
  examDate: string;
  startTime: string;
  durationMinutes: string;
  room: string;
  note: string;
};

function dateInput(value: AdminPrimitive | undefined) {
  return value ? String(value).slice(0, 10) : "";
}

function timeInput(value: string) {
  return value.slice(0, 5);
}

function examTypeLabel(value: number) {
  return examTypes.find(type => type.value === value)?.label ?? "Khác";
}

function emptyForm(startDate?: AdminPrimitive, endDate?: AdminPrimitive): ExamForm {
  return {
    examName: "",
    examType: "1",
    examDate: dateInput(endDate) || dateInput(startDate) || new Date().toISOString().slice(0, 10),
    startTime: "08:00",
    durationMinutes: "90",
    room: "",
    note: "",
  };
}

export function SectionExamsDialog({ row, semester, token, onClose, onChanged, onUnauthorized }: {
  row: AdminRecord;
  semester?: AdminRecord;
  token: string;
  onClose: () => void;
  onChanged: () => void;
  onUnauthorized: () => void;
}) {
  const sectionId = Number(row.sectionId);
  const [exams, setExams] = useState<AdminExam[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [revision, setRevision] = useState(0);
  const [formOpen, setFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<ExamForm>(() => emptyForm(semester?.startDate, semester?.endDate));

  useEffect(() => {
    let ignore = false;
    setLoading(true);
    setError("");
    listSectionExams(sectionId, token).then(result => {
      if (!ignore) setExams(result);
    }).catch(err => {
      if (ignore) return;
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không tải được lịch thi.");
    }).finally(() => {
      if (!ignore) setLoading(false);
    });
    return () => { ignore = true; };
  // The dialog keeps the selected section fixed while it is open.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sectionId, token, revision]);

  function beginCreate() {
    setEditingId(null);
    setForm(emptyForm(semester?.startDate, semester?.endDate));
    setError("");
    setSuccess("");
    setFormOpen(true);
  }

  function beginEdit(exam: AdminExam) {
    setEditingId(exam.examId);
    setForm({
      examName: exam.examName,
      examType: String(exam.examType),
      examDate: dateInput(exam.examDate),
      startTime: timeInput(exam.startTime),
      durationMinutes: String(exam.durationMinutes),
      room: exam.room ?? "",
      note: exam.note ?? "",
    });
    setError("");
    setSuccess("");
    setFormOpen(true);
  }

  function changeForm(key: keyof ExamForm, value: string) {
    setForm(current => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;

    const duration = Number(form.durationMinutes);
    if (!form.examName.trim()) {
      setError("Vui lòng nhập tên lịch thi.");
      return;
    }
    if (!Number.isFinite(duration) || duration <= 0) {
      setError("Thời lượng thi phải lớn hơn 0 phút.");
      return;
    }

    const data: SaveAdminExam = {
      examName: form.examName.trim(),
      examType: Number(form.examType),
      examDate: form.examDate,
      startTime: `${form.startTime}:00`,
      durationMinutes: duration,
      room: form.room.trim() || null,
      note: form.note.trim() || null,
    };

    setBusy(true);
    setError("");
    setSuccess("");
    try {
      await saveSectionExam(sectionId, editingId, token, data);
      setSuccess(editingId === null ? "Đã thêm lịch thi." : "Đã cập nhật lịch thi.");
      setFormOpen(false);
      setRevision(value => value + 1);
      onChanged();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không lưu được lịch thi.");
    } finally {
      setBusy(false);
    }
  }

  async function remove(exam: AdminExam) {
    if (busy || !window.confirm(`Xóa lịch thi "${exam.examName}"?`)) return;
    setBusy(true);
    setError("");
    setSuccess("");
    try {
      await deleteSectionExam(sectionId, exam.examId, token);
      setSuccess("Đã xóa lịch thi.");
      setRevision(value => value + 1);
      onChanged();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không xóa được lịch thi.");
    } finally {
      setBusy(false);
    }
  }

  const title = String(row.sectionCode ?? "Lớp học phần");

  return <ActionModal title={`Lịch thi: ${title}`} busy={busy} onClose={onClose}>
    <p className="schedule-dialog-description">
      {row.sectionName ? `${row.sectionName} · ` : ""}
      {String(semester?.semesterName ?? "Lịch thi được áp dụng cho lớp học phần này.")}
    </p>
    {error && <p className="notice error" role="alert">{error}</p>}
    {success && <p className="notice success" role="status">{success}</p>}

    <div className="schedule-toolbar">
      <span>{exams.length} lịch thi</span>
      <div className="row-actions">
        <button type="button" className="icon-button" title="Tải lại lịch thi" disabled={loading || busy} onClick={() => setRevision(value => value + 1)}><RefreshCw size={16} /></button>
        <button type="button" className="primary-button" disabled={busy} onClick={beginCreate}><Plus size={17} />Thêm lịch thi</button>
      </div>
    </div>

    {formOpen && <form className="access-form" onSubmit={event => void submit(event)}>
      <fieldset disabled={busy}>
        <div className="form-grid">
          <label className="schedule-note">Tên lịch thi<input required maxLength={200} value={form.examName} onChange={event => changeForm("examName", event.target.value)} placeholder="VD: Thi cuối kỳ" /></label>
          <label>Loại thi<select required value={form.examType} onChange={event => changeForm("examType", event.target.value)}>
            {examTypes.map(type => <option key={type.value} value={type.value}>{type.label}</option>)}
          </select></label>
          <label>Ngày thi<input type="date" required value={form.examDate} onChange={event => changeForm("examDate", event.target.value)} /></label>
          <label>Giờ bắt đầu<input type="time" required value={form.startTime} onChange={event => changeForm("startTime", event.target.value)} /></label>
          <label>Thời lượng phút<input type="number" required min={1} value={form.durationMinutes} onChange={event => changeForm("durationMinutes", event.target.value)} /></label>
          <label>Phòng<input maxLength={50} value={form.room} onChange={event => changeForm("room", event.target.value)} placeholder="VD: A203" /></label>
          <label className="schedule-note">Ghi chú<textarea maxLength={500} value={form.note} onChange={event => changeForm("note", event.target.value)} /></label>
        </div>
      </fieldset>
      <div className="dialog-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={() => setFormOpen(false)}><X size={16} />Hủy</button>
        <button type="submit" className="primary-button" disabled={busy}><Save size={16} />{busy ? "Đang lưu..." : "Lưu lịch thi"}</button>
      </div>
    </form>}

    <div className="table-wrap">
      <table className="schedule-table">
        <thead><tr><th>Tên lịch thi</th><th>Loại</th><th>Ngày giờ</th><th>Phòng</th><th>Ghi chú</th><th>Thao tác</th></tr></thead>
        <tbody>
          {loading ? <tr><td colSpan={6}>Đang tải lịch thi...</td></tr> : exams.map(exam => <tr key={exam.examId}>
            <td>{exam.examName}</td>
            <td>{examTypeLabel(exam.examType)}</td>
            <td>{dateInput(exam.examDate)} · {timeInput(exam.startTime)} · {exam.durationMinutes} phút</td>
            <td>{exam.room || "—"}</td>
            <td>{exam.note || "—"}</td>
            <td><div className="row-actions">
              <button type="button" className="icon-button small" title="Sửa lịch thi" disabled={busy} onClick={() => beginEdit(exam)}><Pencil size={15} /></button>
              <button type="button" className="icon-button small danger" title="Xóa lịch thi" disabled={busy} onClick={() => void remove(exam)}><Trash2 size={15} /></button>
            </div></td>
          </tr>)}
          {!loading && exams.length === 0 && <tr><td colSpan={6} className="schedule-empty"><AlarmClock size={18} />Chưa có lịch thi cho lớp này.</td></tr>}
        </tbody>
      </table>
    </div>
  </ActionModal>;
}
