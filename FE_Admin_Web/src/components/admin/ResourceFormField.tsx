import type { AdminFormState, LookupState, ResourceField } from "../../types/admin";
import { valueText } from "../../utils/adminDisplay";

type ResourceFormFieldProps = {
  disabled: boolean;
  field: ResourceField;
  lookups: LookupState;
  onChange: (key: string, value: AdminFormState[string]) => void;
  value: AdminFormState[string];
};

export function ResourceFormField({
  disabled,
  field,
  lookups,
  onChange,
  value,
}: ResourceFormFieldProps) {
  if (field.kind === "checkbox") {
    return (
      <label className="checkbox-field">
        <input
          checked={Boolean(value)}
          disabled={disabled}
          onChange={(event) => onChange(field.key, event.target.checked)}
          type="checkbox"
        />
        <span>{field.label}</span>
      </label>
    );
  }

  if (field.kind === "textarea") {
    return (
      <label>
        {field.label}
        <textarea
          disabled={disabled}
          onChange={(event) => onChange(field.key, event.target.value)}
          placeholder={field.placeholder}
          value={valueText(value)}
        />
      </label>
    );
  }

  if (field.kind === "select") {
    return (
      <label>
        {field.label}
        <select
          disabled={disabled}
          onChange={(event) => {
            const raw = event.target.value;
            if (raw === "") {
              onChange(field.key, "");
            } else if (field.valueType === "number") {
              onChange(field.key, Number(raw));
            } else if (field.valueType === "boolean") {
              onChange(field.key, raw === "true");
            } else {
              onChange(field.key, raw);
            }
          }}
          value={valueText(value)}
        >
          <option value="">Chọn...</option>
          {(field.getOptions?.(lookups) ?? []).map((option) => (
            <option key={String(option.value)} value={String(option.value)}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    );
  }

  return (
    <label>
      {field.label}
      <input
        disabled={disabled}
        max={field.max}
        min={field.min}
        onChange={(event) => {
          const raw = event.target.value;
          onChange(field.key, field.kind === "number" && raw !== "" ? Number(raw) : raw);
        }}
        placeholder={field.placeholder}
        type={field.kind}
        value={valueText(value)}
      />
    </label>
  );
}
