import { useEffect, useState, type FormEvent } from "react";
import { CalendarDays, Pencil, Plus, RefreshCw, Save, Trash2, X } from "lucide-react";
import {
  AdminApiError,
  deleteSectionSchedule,
  listSectionSchedules,
  saveSectionSchedule,
} from "../../api/admin";
import type { AdminClassSchedule, AdminPrimitive, AdminRecord, SaveAdminClassSchedule } from "../../types/admin";
import { ActionModal } from "./ActionModal";

const days = [
  { value: 2, label: "Thứ 2" },
  { value: 3, label: "Thứ 3" },
  { value: 4, label: "Thứ 4" },
  { value: 5, label: "Thứ 5" },
  { value: 6, label: "Thứ 6" },
  { value: 7, label: "Thứ 7" },
  { value: 8, label: "Chủ nhật" },
];

type ScheduleForm = {
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  room: string;
  building: string;
  effectiveFrom: string;
  effectiveTo: string;
  note: string;
};

function dateInput(value: AdminPrimitive | undefined) {
  return value ? String(value).slice(0, 10) : "";
}

function emptyForm(startDate?: AdminPrimitive, endDate?: AdminPrimitive): ScheduleForm {
  return {
    dayOfWeek: "2",
    startTime: "",
    endTime: "",
    room: "",
    building: "",
    effectiveFrom: dateInput(startDate),
    effectiveTo: dateInput(endDate),
    note: "",
  };
}

function timeInput(value: string) {
  return value.slice(0, 5);
}

function dayLabel(value: number) {
  return days.find(day => day.value === value)?.label ?? `Thứ ${value}`;
}

export function SectionSchedulesDialog({ row, semester, token, onClose, onChanged, onUnauthorized }: {
  row: AdminRecord;
  semester?: AdminRecord;
  token: string;
  onClose: () => void;
  onChanged: () => void;
  onUnauthorized: () => void;
}) {
  const sectionId = Number(row.sectionId);
  const [schedules, setSchedules] = useState<AdminClassSchedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [revision, setRevision] = useState(0);
  const [formOpen, setFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<ScheduleForm>(() => emptyForm(semester?.startDate, semester?.endDate));

  useEffect(() => {
    let ignore = false;
    setLoading(true);
    setError("");
    listSectionSchedules(sectionId, token).then(result => {
      if (!ignore) setSchedules(result);
    }).catch(err => {
      if (ignore) return;
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không tải được lịch học.");
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

  function beginEdit(schedule: AdminClassSchedule) {
    setEditingId(schedule.scheduleId);
    setForm({
      dayOfWeek: String(schedule.dayOfWeek),
      startTime: timeInput(schedule.startTime),
      endTime: timeInput(schedule.endTime),
      room: schedule.room ?? "",
      building: schedule.building ?? "",
      effectiveFrom: dateInput(schedule.effectiveFrom),
      effectiveTo: dateInput(schedule.effectiveTo),
      note: schedule.note ?? "",
    });
    setError("");
    setSuccess("");
    setFormOpen(true);
  }

  function changeForm(key: keyof ScheduleForm, value: string) {
    setForm(current => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    if (form.endTime <= form.startTime) {
      setError("Giờ kết thúc phải sau giờ bắt đầu.");
      return;
    }
    if (form.effectiveTo < form.effectiveFrom) {
      setError("Ngày kết thúc phải từ ngày bắt đầu trở đi.");
      return;
    }

    const data: SaveAdminClassSchedule = {
      dayOfWeek: Number(form.dayOfWeek),
      startTime: `${form.startTime}:00`,
      endTime: `${form.endTime}:00`,
      room: form.room.trim() || null,
      building: form.building.trim() || null,
      effectiveFrom: form.effectiveFrom,
      effectiveTo: form.effectiveTo,
      note: form.note.trim() || null,
    };

    setBusy(true);
    setError("");
    setSuccess("");
    try {
      await saveSectionSchedule(sectionId, editingId, token, data);
      setSuccess(editingId === null ? "Đã thêm buổi học." : "Đã cập nhật buổi học.");
      setFormOpen(false);
      setRevision(value => value + 1);
      onChanged();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không lưu được lịch học.");
    } finally {
      setBusy(false);
    }
  }

  async function remove(schedule: AdminClassSchedule) {
    if (busy || !window.confirm(`Xóa lịch ${dayLabel(schedule.dayOfWeek)} ${timeInput(schedule.startTime)}–${timeInput(schedule.endTime)}?`)) return;
    setBusy(true);
    setError("");
    setSuccess("");
    try {
      await deleteSectionSchedule(sectionId, schedule.scheduleId, token);
      setSuccess("Đã xóa buổi học.");
      setRevision(value => value + 1);
      onChanged();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không xóa được lịch học.");
    } finally {
      setBusy(false);
    }
  }

  const title = String(row.sectionCode ?? "Lớp học phần");

  return <ActionModal title={`Lịch học: ${title}`} busy={busy} onClose={onClose}>
    <p className="schedule-dialog-description">
      {row.sectionName ? `${row.sectionName} · ` : ""}
      {String(semester?.semesterName ?? "Lịch được áp dụng cho lớp học phần này.")}
    </p>
    {error && <p className="notice error" role="alert">{error}</p>}
    {success && <p className="notice success" role="status">{success}</p>}

    <div className="schedule-toolbar">
      <span>{schedules.length} buổi học</span>
      <div className="row-actions">
        <button type="button" className="icon-button" title="Tải lại lịch" disabled={loading || busy} onClick={() => setRevision(value => value + 1)}><RefreshCw size={16} /></button>
        <button type="button" className="primary-button" disabled={busy} onClick={beginCreate}><Plus size={17} />Thêm buổi học</button>
      </div>
    </div>

    {formOpen && <form className="access-form" onSubmit={event => void submit(event)}>
      <fieldset disabled={busy}>
        <div className="form-grid">
          <label>Thứ<select required value={form.dayOfWeek} onChange={event => changeForm("dayOfWeek", event.target.value)}>
            {days.map(day => <option key={day.value} value={day.value}>{day.label}</option>)}
          </select></label>
          <label>Phòng<input maxLength={50} value={form.room} onChange={event => changeForm("room", event.target.value)} placeholder="VD: A203" /></label>
          <label>Giờ bắt đầu<input type="time" required value={form.startTime} onChange={event => changeForm("startTime", event.target.value)} /></label>
          <label>Giờ kết thúc<input type="time" required value={form.endTime} onChange={event => changeForm("endTime", event.target.value)} /></label>
          <label>Tòa nhà<input maxLength={100} value={form.building} onChange={event => changeForm("building", event.target.value)} placeholder="VD: Nhà A" /></label>
          <label>Áp dụng từ<input type="date" required value={form.effectiveFrom} onChange={event => changeForm("effectiveFrom", event.target.value)} /></label>
          <label>Áp dụng đến<input type="date" required value={form.effectiveTo} onChange={event => changeForm("effectiveTo", event.target.value)} /></label>
          <label className="schedule-note">Ghi chú<textarea maxLength={500} value={form.note} onChange={event => changeForm("note", event.target.value)} /></label>
        </div>
      </fieldset>
      <div className="dialog-actions">
        <button type="button" className="secondary-button" disabled={busy} onClick={() => setFormOpen(false)}><X size={16} />Hủy</button>
        <button type="submit" className="primary-button" disabled={busy}><Save size={16} />{busy ? "Đang lưu..." : "Lưu lịch"}</button>
      </div>
    </form>}

    <div className="table-wrap">
      <table className="schedule-table">
        <thead><tr><th>Thứ</th><th>Thời gian</th><th>Địa điểm</th><th>Thời hạn</th><th>Ghi chú</th><th>Thao tác</th></tr></thead>
        <tbody>
          {loading ? <tr><td colSpan={6}>Đang tải lịch học...</td></tr> : schedules.map(schedule => <tr key={schedule.scheduleId}>
            <td>{dayLabel(schedule.dayOfWeek)}</td>
            <td>{timeInput(schedule.startTime)}–{timeInput(schedule.endTime)}</td>
            <td>{[schedule.room, schedule.building].filter(Boolean).join(" · ") || "—"}</td>
            <td>{dateInput(schedule.effectiveFrom)} – {dateInput(schedule.effectiveTo)}</td>
            <td>{schedule.note || "—"}</td>
            <td><div className="row-actions">
              <button type="button" className="icon-button small" title="Sửa lịch" disabled={busy} onClick={() => beginEdit(schedule)}><Pencil size={15} /></button>
              <button type="button" className="icon-button small danger" title="Xóa lịch" disabled={busy} onClick={() => void remove(schedule)}><Trash2 size={15} /></button>
            </div></td>
          </tr>)}
          {!loading && schedules.length === 0 && <tr><td colSpan={6} className="schedule-empty"><CalendarDays size={18} />Chưa có lịch học cho lớp này.</td></tr>}
        </tbody>
      </table>
    </div>
  </ActionModal>;
}
