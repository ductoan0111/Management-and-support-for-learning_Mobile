import { Loader2, Save, X } from "lucide-react";
import type { FormEvent } from "react";
import type {
  AdminFormState,
  DialogState,
  LookupState,
  ResourceConfig,
} from "../../types/admin";
import { getVisibleFields } from "../../utils/adminForms";
import { ResourceFormField } from "./ResourceFormField";

type ResourceDialogProps = {
  dialog: DialogState | null;
  form: AdminFormState;
  isSaving: boolean;
  lookups: LookupState;
  onChange: (key: string, value: AdminFormState[string]) => void;
  onClose: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  resource: ResourceConfig | null;
};

export function ResourceDialog({
  dialog,
  form,
  isSaving,
  lookups,
  onChange,
  onClose,
  onSubmit,
  resource,
}: ResourceDialogProps) {
  if (!dialog || !resource) return null;

  return (
    <div className="dialog-backdrop" role="presentation">
      <form className="dialog-card" onSubmit={onSubmit}>
        <div className="dialog-header">
          <div>
            <p className="eyebrow">{dialog.mode === "create" ? "Thêm mới" : "Cập nhật"}</p>
            <h2>{resource.title}</h2>
          </div>
          <button className="icon-button" onClick={onClose} title="Đóng" type="button">
            <X size={18} aria-hidden="true" />
          </button>
        </div>

        <div className="form-grid">
          {getVisibleFields(resource, dialog.mode).map((field) => (
            <ResourceFormField
              disabled={dialog.mode === "edit" && Boolean(field.disabledOnEdit)}
              field={field}
              key={field.key}
              lookups={lookups}
              onChange={onChange}
              value={form[field.key]}
            />
          ))}
        </div>

        <div className="dialog-actions">
          <button className="secondary-button" onClick={onClose} type="button">
            Hủy
          </button>
          <button className="primary-button" disabled={isSaving} type="submit">
            {isSaving ? (
              <Loader2 className="spin" size={18} aria-hidden="true" />
            ) : (
              <Save size={18} aria-hidden="true" />
            )}
            <span>Lưu</span>
          </button>
        </div>
      </form>
    </div>
  );
}
