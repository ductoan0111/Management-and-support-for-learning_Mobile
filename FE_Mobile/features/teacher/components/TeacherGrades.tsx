import { teacherRequest, type Row } from "@/api/teacher";
import { colors } from "@/constants/theme";
import { useCallback, useMemo, useState } from "react";
import { Pressable, StyleSheet, Text, TextInput, View } from "react-native";
import { useTeacherData } from "../useTeacherData";
import { Button, Dialog, IconButton, Select, display, ui } from "./TeacherUI";
import { RecordEditor } from "./RecordEditor";

type Mode = "scores" | "components" | "summary";

type GradeComponent = {
  gradeComponentId: number;
  sectionId: number;
  componentName: string;
  weightPercent: number;
  maxScore: number;
  displayOrder: number;
};

type Student = {
  studentId: number;
  studentCode: string;
  fullName: string;
};

type StudentGrade = {
  studentId: number;
  gradeComponentId: number;
  score: number | null;
  note: string | null;
};

type FinalGrade = {
  studentId: number;
  studentCode: string;
  fullName: string;
  calculatedScore10: number;
  finalScore10: number | null;
  letterGrade: string | null;
  gradedComponents: number;
  totalComponents: number;
};

type GradeSummary = {
  sectionId: number;
  componentCount: number;
  totalWeightPercent: number;
  isWeightComplete: boolean;
  studentCount: number;
  finalizedCount: number;
  students: FinalGrade[];
};

type GradeData = {
  components: GradeComponent[];
  grades: StudentGrade[];
  students: Student[];
  summary: GradeSummary;
};

const modes: { value: Mode; label: string }[] = [
  { value: "scores", label: "Nhập điểm" },
  { value: "components", label: "Thành phần" },
  { value: "summary", label: "Tổng kết" },
];

const componentFields = [
  { key: "componentName", label: "Tên thành phần", type: "text" as const, required: true },
  { key: "weightPercent", label: "Trọng số (%)", type: "number" as const, min: 0.01, max: 100, required: true },
  { key: "maxScore", label: "Điểm tối đa", type: "number" as const, min: 0.01, max: 100, required: true },
  { key: "displayOrder", label: "Thứ tự hiển thị", type: "number" as const, min: 1, required: true },
];

export function TeacherGrades({ sectionId }: { sectionId: string }) {
  const path = `/sections/${sectionId}`;
  const load = useCallback(async () => {
    const [components, grades, students, summary] = await Promise.all([
      teacherRequest<GradeComponent[]>(`${path}/grade-components`),
      teacherRequest<StudentGrade[]>(`${path}/grades`),
      teacherRequest<Student[]>(`${path}/students`),
      teacherRequest<GradeSummary>(`${path}/grade-summary`),
    ]);
    return { components, grades, students, summary };
  }, [path]);

  const state = useTeacherData<GradeData>(load);
  const [mode, setMode] = useState<Mode>("scores");
  const [componentId, setComponentId] = useState("");
  const [search, setSearch] = useState("");
  const [editingGrade, setEditingGrade] = useState<Row | null>(null);
  const [editingComponent, setEditingComponent] = useState<Row | null>(null);
  const [deletingComponent, setDeletingComponent] = useState<GradeComponent | null>(null);
  const [busy, setBusy] = useState(false);
  const [localError, setLocalError] = useState("");
  const [message, setMessage] = useState("");

  const components = useMemo(() => state.data?.components ?? [], [state.data?.components]);
  const selected = useMemo(() => {
    if (components.some(component => String(component.gradeComponentId) === componentId)) return componentId;
    return String(components[0]?.gradeComponentId ?? "");
  }, [componentId, components]);
  const component = components.find(item => String(item.gradeComponentId) === selected);
  const filteredStudents = (state.data?.students ?? []).filter(student =>
    `${student.studentCode} ${student.fullName}`.toLocaleLowerCase().includes(search.toLocaleLowerCase()));
  const totalWeight = Number(state.data?.summary.totalWeightPercent ?? 0);

  async function removeComponent() {
    if (!deletingComponent || busy) return;
    setBusy(true); setLocalError(""); setMessage("");
    try {
      await teacherRequest(`${path}/grade-components/${deletingComponent.gradeComponentId}`, "DELETE");
      setDeletingComponent(null);
      setMessage("Đã xóa thành phần điểm.");
      await state.refresh();
    } catch (error) {
      setLocalError(error instanceof Error ? error.message : "Không xóa được thành phần điểm.");
    } finally {
      setBusy(false);
    }
  }

  async function finalizeGrades() {
    if (busy) return;
    setBusy(true); setLocalError(""); setMessage("");
    try {
      await teacherRequest<GradeSummary>(`${path}/grades/finalize`, "POST");
      setMessage("Đã cập nhật điểm tổng kết cho lớp.");
      await state.refresh();
    } catch (error) {
      setLocalError(error instanceof Error ? error.message : "Không cập nhật được điểm tổng kết.");
    } finally {
      setBusy(false);
    }
  }

  return <View style={{ gap: 14 }}>
    <View style={ui.row}>
      <Text style={[ui.heading, ui.grow]}>Bảng điểm</Text>
      <IconButton label="Tải lại điểm" icon="refresh" onPress={() => void state.refresh()} disabled={state.loading} />
    </View>

    <View style={styles.segment}>
      {modes.map(item => <Pressable key={item.value} accessibilityRole="button" onPress={() => { setMode(item.value); setLocalError(""); }} style={[styles.segmentItem, mode === item.value && styles.segmentActive]}>
        <Text style={[styles.segmentText, mode === item.value && styles.segmentTextActive]}>{item.label}</Text>
      </Pressable>)}
    </View>

    {!!(state.error || localError) && <Text accessibilityRole="alert" style={ui.error}>{state.error || localError}</Text>}
    {!!message && <Text accessibilityRole="alert" style={ui.success}>{message}</Text>}

    {state.loading ? <Text style={ui.muted}>Đang tải bảng điểm...</Text> : <>
      <GradeStatus summary={state.data?.summary} />
      {mode === "scores" && <ScoresPanel
        component={component}
        components={components}
        componentId={selected}
        grades={state.data?.grades ?? []}
        students={filteredStudents}
        search={search}
        setSearch={setSearch}
        setComponentId={setComponentId}
        setEditingGrade={setEditingGrade}
      />}
      {mode === "components" && <ComponentsPanel
        components={components}
        totalWeight={totalWeight}
        onAdd={() => setEditingComponent({ componentName: "", weightPercent: "", maxScore: 10, displayOrder: components.length + 1 })}
        onEdit={component => setEditingComponent(component as unknown as Row)}
        onDelete={component => { setLocalError(""); setDeletingComponent(component); }}
      />}
      {mode === "summary" && <SummaryPanel
        summary={state.data?.summary}
        busy={busy}
        onFinalize={() => void finalizeGrades()}
      />}
    </>}

    {editingGrade && component && <RecordEditor
      title={`Nhập điểm: ${editingGrade.fullName}`}
      initial={editingGrade}
      fields={[
        { key: "score", label: `Điểm (tối đa ${component.maxScore})`, type: "number", min: 0, max: Number(component.maxScore) },
        { key: "note", label: "Ghi chú", type: "multiline" },
      ]}
      onClose={() => setEditingGrade(null)}
      save={body => teacherRequest(`${path}/grades/${editingGrade.studentId}`, "PUT", { ...body, gradeComponentId: Number(selected) })}
      onSaved={() => { setEditingGrade(null); setMessage("Đã lưu điểm. Điểm tổng kết của sinh viên này cần cập nhật lại."); void state.refresh(); }}
    />}

    {editingComponent && <RecordEditor
      title={`${editingComponent.gradeComponentId ? "Sửa" : "Thêm"} thành phần điểm`}
      initial={editingComponent}
      fields={componentFields}
      onClose={() => setEditingComponent(null)}
      save={body => teacherRequest(
        editingComponent.gradeComponentId ? `${path}/grade-components/${editingComponent.gradeComponentId}` : `${path}/grade-components`,
        editingComponent.gradeComponentId ? "PUT" : "POST",
        body)}
      onSaved={() => { setEditingComponent(null); setMessage("Đã lưu thành phần điểm. Điểm tổng kết của lớp cần cập nhật lại."); void state.refresh(); }}
    />}

    {deletingComponent && <Dialog title="Xóa thành phần điểm" busy={busy} onClose={() => setDeletingComponent(null)}>
      <Text style={ui.text}>Xóa “{deletingComponent.componentName}”?</Text>
      <Text style={ui.muted}>Chỉ xóa được thành phần chưa có điểm sinh viên.</Text>
      {!!localError && <Text style={ui.error}>{localError}</Text>}
      <Button label={busy ? "Đang xóa..." : "Xóa"} icon="trash-outline" disabled={busy} onPress={() => void removeComponent()} />
    </Dialog>}
  </View>;
}

function GradeStatus({ summary }: { summary?: GradeSummary }) {
  if (!summary) return null;
  const complete = summary.isWeightComplete;
  return <View style={[ui.card, complete ? styles.goodCard : styles.warnCard]}>
    <View style={ui.row}>
      <Text style={[ui.heading, ui.grow]}>Trọng số</Text>
      <Text style={complete ? styles.goodText : styles.warnText}>{summary.totalWeightPercent}%</Text>
    </View>
    <Text style={ui.muted}>{complete ? "Đã đủ 100%, có thể cập nhật điểm tổng kết." : "Tổng trọng số cần bằng 100% trước khi cập nhật tổng kết."}</Text>
    <Text style={ui.muted}>{summary.componentCount} thành phần · {summary.finalizedCount}/{summary.studentCount} sinh viên đã có điểm tổng kết</Text>
  </View>;
}

function ScoresPanel({
  component,
  components,
  componentId,
  grades,
  students,
  search,
  setSearch,
  setComponentId,
  setEditingGrade,
}: {
  component?: GradeComponent;
  components: GradeComponent[];
  componentId: string;
  grades: StudentGrade[];
  students: Student[];
  search: string;
  setSearch: (value: string) => void;
  setComponentId: (value: string) => void;
  setEditingGrade: (value: Row) => void;
}) {
  return <View style={{ gap: 14 }}>
    <Select label="Thành phần điểm" value={componentId} onChange={setComponentId} options={components.map(item => ({
      value: String(item.gradeComponentId),
      label: `${item.componentName} (${item.weightPercent}%)`,
    }))} />
    {!component && <Text style={ui.muted}>Lớp chưa có thành phần điểm.</Text>}
    <TextInput style={ui.input} accessibilityLabel="Tìm sinh viên" placeholder="Mã hoặc tên sinh viên" value={search} onChangeText={setSearch} />
    {component && students.map(student => {
      const grade = grades.find(item => item.studentId === student.studentId && String(item.gradeComponentId) === componentId);
      return <View key={String(student.studentId)} style={ui.card}>
        <Text style={ui.heading}>{student.fullName}</Text>
        <Text style={ui.muted}>{student.studentCode}</Text>
        <Text style={ui.code}>{display(grade?.score)} / {component.maxScore}</Text>
        {!!grade?.note && <Text style={ui.text}>{grade.note}</Text>}
        <Button label="Nhập điểm" icon="create-outline" onPress={() => setEditingGrade({ ...student, score: grade?.score ?? null, note: grade?.note ?? null } as Row)} />
      </View>;
    })}
    {!students.length && <Text style={ui.muted}>Không có sinh viên phù hợp.</Text>}
  </View>;
}

function ComponentsPanel({
  components,
  totalWeight,
  onAdd,
  onEdit,
  onDelete,
}: {
  components: GradeComponent[];
  totalWeight: number;
  onAdd: () => void;
  onEdit: (component: GradeComponent) => void;
  onDelete: (component: GradeComponent) => void;
}) {
  return <View style={{ gap: 14 }}>
    <View style={ui.row}>
      <Text style={[ui.muted, ui.grow]}>Tổng trọng số hiện tại: {totalWeight}%</Text>
      <IconButton icon="add" label="Thêm thành phần điểm" onPress={onAdd} />
    </View>
    {components.map(component => <View key={String(component.gradeComponentId)} style={ui.card}>
      <View style={ui.row}>
        <Text style={[ui.heading, ui.grow]}>{component.componentName}</Text>
        <Text style={ui.code}>{component.weightPercent}%</Text>
      </View>
      <Text style={ui.muted}>Điểm tối đa {component.maxScore} · Thứ tự {component.displayOrder}</Text>
      <View style={ui.row}>
        <View style={ui.grow} />
        <IconButton label="Sửa thành phần" icon="create-outline" onPress={() => onEdit(component)} />
        <IconButton label="Xóa thành phần" icon="trash-outline" onPress={() => onDelete(component)} />
      </View>
    </View>)}
    {!components.length && <Text style={ui.muted}>Lớp chưa có thành phần điểm.</Text>}
  </View>;
}

function SummaryPanel({ summary, busy, onFinalize }: { summary?: GradeSummary; busy: boolean; onFinalize: () => void }) {
  if (!summary) return null;
  const canFinalize = summary.isWeightComplete && summary.componentCount > 0 && !busy;
  return <View style={{ gap: 14 }}>
    <Button label={busy ? "Đang cập nhật..." : "Cập nhật điểm tổng kết"} icon="calculator-outline" disabled={!canFinalize} onPress={onFinalize} />
    {!summary.isWeightComplete && <Text style={ui.muted}>Cần chỉnh tổng trọng số thành đúng 100% trước khi cập nhật.</Text>}
    {summary.students.map(student => <View key={String(student.studentId)} style={ui.card}>
      <Text style={ui.heading}>{student.fullName}</Text>
      <Text style={ui.muted}>{student.studentCode}</Text>
      <Text style={ui.text}>Điểm tính toán: {student.calculatedScore10}</Text>
      <Text style={ui.text}>Đã lưu: {display(student.finalScore10)} · {display(student.letterGrade)}</Text>
      <Text style={ui.muted}>{student.gradedComponents}/{student.totalComponents} thành phần đã nhập điểm</Text>
    </View>)}
    {!summary.students.length && <Text style={ui.muted}>Lớp chưa có sinh viên.</Text>}
  </View>;
}

const styles = StyleSheet.create({
  segment: { flexDirection: "row", gap: 8, flexWrap: "wrap" },
  segmentItem: { borderWidth: 1, borderColor: colors.border, backgroundColor: colors.surface, borderRadius: 8, paddingHorizontal: 12, paddingVertical: 10, minHeight: 42, justifyContent: "center" },
  segmentActive: { backgroundColor: colors.primary, borderColor: colors.primary },
  segmentText: { color: colors.ink, fontWeight: "700" },
  segmentTextActive: { color: "white" },
  goodCard: { borderColor: colors.success },
  warnCard: { borderColor: "#D88A2D" },
  goodText: { color: colors.success, fontWeight: "800" },
  warnText: { color: "#A65F00", fontWeight: "800" },
});
