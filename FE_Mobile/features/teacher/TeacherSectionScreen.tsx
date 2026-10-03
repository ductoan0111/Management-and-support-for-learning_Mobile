import { getSections, teacherRequest, type Row } from "@/api/teacher";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useLocalSearchParams } from "expo-router";
import { useCallback, useState } from "react";
import { Linking, Pressable, ScrollView, Text, TextInput, View } from "react-native";
import { Button, Dialog, IconButton, Page, Select, dateText, display, ui, type IconName } from "./components/TeacherUI";
import { RecordEditor } from "./components/RecordEditor";
import { TeacherGrades } from "./components/TeacherGrades";
import { TeacherSubmissions } from "./components/TeacherSubmissions";
import { resources, type ResourceKind } from "./config/resourceForms";
import { useTeacherData } from "./useTeacherData";

type Tab = "students" | ResourceKind | "grades";

const tabs: { value: Tab; label: string; icon: IconName }[] = [
  { value: "students", label: "Sinh viên", icon: "people-outline" },
  { value: "materials", label: "Tài liệu", icon: "document-text-outline" },
  { value: "assignments", label: "Bài tập", icon: "create-outline" },
  { value: "grades", label: "Bảng điểm", icon: "stats-chart-outline" },
  { value: "announcements", label: "Thông báo", icon: "notifications-outline" },
];

export default function TeacherSectionScreen({ initialTab = "students" }: { initialTab?: Tab }) {
  const params = useLocalSearchParams<{ sectionId?: string }>();
  const sections = useTeacherData(getSections);
  const [selected, setSelected] = useState(params.sectionId ?? "");
  const [tab, setTab] = useState<Tab>(initialTab);
  const sectionId = selected || String(sections.data?.[0]?.sectionId ?? "");
  const section = sections.data?.find(item => String(item.sectionId) === sectionId);

  return <Page title="Lớp học phần" loading={sections.loading} error={sections.error} refresh={() => void sections.refresh()}>
    <Select
      label="Lớp phụ trách"
      value={sectionId}
      onChange={setSelected}
      options={(sections.data ?? []).map(item => ({ value: String(item.sectionId), label: `${item.sectionCode} · ${item.courseName}` }))}
    />
    {section ? <>
      <Text style={ui.muted}>{section.semesterName} · {section.enrolledCount} sinh viên</Text>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 8 }}>
        {tabs.map(item => <Pressable
          key={item.value}
          accessibilityRole="tab"
          accessibilityState={{ selected: tab === item.value }}
          onPress={() => setTab(item.value)}
          style={[ui.row, { flexWrap: "nowrap", gap: 6, paddingHorizontal: 14, paddingVertical: 9, borderRadius: 999, backgroundColor: tab === item.value ? colors.primary : colors.surface, borderWidth: 1, borderColor: tab === item.value ? colors.primary : colors.border }]}
        >
          <Ionicons name={item.icon} size={16} color={tab === item.value ? "white" : colors.primary} />
          <Text style={{ color: tab === item.value ? "white" : colors.ink, fontWeight: "700", fontSize: 13 }}>{item.label}</Text>
        </Pressable>)}
      </ScrollView>
      {tab === "grades"
        ? <TeacherGrades key={sectionId} sectionId={sectionId} />
        : <SectionRecords key={`${sectionId}-${tab}`} sectionId={sectionId} tab={tab} />}
    </> : <Text style={ui.muted}>Chưa có lớp được phân công.</Text>}
  </Page>;
}

function SectionRecords({ sectionId, tab }: { sectionId: string; tab: Exclude<Tab, "grades"> }) {
  const path = `/sections/${sectionId}/${tab}`;
  const listPath = tab === "announcements" ? `/announcements?sectionId=${sectionId}` : path;
  const load = useCallback(() => teacherRequest<Row[]>(listPath), [listPath]);
  const state = useTeacherData(load);
  const config = tab === "students" ? null : resources[tab];
  const [search, setSearch] = useState("");
  const [editing, setEditing] = useState<Row | null>(null);
  const [deleting, setDeleting] = useState<Row | null>(null);
  const [assignment, setAssignment] = useState<Row | null>(null);
  const [detail, setDetail] = useState<Row | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  function itemPath(row: Row) {
    return `${tab === "announcements" ? "/announcements" : path}/${row[config!.id]}`;
  }

  async function remove() {
    if (!deleting || busy) return;
    setBusy(true);
    setError("");
    try {
      await teacherRequest(itemPath(deleting), "DELETE");
      setDeleting(null);
      setMessage("Đã xóa.");
      await state.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không xóa được dữ liệu.");
    } finally {
      setBusy(false);
    }
  }

  async function openLink(value: Row[string]) {
    const url = String(value ?? "");
    if (!/^https?:\/\//i.test(url)) {
      setError("Liên kết không hợp lệ.");
      return;
    }
    try {
      await Linking.openURL(url);
    } catch {
      setError("Không mở được liên kết.");
    }
  }

  async function openStudent(row: Row) {
    setBusy(true);
    setError("");
    try {
      setDetail(await teacherRequest<Row>(`${path}/${row.studentId}`));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không tải được hồ sơ.");
    } finally {
      setBusy(false);
    }
  }

  const rows = state.data?.filter(row =>
    `${row.title ?? ""} ${row.studentCode ?? ""} ${row.fullName ?? ""}`.toLocaleLowerCase().includes(search.toLocaleLowerCase())) ?? [];

  return <View style={{ gap: 14 }}>
    <View style={ui.row}>
      <TextInput accessibilityLabel="Tìm kiếm" placeholder="Tìm kiếm" style={[ui.input, ui.grow]} value={search} onChangeText={setSearch} />
      <IconButton label="Tải lại danh sách" icon="refresh" disabled={state.loading} onPress={() => void state.refresh()} />
      {config && <IconButton icon="add" label={`Thêm ${config.title}`} onPress={() => setEditing({ ...config.defaults })} />}
    </View>

    {!!(state.error || error) && <Text accessibilityRole="alert" style={ui.error}>{state.error || error}</Text>}
    {!!message && <Text accessibilityRole="alert" style={ui.success}>{message}</Text>}

    {state.loading ? <Text style={ui.muted}>Đang tải dữ liệu...</Text> : rows.map(row => <View key={String(row[config?.id ?? "studentId"])} style={ui.card}>
      <Text style={ui.heading}>{display(row.title ?? row.fullName)}</Text>
      {tab === "students" ? <>
        <Text style={ui.code}>{row.studentCode}</Text>
        <Text style={ui.text}>{row.email}</Text>
        <Text style={ui.muted}>Điểm tổng kết: {display(row.finalScore10)} · {display(row.letterGrade)}</Text>
        <Button label="Xem hồ sơ" icon="person-outline" disabled={busy} onPress={() => void openStudent(row)} />
      </> : <>
        <Text style={ui.text}>{display(row.description ?? row.content)}</Text>
        {tab === "assignments" && <>
          <Text style={ui.muted}>Hạn nộp: {dateText(row.dueAt)}</Text>
          <Text style={ui.muted}>{row.totalSubmissions} bài nộp · {row.gradedSubmissions} đã chấm · Điểm tối đa {row.maxScore}</Text>
          <Button label="Bài nộp và chấm điểm" icon="reader-outline" onPress={() => setAssignment(row)} />
        </>}
        <Text style={ui.code}>{(tab === "materials" ? row.isVisible : tab === "assignments" ? row.isPublished : row.isActive) ? "Đã công bố" : "Đang ẩn"}</Text>
        <View style={ui.row}>
          {["fileUrl", "externalUrl", "attachmentUrl"].filter(key => row[key]).map(key => <IconButton
            key={key}
            icon="open-outline"
            label={key === "externalUrl" ? "Mở liên kết" : "Mở tệp đính kèm"}
            onPress={() => void openLink(row[key])}
          />)}
          <View style={ui.grow} />
          <IconButton label="Sửa" icon="create-outline" onPress={() => setEditing(row)} />
          <IconButton label="Xóa" icon="trash-outline" onPress={() => { setError(""); setDeleting(row); }} />
        </View>
      </>}
    </View>)}

    {!state.loading && !state.error && !rows.length && <Text style={ui.muted}>Chưa có dữ liệu phù hợp.</Text>}

    {editing && config && <RecordEditor
      title={`${editing[config.id] ? "Sửa" : "Thêm"} ${config.title.toLowerCase()}`}
      fields={config.fields}
      initial={editing}
      onClose={() => setEditing(null)}
      save={payload => teacherRequest(editing[config.id] ? itemPath(editing) : path, editing[config.id] ? "PUT" : "POST", payload)}
      onSaved={() => { setEditing(null); setMessage("Đã lưu thay đổi."); void state.refresh(); }}
    />}

    {deleting && <Dialog title="Xác nhận xóa" busy={busy} onClose={() => setDeleting(null)}>
      <Text style={ui.text}>Xóa “{deleting.title}”?</Text>
      {!!error && <Text style={ui.error}>{error}</Text>}
      <Button label={busy ? "Đang xóa..." : "Xóa"} icon="trash-outline" disabled={busy} onPress={() => void remove()} />
    </Dialog>}

    {assignment && <TeacherSubmissions sectionId={sectionId} assignment={assignment} onClose={() => { setAssignment(null); void state.refresh(); }} />}

    {detail && <Dialog title="Hồ sơ sinh viên" onClose={() => setDetail(null)}>
      <ScrollView>
        {[
          ["fullName", "Họ tên"],
          ["studentCode", "Mã sinh viên"],
          ["email", "Email"],
          ["phone", "Điện thoại"],
          ["className", "Lớp hành chính"],
          ["majorName", "Ngành"],
          ["finalScore10", "Điểm tổng kết"],
          ["letterGrade", "Điểm chữ"],
        ].map(([key, label]) => <View key={key} style={{ marginBottom: 12 }}>
          <Text style={ui.muted}>{label}</Text>
          <Text style={ui.text}>{display(detail[key])}</Text>
        </View>)}
      </ScrollView>
    </Dialog>}
  </View>;
}
