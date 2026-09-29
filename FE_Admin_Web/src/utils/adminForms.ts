import type {
  AdminFormState,
  AdminRecord,
  ResourceConfig,
  ResourceField,
} from "../types/admin";

export function getVisibleFields(
  resource: ResourceConfig,
  mode: "create" | "edit",
) {
  return resource.fields.filter((field) => {
    if (mode === "create" && field.editOnly) return false;
    if (mode === "edit" && field.createOnly) return false;
    return true;
  });
}

function coerceValue(field: ResourceField, value: AdminFormState[string]) {
  if (value === "" || value === null || value === undefined) {
    return field.optional ? null : "";
  }

  if (field.kind === "checkbox" || field.valueType === "boolean") {
    return value === true || value === "true";
  }

  if (field.kind === "number" || field.valueType === "number") {
    return Number(value);
  }

  return typeof value === "string" ? value.trim() : value;
}

export function hasMissingRequiredField(
  field: ResourceField,
  value: AdminFormState[string],
) {
  if (!field.required) return false;
  return value === "" || value === null || value === undefined;
}

export function makePayload(
  resource: ResourceConfig,
  form: AdminFormState,
  mode: "create" | "edit",
) {
  return getVisibleFields(resource, mode).reduce<AdminRecord>((payload, field) => {
    payload[field.key] = coerceValue(field, form[field.key]);
    return payload;
  }, {});
}

export function rowToForm(resource: ResourceConfig, row: AdminRecord) {
  const form: AdminFormState = { ...resource.defaultValues };
  resource.fields.forEach((field) => {
    if (field.key in row) {
      form[field.key] =
        row[field.key] === undefined ? "" : (row[field.key] as AdminFormState[string]);
    }
  });
  return form;
}
