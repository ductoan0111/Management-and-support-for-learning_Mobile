import type {
  AdminPrimitive,
  AdminRecord,
  LookupKey,
  LookupState,
} from "../types/admin";

export function valueText(value: AdminPrimitive) {
  return value === null || value === undefined || value === "" ? "" : String(value);
}

export function labelFromRecord(record: AdminRecord, keys: string[]) {
  const text = keys.map((key) => valueText(record[key])).filter(Boolean).join(" - ");
  return text || `#${valueText(Object.values(record)[0])}`;
}

export function optionsFromRecords(
  records: AdminRecord[],
  valueKey: string,
  labelKeys: string[],
) {
  return records
    .filter((record) => record[valueKey] !== null && record[valueKey] !== undefined)
    .map((record) => ({
      label: labelFromRecord(record, labelKeys),
      value: record[valueKey] as string | number,
    }));
}

export function labelFromLookup(
  lookups: LookupState,
  key: LookupKey,
  idKey: string,
  id: AdminPrimitive,
  labelKeys: string[],
) {
  const record = lookups[key].find((item) => String(item[idKey]) === String(id));
  return record ? labelFromRecord(record, labelKeys) : valueText(id) || "-";
}

export function formatCell(value: AdminPrimitive) {
  if (value === null || value === undefined || value === "") return "-";
  if (typeof value === "boolean") return value ? "Có" : "Không";
  return String(value);
}
