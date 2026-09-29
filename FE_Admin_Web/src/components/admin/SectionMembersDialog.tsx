import { useEffect, useState, type FormEvent } from "react";
import { Pencil, Trash2, Save, RefreshCw } from "lucide-react";
import { AdminApiError, deleteAdminResource, listAdminResource, updateAdminResource } from "../../api/admin";
import type { AdminRecord, PagedResult } from "../../types/admin";
import { ActionModal } from "./ActionModal";

export function SectionMembersDialog({ row, kind, token, onClose, onChanged, onUnauthorized }: {
  row: AdminRecord; kind: "teachers" | "students"; token: string;
  onClose: () => void; onChanged: () => void; onUnauthorized: () => void;
}) {
  const teachers = kind === "teachers";
  const idKey = teachers ? "teacherId" : "studentId";
  const codeKey = teachers ? "teacherCode" : "studentCode";
  const path = `/api/admin/course-sections/${row.sectionId}/${kind}`;
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult | null>(null);
  const [candidates, setCandidates] = useState<AdminRecord[]>([]);
  const [selected, setSelected] = useState("");
  const [primary, setPrimary] = useState(false);
  const [status, setStatus] = useState("1");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    let ignore = false;
    setLoading(true); setError("");
    async function load() {
      const members = await listAdminResource(path, token, { page, pageSize: 20 });
      const options: AdminRecord[] = [];
      // Load every page so assignment is not restricted to the first 100 records.
      for (let index = 1; ; index++) {
        const batch = await listAdminResource(`/api/admin/${kind}`, token, { page: index, pageSize: 100 });
        if (ignore) return;
        options.push(...batch.items);
        if (index >= batch.totalPages) break;
      }
      if (!ignore) { setResult(members); setCandidates(options); }
    }
    void load().catch(err => {
      if (ignore) return;
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không tải được danh sách.");
    }).finally(() => { if (!ignore) setLoading(false); });
    return () => { ignore = true; };
  // The parent callbacks do not change the resource being loaded.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [path, kind, token, page, revision]);

  async function mutate(id: number, remove: boolean) {
    if (busy) return;
    if (remove && !window.confirm(teachers ? "Gỡ giảng viên khỏi lớp học phần?" : "Hủy đăng ký học phần của sinh viên?")) return;
    setBusy(true); setError(""); setSuccess("");
    try {
      if (remove) await deleteAdminResource(path, token, id);
      else await updateAdminResource(path, token, id, teachers ? { isPrimary: primary } : { status: Number(status) });
      setSelected(""); setPrimary(false); setStatus("1");
      setSuccess(remove ? "Đã cập nhật danh sách." : "Đã lưu thay đổi.");
      if (remove && result?.items.length === 1 && page > 1) setPage(page - 1);
      else setRevision(value => value + 1);
      onChanged();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không thể lưu thay đổi.");
    } finally { setBusy(false); }
  }
  function submit(event: FormEvent) { event.preventDefault(); if (selected) void mutate(Number(selected), false); }
  const statuses: Record<string, string> = { "0": "Đã hủy", "1": "Đang học", "2": "Hoàn thành" };
  return <ActionModal title={`${teachers ? "Giảng viên" : "Sinh viên"}: ${row.sectionCode}`} busy={busy} onClose={onClose}>
    {error && <p className="notice error" role="alert">{error}</p>}
    {success && <p className="notice success" role="status">{success}</p>}
    <form onSubmit={submit} className="access-form">
      <fieldset disabled={busy || loading}>
        <label>{teachers ? "Giảng viên" : "Sinh viên"}<select required value={selected} onChange={e => setSelected(e.target.value)}>
          <option value="">Chọn {teachers ? "giảng viên" : "sinh viên"}</option>
          {candidates.map(person => <option key={String(person[idKey])} value={String(person[idKey])}>{person[codeKey]} - {person.fullName || `#${person[idKey]}`}</option>)}
        </select></label>
        {teachers ? <label className="checkbox-field"><input type="checkbox" checked={primary} onChange={e => setPrimary(e.target.checked)} />Giảng viên chính</label> :
          <label>Trạng thái<select value={status} onChange={e => setStatus(e.target.value)}>{Object.entries(statuses).map(([value, label]) => <option key={value} value={value}>{label}</option>)}</select></label>}
      </fieldset>
      <div className="dialog-actions"><button type="submit" className="primary-button" disabled={busy || loading || !selected}><Save size={18} />{busy ? "Đang lưu..." : "Lưu"}</button></div>
    </form>
    <div className="table-wrap"><table><thead><tr><th>Mã</th><th>Họ tên</th><th>{teachers ? "Phân công" : "Trạng thái"}</th><th>Thao tác</th></tr></thead>
      <tbody>{loading ? <tr><td colSpan={4}>Đang tải dữ liệu...</td></tr> : result?.items.map(member => <tr key={String(member[idKey])}>
        <td>{member[codeKey]}</td><td>{member.fullName}</td><td>{teachers ? (member.isPrimary ? "Giảng viên chính" : "Giảng viên phụ") : statuses[String(member.status)]}</td>
        <td><div className="row-actions"><button type="button" className="icon-button small" title="Sửa phân công" disabled={busy} onClick={() => { setSelected(String(member[idKey])); setPrimary(Boolean(member.isPrimary)); setStatus(String(member.status ?? 1)); setSuccess(""); }}><Pencil size={16} /></button>
          <button type="button" className="icon-button small danger" title={teachers ? "Gỡ phân công" : "Hủy đăng ký"} disabled={busy || (!teachers && member.status === 0)} onClick={() => void mutate(Number(member[idKey]), true)}><Trash2 size={16} /></button></div></td>
      </tr>)}{!loading && result?.items.length === 0 && <tr><td colSpan={4}>Chưa có dữ liệu.</td></tr>}</tbody></table></div>
    <footer className="pagination"><span>{result?.totalCount ?? 0} bản ghi · Trang {page}/{Math.max(1, result?.totalPages ?? 1)}</span><div>
      <button className="icon-button" type="button" title="Tải lại" disabled={busy || loading} onClick={() => setRevision(value => value + 1)}><RefreshCw size={16} /></button>
      <button className="secondary-button" disabled={busy || loading || page <= 1} onClick={() => setPage(page - 1)}>Trước</button>
      <button className="secondary-button" disabled={busy || loading || page >= (result?.totalPages ?? 1)} onClick={() => setPage(page + 1)}>Sau</button>
    </div></footer>
  </ActionModal>;
}
