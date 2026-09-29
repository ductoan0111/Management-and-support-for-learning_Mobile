import { useState, type FormEvent } from "react";
import { Save } from "lucide-react";
import { AdminApiError, resetAdminUserPassword, setAdminUserRole } from "../../api/admin";
import type { AdminRecord } from "../../types/admin";
import { ActionModal } from "./ActionModal";

export function UserAccessDialog({ row, mode, roles, token, onClose, onSaved, onUnauthorized }: {
  row: AdminRecord; mode: "role" | "password"; roles: AdminRecord[]; token: string;
  onClose: () => void; onSaved: () => void; onUnauthorized: () => void;
}) {
  const [roleId, setRoleId] = useState(String(row.roleId ?? ""));
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  async function submit(event: FormEvent) {
    event.preventDefault();
    if (busy) return;
    if (mode === "password" && password !== confirm) { setError("Mật khẩu xác nhận không khớp."); return; }
    setBusy(true); setError("");
    try {
      if (mode === "role") await setAdminUserRole(token, Number(row.userId), Number(roleId));
      else await resetAdminUserPassword(token, Number(row.userId), password);
      onSaved();
    } catch (err) {
      if (err instanceof AdminApiError && err.status === 401) onUnauthorized();
      else setError(err instanceof Error ? err.message : "Không thể lưu thay đổi.");
    } finally { setBusy(false); }
  }
  return <ActionModal title={`${mode === "role" ? "Đổi vai trò" : "Đổi mật khẩu"}: ${row.username}`} busy={busy} onClose={onClose}>
    <form onSubmit={submit} className="access-form">
      {error && <p className="notice error" role="alert">{error}</p>}
      <fieldset disabled={busy}>
        {mode === "role" ? <label>Vai trò<select required value={roleId} onChange={e => setRoleId(e.target.value)}>
          <option value="">Chọn vai trò</option>
          {roles.map(role => <option key={String(role.roleId)} value={String(role.roleId)}>{role.roleName} ({role.roleCode})</option>)}
        </select></label> : <>
          <label>Mật khẩu mới<input type="password" autoComplete="new-password" required minLength={12} maxLength={128} value={password} onChange={e => setPassword(e.target.value)} /></label>
          <label>Xác nhận mật khẩu<input type="password" autoComplete="new-password" required minLength={12} maxLength={128} value={confirm} onChange={e => setConfirm(e.target.value)} /></label>
        </>}
      </fieldset>
      <div className="dialog-actions"><button className="secondary-button" type="button" disabled={busy} onClick={onClose}>Hủy</button>
        <button className="primary-button" disabled={busy} type="submit"><Save size={18} />{busy ? "Đang lưu..." : "Lưu"}</button></div>
    </form>
  </ActionModal>;
}
