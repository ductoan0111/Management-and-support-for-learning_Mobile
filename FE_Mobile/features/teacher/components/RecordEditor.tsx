import type { Row } from "@/api/teacher";
import { useState } from "react";
import { Switch, Text, TextInput, View } from "react-native";
import type { Field } from "../config/resourceForms";
import { Button, Dialog, Select, ui } from "./TeacherUI";

function dateInput(value: Row[string]) {
  if (!value) return "";
  const date = new Date(String(value));
  if (Number.isNaN(date.getTime())) return String(value);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")} ${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`;
}

export function RecordEditor({
  title,
  fields,
  initial,
  save,
  onClose,
  onSaved,
}: {
  title: string;
  fields: Field[];
  initial: Row;
  save: (payload: Row) => Promise<unknown>;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [form, setForm] = useState<Row>(() => Object.fromEntries(fields.map(field => [
    field.key,
    field.type === "datetime" ? dateInput(initial[field.key]) : initial[field.key] ?? "",
  ])));
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function submit() {
    if (busy) return;
    setError("");
    try {
      const payload: Row = {};
      for (const field of fields) {
        const value = form[field.key];
        const text = String(value ?? "").trim();
        if (field.required && !text) throw new Error(`Vui lòng nhập ${field.label.toLowerCase()}.`);
        if (field.type === "switch") payload[field.key] = Boolean(value);
        else if (!text) payload[field.key] = null;
        else if (field.type === "number" || field.type === "select") {
          const number = Number(text);
          if (!Number.isFinite(number) || (field.min !== undefined && number < field.min) || (field.max !== undefined && number > field.max)) {
            throw new Error(`${field.label} không hợp lệ.`);
          }
          payload[field.key] = number;
        } else if (field.type === "datetime" || field.type === "date") {
          const format = field.type === "date" ? /^\d{4}-\d{2}-\d{2}$/ : /^\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}$/;
          const date = new Date(text.replace(" ", "T"));
          const parts = text.slice(0, 10).split("-").map(Number);
          const calendar = new Date(parts[0], parts[1] - 1, parts[2]);
          if (!format.test(text) || Number.isNaN(date.getTime()) || calendar.getFullYear() !== parts[0] || calendar.getMonth() !== parts[1] - 1 || calendar.getDate() !== parts[2]) {
            throw new Error(`${field.label} không hợp lệ.`);
          }
          payload[field.key] = field.type === "date" ? text : `${text.replace(" ", "T")}:00`;
        } else {
          if (field.type === "url" && !/^https?:\/\/\S+$/i.test(text)) throw new Error(`${field.label} phải bắt đầu bằng http:// hoặc https://.`);
          payload[field.key] = text;
        }
      }
      if (payload.openAt && payload.dueAt && String(payload.openAt) >= String(payload.dueAt)) throw new Error("Hạn nộp phải sau thời gian mở bài.");
      if (payload.expiresAt && new Date(String(payload.expiresAt)) < new Date(String(initial.publishedAt || new Date().toISOString()))) {
        throw new Error("Ngày hết hạn phải sau ngày công bố.");
      }
      setBusy(true);
      await save(payload);
      onSaved();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không lưu được dữ liệu.");
    } finally {
      setBusy(false);
    }
  }

  return <Dialog title={title} onClose={onClose} busy={busy}>
    {!!error && <Text accessibilityRole="alert" style={ui.error}>{error}</Text>}
    {fields.map(field => <View key={field.key} style={{ gap: 6 }}>
      {field.type === "switch" ? <View style={ui.row}>
        <Text style={[ui.text, ui.grow]}>{field.label}</Text>
        <Switch disabled={busy} value={Boolean(form[field.key])} onValueChange={value => setForm({ ...form, [field.key]: value })} />
      </View> : field.type === "select" ? <Select
        label={field.label}
        options={field.options ?? []}
        value={String(form[field.key])}
        onChange={value => setForm({ ...form, [field.key]: value })}
      /> : <>
        <Text style={ui.muted}>{field.label}{field.required ? " *" : ""}</Text>
        <TextInput
          accessibilityLabel={field.label}
          editable={!busy}
          style={[ui.input, field.type === "multiline" && { minHeight: 100, textAlignVertical: "top" }]}
          multiline={field.type === "multiline"}
          autoCapitalize={field.type === "url" ? "none" : "sentences"}
          keyboardType={field.type === "number" ? "decimal-pad" : "default"}
          value={String(form[field.key] ?? "")}
          onChangeText={value => setForm({ ...form, [field.key]: value })}
        />
      </>}
    </View>)}
    <Button label={busy ? "Đang lưu..." : "Lưu"} icon="save-outline" onPress={() => void submit()} disabled={busy} />
  </Dialog>;
}
