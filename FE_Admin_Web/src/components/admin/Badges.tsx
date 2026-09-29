import type { AdminPrimitive } from "../../types/admin";

export function BooleanBadge({ value }: { value: AdminPrimitive }) {
  const active = value === true || value === "true" || value === 1;
  return (
    <span className={`badge ${active ? "success" : "muted"}`}>
      {active ? "Hoạt động" : "Ngưng"}
    </span>
  );
}

export function CurrentBadge({ value }: { value: AdminPrimitive }) {
  const current = value === true || value === "true" || value === 1;
  return (
    <span className={`badge ${current ? "info" : "muted"}`}>
      {current ? "Hiện tại" : "Khác"}
    </span>
  );
}

export function StatusBadge({
  labels,
  value,
}: {
  labels: Record<string, string>;
  value: AdminPrimitive;
}) {
  const key = String(value ?? "");
  const tone = key === "1" ? "success" : key === "2" ? "info" : "warning";

  return <span className={`badge ${tone}`}>{labels[key] ?? "-"}</span>;
}
