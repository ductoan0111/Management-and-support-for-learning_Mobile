import { createStudyGoal, createStudyTask, deleteStudyGoal, deleteStudyTask, getStudentAssignments, getStudyGoals, getStudyTasks, submitStudentAssignment, updateStudyGoal, updateStudyTask, type StudentAssignment, type StudyGoal, type StudyGoalWriteParams, type StudyTask, type StudyTaskWriteParams, type SubmitAssignmentParams } from "@/api/student";
import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import InfoCard from "@/components/InfoCard";
import { colors, shadows } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useState, type ReactNode } from "react";
import { ActivityIndicator, KeyboardAvoidingView, Linking, Modal, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { ActionButton, EmptyState, FeedbackText, formatDate, formatDateTime, formatNumber, goalStatusLabel, goalTypeLabel, httpUrl, priorityLabel, SegmentedTabs, StatusPill, statusTone, studentUi, taskStatusLabel, todayInputDate, type SegmentOption } from "./components/StudentUI";
import { useStudentData } from "./useStudentData";

type StudentTasksData = {
  assignments: StudentAssignment[];
  goals: StudyGoal[];
  tasks: StudyTask[];
};

type TaskTab = "assignments" | "goals" | "tasks";

const tabs: SegmentOption<TaskTab>[] = [
  { value: "assignments", label: "Bài tập", icon: "create-outline" },
  { value: "goals", label: "Mục tiêu", icon: "flag-outline" },
  { value: "tasks", label: "Nhiệm vụ", icon: "checkbox-outline" },
];

const goalTypes = [
  { value: 1, label: "GPA" },
  { value: 2, label: "Điểm môn" },
  { value: 3, label: "Tín chỉ" },
  { value: 4, label: "Thói quen" },
  { value: 5, label: "Khác" },
];

const goalStatuses = [
  { value: 1, label: "Đang làm" },
  { value: 2, label: "Hoàn thành" },
  { value: 3, label: "Tạm dừng" },
];

const taskPriorities = [
  { value: 1, label: "Thấp" },
  { value: 2, label: "Vừa" },
  { value: 3, label: "Cao" },
];

const taskStatuses = [
  { value: 1, label: "Chưa làm" },
  { value: 2, label: "Đang làm" },
  { value: 3, label: "Hoàn thành" },
  { value: 4, label: "Tạm dừng" },
];

async function loadTasks(): Promise<StudentTasksData> {
  const [assignments, goals, tasks] = await Promise.all([
    getStudentAssignments(),
    getStudyGoals(),
    getStudyTasks(),
  ]);
  return { assignments, goals, tasks };
}

export default function StudentTasksScreen() {
  const state = useStudentData(loadTasks);
  const [tab, setTab] = useState<TaskTab>("assignments");
  const [submissionTarget, setSubmissionTarget] = useState<StudentAssignment | null>(null);
  const [goalEditing, setGoalEditing] = useState<StudyGoal | "new" | null>(null);
  const [taskEditing, setTaskEditing] = useState<StudyTask | "new" | null>(null);
  const [confirm, setConfirm] = useState<{ title: string; message: string; action: () => Promise<void> } | null>(null);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [actionError, setActionError] = useState("");
  const data = state.data;
  const assignments = data?.assignments ?? [];
  const goals = data?.goals ?? [];
  const tasks = data?.tasks ?? [];
  const pendingAssignments = assignments.filter((item) => !item.submittedAt).length;
  const activeGoals = goals.filter((item) => item.status === 1).length;
  const openTasks = tasks.filter((item) => item.status !== 3).length;

  async function runAction(action: () => Promise<void>, successMessage: string) {
    if (busy) return;
    setBusy(true);
    setActionError("");
    setMessage("");
    try {
      await action();
      setMessage(successMessage);
      await state.refresh();
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "Không lưu được thay đổi.");
    } finally {
      setBusy(false);
    }
  }

  async function saveSubmission(assignment: StudentAssignment, params: SubmitAssignmentParams) {
    await runAction(async () => {
      await submitStudentAssignment(assignment.assignmentId, params);
      setSubmissionTarget(null);
    }, "Đã nộp bài tập.");
  }

  async function saveGoal(target: StudyGoal | "new", params: StudyGoalWriteParams) {
    await runAction(async () => {
      if (target === "new") await createStudyGoal(params);
      else await updateStudyGoal(target.goalId, params);
      setGoalEditing(null);
    }, "Đã lưu mục tiêu học tập.");
  }

  async function saveTask(target: StudyTask | "new", params: StudyTaskWriteParams) {
    await runAction(async () => {
      if (target === "new") await createStudyTask(params);
      else await updateStudyTask(target.studyTaskId, params);
      setTaskEditing(null);
    }, "Đã lưu nhiệm vụ học tập.");
  }

  async function openLink(value: string | null) {
    setActionError("");
    const url = httpUrl(value);
    if (!url) {
      setActionError("Liên kết không hợp lệ.");
      return;
    }
    try {
      await Linking.openURL(url);
    } catch {
      setActionError("Không mở được liên kết.");
    }
  }

  return (
    <AppScreen>
      <BackHeader
        title="Bài tập và mục tiêu"
        subtitle="Làm bài tập, theo dõi deadline và quản lý mục tiêu học tập"
      />

      {state.loading ? <ActivityIndicator color={colors.primary} style={styles.loader} /> : null}
      <FeedbackText message={state.error || actionError} />
      <FeedbackText message={message} type="success" />

      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="create-outline" title="Chưa nộp" value={String(pendingAssignments)} tone="warning" />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="flag-outline" title="Mục tiêu" value={String(activeGoals)} tone="success" />
        </View>
      </View>
      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="checkbox-outline" title="Nhiệm vụ mở" value={String(openTasks)} />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="calendar-outline" title="Deadline" value={String(assignments.length)} tone="accent" />
        </View>
      </View>

      <SegmentedTabs options={tabs} value={tab} onChange={setTab} />

      {tab === "assignments" ? (
        <AssignmentsView
          assignments={assignments}
          busy={busy}
          onOpenLink={openLink}
          onSubmit={setSubmissionTarget}
        />
      ) : null}

      {tab === "goals" ? (
        <GoalsView
          goals={goals}
          busy={busy}
          onAdd={() => setGoalEditing("new")}
          onEdit={setGoalEditing}
          onComplete={(goal) => void runAction(async () => {
            await updateStudyGoal(goal.goalId, goalToPayload(goal, { status: 2, currentValue: goal.targetValue ?? goal.currentValue }));
          }, "Đã cập nhật mục tiêu.")}
          onDelete={(goal) => setConfirm({
            title: "Xóa mục tiêu",
            message: `Xóa “${goal.title}”?`,
            action: async () => {
              await deleteStudyGoal(goal.goalId);
            },
          })}
        />
      ) : null}

      {tab === "tasks" ? (
        <StudyTasksView
          busy={busy}
          tasks={tasks}
          onAdd={() => setTaskEditing("new")}
          onEdit={setTaskEditing}
          onComplete={(task) => void runAction(async () => {
            await updateStudyTask(task.studyTaskId, taskToPayload(task, { status: 3 }));
          }, "Đã hoàn thành nhiệm vụ.")}
          onDelete={(task) => setConfirm({
            title: "Xóa nhiệm vụ",
            message: `Xóa “${task.title}”?`,
            action: async () => {
              await deleteStudyTask(task.studyTaskId);
            },
          })}
        />
      ) : null}

      {submissionTarget ? (
        <SubmissionDialog
          assignment={submissionTarget}
          busy={busy}
          error={actionError}
          onClose={() => setSubmissionTarget(null)}
          onSubmit={(params) => void saveSubmission(submissionTarget, params)}
        />
      ) : null}

      {goalEditing ? (
        <GoalDialog
          busy={busy}
          error={actionError}
          goal={goalEditing === "new" ? null : goalEditing}
          onClose={() => setGoalEditing(null)}
          onSubmit={(params) => void saveGoal(goalEditing, params)}
        />
      ) : null}

      {taskEditing ? (
        <TaskDialog
          busy={busy}
          error={actionError}
          task={taskEditing === "new" ? null : taskEditing}
          onClose={() => setTaskEditing(null)}
          onSubmit={(params) => void saveTask(taskEditing, params)}
        />
      ) : null}

      {confirm ? (
        <ConfirmDialog
          busy={busy}
          message={confirm.message}
          title={confirm.title}
          onCancel={() => setConfirm(null)}
          onConfirm={() => void runAction(async () => {
            await confirm.action();
            setConfirm(null);
          }, "Đã xóa.")}
        />
      ) : null}
    </AppScreen>
  );
}

function AssignmentsView({
  assignments,
  busy,
  onOpenLink,
  onSubmit,
}: {
  assignments: StudentAssignment[];
  busy: boolean;
  onOpenLink: (url: string | null) => Promise<void>;
  onSubmit: (assignment: StudentAssignment) => void;
}) {
  if (!assignments.length) {
    return <EmptyState title="Chưa có bài tập" detail="Bài tập được công bố từ giảng viên sẽ hiển thị tại đây." icon="create-outline" />;
  }

  return (
    <View>
      <Text style={studentUi.sectionTitle}>Bài tập và deadline</Text>
      {assignments.map((assignment) => {
        const due = new Date(assignment.dueAt);
        const isOverdue = !Number.isNaN(due.getTime()) && due.getTime() < Date.now();
        const locked = isOverdue && !assignment.allowLateSubmission && !assignment.submissionId;
        return (
          <View key={assignment.assignmentId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{assignment.courseCode}</Text>
              <StatusPill label={assignment.submissionStatus} tone={statusTone(assignment.submissionStatus)} />
            </View>
            <Text style={studentUi.title}>{assignment.title}</Text>
            <Text style={studentUi.muted}>{assignment.courseName}</Text>
            {assignment.description ? <Text style={studentUi.text}>{assignment.description}</Text> : null}
            <Text style={studentUi.text}>Hạn nộp: {formatDateTime(assignment.dueAt)}</Text>
            <View style={studentUi.row}>
              <StatusPill label={`Điểm tối đa ${formatNumber(assignment.maxScore)}`} />
              {assignment.allowLateSubmission ? <StatusPill label="Cho nộp muộn" tone="warning" /> : null}
              {assignment.score !== null ? <StatusPill label={`Điểm ${formatNumber(assignment.score)}`} tone="success" /> : null}
            </View>
            {assignment.submittedAt ? (
              <View style={studentUi.mutedCard}>
                <Text style={studentUi.text}>Đã nộp: {formatDateTime(assignment.submittedAt)}</Text>
                {assignment.feedback ? <Text style={studentUi.muted}>Nhận xét: {assignment.feedback}</Text> : null}
              </View>
            ) : null}
            <View style={studentUi.row}>
              {assignment.attachmentUrl ? (
                <ActionButton label="Mở đề bài" icon="open-outline" variant="outline" onPress={() => void onOpenLink(assignment.attachmentUrl)} />
              ) : null}
              <ActionButton
                disabled={busy || locked}
                icon="cloud-upload-outline"
                label={assignment.submissionId ? "Cập nhật bài nộp" : locked ? "Đã quá hạn" : "Nộp bài"}
                onPress={() => onSubmit(assignment)}
              />
            </View>
          </View>
        );
      })}
    </View>
  );
}

function GoalsView({
  goals,
  busy,
  onAdd,
  onEdit,
  onComplete,
  onDelete,
}: {
  goals: StudyGoal[];
  busy: boolean;
  onAdd: () => void;
  onEdit: (goal: StudyGoal) => void;
  onComplete: (goal: StudyGoal) => void;
  onDelete: (goal: StudyGoal) => void;
}) {
  return (
    <View>
      <View style={studentUi.splitRow}>
        <Text style={studentUi.sectionTitle}>Mục tiêu học tập</Text>
        <ActionButton label="Thêm" icon="add" onPress={onAdd} />
      </View>
      {!goals.length ? (
        <EmptyState title="Chưa có mục tiêu" detail="Tạo mục tiêu GPA, điểm môn hoặc thói quen học tập để tự theo dõi tiến độ." icon="flag-outline" />
      ) : null}
      {goals.map((goal) => {
        const progress = goal.targetValue && goal.currentValue !== null
          ? Math.min(100, Math.max(0, (goal.currentValue / goal.targetValue) * 100))
          : null;
        return (
          <View key={goal.goalId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{goalTypeLabel(goal.goalType)}</Text>
              <StatusPill label={goalStatusLabel(goal.status)} tone={goal.status === 2 ? "success" : goal.status === 3 ? "muted" : "primary"} />
            </View>
            <Text style={studentUi.title}>{goal.title}</Text>
            {goal.description ? <Text style={studentUi.text}>{goal.description}</Text> : null}
            <Text style={studentUi.muted}>{formatDate(goal.startDate)} - {formatDate(goal.endDate)}</Text>
            {progress !== null ? (
              <View style={styles.progressShell}>
                <View style={[styles.progressFill, { width: `${progress}%` }]} />
              </View>
            ) : null}
            <Text style={studentUi.muted}>Hiện tại {formatNumber(goal.currentValue)} / Mục tiêu {formatNumber(goal.targetValue)}</Text>
            <View style={studentUi.row}>
              <ActionButton label="Sửa" icon="create-outline" variant="outline" onPress={() => onEdit(goal)} />
              {goal.status !== 2 ? <ActionButton disabled={busy} label="Hoàn thành" icon="checkmark-circle-outline" onPress={() => onComplete(goal)} /> : null}
              <ActionButton disabled={busy} label="Xóa" icon="trash-outline" variant="danger" onPress={() => onDelete(goal)} />
            </View>
          </View>
        );
      })}
    </View>
  );
}

function StudyTasksView({
  tasks,
  busy,
  onAdd,
  onEdit,
  onComplete,
  onDelete,
}: {
  tasks: StudyTask[];
  busy: boolean;
  onAdd: () => void;
  onEdit: (task: StudyTask) => void;
  onComplete: (task: StudyTask) => void;
  onDelete: (task: StudyTask) => void;
}) {
  return (
    <View>
      <View style={studentUi.splitRow}>
        <Text style={studentUi.sectionTitle}>Nhiệm vụ cá nhân</Text>
        <ActionButton label="Thêm" icon="add" onPress={onAdd} />
      </View>
      {!tasks.length ? (
        <EmptyState title="Chưa có nhiệm vụ" detail="Thêm nhiệm vụ ôn tập, đọc tài liệu hoặc chuẩn bị bài để quản lý tiến độ." icon="checkbox-outline" />
      ) : null}
      {tasks.map((task) => (
        <View key={task.studyTaskId} style={studentUi.flatCard}>
          <View style={studentUi.splitRow}>
            <Text style={studentUi.code}>{task.courseCode ?? "Cá nhân"}</Text>
            <StatusPill label={taskStatusLabel(task.status)} tone={task.status === 3 ? "success" : task.status === 4 ? "muted" : "warning"} />
          </View>
          <Text style={studentUi.title}>{task.title}</Text>
          {task.courseName ? <Text style={studentUi.muted}>{task.courseName}</Text> : null}
          {task.description ? <Text style={studentUi.text}>{task.description}</Text> : null}
          <Text style={studentUi.muted}>Ưu tiên: {priorityLabel(task.priority)} · Hạn: {formatDateTime(task.dueAt)}</Text>
          {task.reminderAt ? <Text style={studentUi.muted}>Nhắc lúc: {formatDateTime(task.reminderAt)}</Text> : null}
          <View style={studentUi.row}>
            <ActionButton label="Sửa" icon="create-outline" variant="outline" onPress={() => onEdit(task)} />
            {task.status !== 3 ? <ActionButton disabled={busy} label="Hoàn thành" icon="checkmark-circle-outline" onPress={() => onComplete(task)} /> : null}
            <ActionButton disabled={busy} label="Xóa" icon="trash-outline" variant="danger" onPress={() => onDelete(task)} />
          </View>
        </View>
      ))}
    </View>
  );
}

function StudentModal({ title, children, onClose, busy }: { title: string; children: ReactNode; onClose: () => void; busy?: boolean }) {
  return (
    <Modal transparent animationType="fade" onRequestClose={() => { if (!busy) onClose(); }}>
      <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined} style={styles.overlay}>
        <View style={styles.modal}>
          <View style={studentUi.splitRow}>
            <Text style={styles.modalTitle}>{title}</Text>
            <Pressable
              accessibilityLabel="Đóng"
              accessibilityRole="button"
              disabled={busy}
              onPress={onClose}
              style={studentUi.iconButton}
            >
              <Ionicons name="close" size={21} color={colors.primary} />
            </Pressable>
          </View>
          <ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={styles.modalBody}>
            {children}
          </ScrollView>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}

function SubmissionDialog({
  assignment,
  busy,
  error,
  onClose,
  onSubmit,
}: {
  assignment: StudentAssignment;
  busy: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (params: SubmitAssignmentParams) => void;
}) {
  const [textContent, setTextContent] = useState(assignment.textContent ?? "");
  const [fileUrl, setFileUrl] = useState(assignment.fileUrl ?? "");
  const [formError, setFormError] = useState("");

  function submit() {
    const content = textContent.trim();
    const file = fileUrl.trim();
    setFormError("");
    if (!content && !file) {
      setFormError("Vui lòng nhập nội dung bài làm hoặc liên kết file.");
      return;
    }
    if (file && !httpUrl(file)) {
      setFormError("Liên kết file phải bắt đầu bằng http:// hoặc https://.");
      return;
    }
    onSubmit({ textContent: content || null, fileUrl: file || null });
  }

  return (
    <StudentModal title="Nộp bài tập" busy={busy} onClose={onClose}>
      <Text style={studentUi.title}>{assignment.title}</Text>
      <Text style={studentUi.muted}>{assignment.courseCode} · Hạn nộp {formatDateTime(assignment.dueAt)}</Text>
      <FeedbackText message={formError || error} />
      <LabeledInput
        label="Nội dung bài làm"
        multiline
        onChangeText={setTextContent}
        placeholder="Nhập câu trả lời, ghi chú hoặc mô tả bài nộp"
        value={textContent}
      />
      <LabeledInput
        autoCapitalize="none"
        label="Liên kết file"
        onChangeText={setFileUrl}
        placeholder="https://..."
        value={fileUrl}
      />
      <ActionButton disabled={busy} icon="send-outline" label={busy ? "Đang nộp..." : "Nộp bài"} onPress={submit} />
    </StudentModal>
  );
}

function GoalDialog({
  goal,
  busy,
  error,
  onClose,
  onSubmit,
}: {
  goal: StudyGoal | null;
  busy: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (params: StudyGoalWriteParams) => void;
}) {
  const [title, setTitle] = useState(goal?.title ?? "");
  const [description, setDescription] = useState(goal?.description ?? "");
  const [goalType, setGoalType] = useState(goal?.goalType ?? 1);
  const [targetValue, setTargetValue] = useState(valueInput(goal?.targetValue));
  const [currentValue, setCurrentValue] = useState(valueInput(goal?.currentValue));
  const [startDate, setStartDate] = useState(dateInput(goal?.startDate) || todayInputDate());
  const [endDate, setEndDate] = useState(dateInput(goal?.endDate));
  const [status, setStatus] = useState(goal?.status ?? 1);
  const [formError, setFormError] = useState("");

  function submit() {
    setFormError("");
    const payload: StudyGoalWriteParams = {
      title: title.trim(),
      description: description.trim() || null,
      goalType,
      targetValue: numberOrNull(targetValue),
      currentValue: numberOrNull(currentValue),
      startDate: startDate.trim(),
      endDate: endDate.trim() || null,
      status,
    };
    if (!payload.title) {
      setFormError("Vui lòng nhập tên mục tiêu.");
      return;
    }
    if (!isDateOnly(payload.startDate) || (payload.endDate && !isDateOnly(payload.endDate))) {
      setFormError("Ngày phải có dạng YYYY-MM-DD.");
      return;
    }
    onSubmit(payload);
  }

  return (
    <StudentModal title={goal ? "Sửa mục tiêu" : "Thêm mục tiêu"} busy={busy} onClose={onClose}>
      <FeedbackText message={formError || error} />
      <LabeledInput label="Tên mục tiêu" onChangeText={setTitle} value={title} />
      <LabeledInput label="Mô tả" multiline onChangeText={setDescription} value={description} />
      <ChoiceRow label="Loại mục tiêu" options={goalTypes} value={goalType} onChange={setGoalType} />
      <View style={styles.twoColumns}>
        <LabeledInput keyboardType="decimal-pad" label="Mục tiêu" onChangeText={setTargetValue} value={targetValue} />
        <LabeledInput keyboardType="decimal-pad" label="Hiện tại" onChangeText={setCurrentValue} value={currentValue} />
      </View>
      <View style={styles.twoColumns}>
        <LabeledInput label="Bắt đầu" onChangeText={setStartDate} placeholder="YYYY-MM-DD" value={startDate} />
        <LabeledInput label="Kết thúc" onChangeText={setEndDate} placeholder="YYYY-MM-DD" value={endDate} />
      </View>
      <ChoiceRow label="Trạng thái" options={goalStatuses} value={status} onChange={setStatus} />
      <ActionButton disabled={busy} icon="save-outline" label={busy ? "Đang lưu..." : "Lưu mục tiêu"} onPress={submit} />
    </StudentModal>
  );
}

function TaskDialog({
  task,
  busy,
  error,
  onClose,
  onSubmit,
}: {
  task: StudyTask | null;
  busy: boolean;
  error: string;
  onClose: () => void;
  onSubmit: (params: StudyTaskWriteParams) => void;
}) {
  const [title, setTitle] = useState(task?.title ?? "");
  const [description, setDescription] = useState(task?.description ?? "");
  const [courseId, setCourseId] = useState(task?.courseId ? String(task.courseId) : "");
  const [startAt, setStartAt] = useState(dateTimeInput(task?.startAt));
  const [dueAt, setDueAt] = useState(dateTimeInput(task?.dueAt));
  const [reminderAt, setReminderAt] = useState(dateTimeInput(task?.reminderAt));
  const [priority, setPriority] = useState(task?.priority ?? 2);
  const [status, setStatus] = useState(task?.status ?? 1);
  const [formError, setFormError] = useState("");

  function submit() {
    setFormError("");
    const normalizedCourseId = courseId.trim() ? Number(courseId.trim()) : null;
    if (!title.trim()) {
      setFormError("Vui lòng nhập tên nhiệm vụ.");
      return;
    }
    if (normalizedCourseId !== null && (!Number.isInteger(normalizedCourseId) || normalizedCourseId <= 0)) {
      setFormError("CourseId không hợp lệ.");
      return;
    }
    onSubmit({
      courseId: normalizedCourseId,
      title: title.trim(),
      description: description.trim() || null,
      startAt: normalizeDateTime(startAt),
      dueAt: normalizeDateTime(dueAt),
      reminderAt: normalizeDateTime(reminderAt),
      priority,
      status,
    });
  }

  return (
    <StudentModal title={task ? "Sửa nhiệm vụ" : "Thêm nhiệm vụ"} busy={busy} onClose={onClose}>
      <FeedbackText message={formError || error} />
      <LabeledInput label="Tên nhiệm vụ" onChangeText={setTitle} value={title} />
      <LabeledInput label="Mô tả" multiline onChangeText={setDescription} value={description} />
      <LabeledInput keyboardType="number-pad" label="CourseId (nếu có)" onChangeText={setCourseId} value={courseId} />
      <LabeledInput label="Bắt đầu" onChangeText={setStartAt} placeholder="YYYY-MM-DD HH:mm" value={startAt} />
      <LabeledInput label="Hạn hoàn thành" onChangeText={setDueAt} placeholder="YYYY-MM-DD HH:mm" value={dueAt} />
      <LabeledInput label="Nhắc nhở" onChangeText={setReminderAt} placeholder="YYYY-MM-DD HH:mm" value={reminderAt} />
      <ChoiceRow label="Ưu tiên" options={taskPriorities} value={priority} onChange={setPriority} />
      <ChoiceRow label="Trạng thái" options={taskStatuses} value={status} onChange={setStatus} />
      <ActionButton disabled={busy} icon="save-outline" label={busy ? "Đang lưu..." : "Lưu nhiệm vụ"} onPress={submit} />
    </StudentModal>
  );
}

function ConfirmDialog({
  title,
  message,
  busy,
  onCancel,
  onConfirm,
}: {
  title: string;
  message: string;
  busy: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  return (
    <StudentModal title={title} busy={busy} onClose={onCancel}>
      <Text style={studentUi.text}>{message}</Text>
      <View style={studentUi.row}>
        <ActionButton disabled={busy} icon="close-outline" label="Hủy" variant="outline" onPress={onCancel} />
        <ActionButton disabled={busy} icon="trash-outline" label={busy ? "Đang xóa..." : "Xóa"} variant="danger" onPress={onConfirm} />
      </View>
    </StudentModal>
  );
}

function LabeledInput({
  label,
  multiline,
  value,
  onChangeText,
  placeholder,
  keyboardType,
  autoCapitalize,
}: {
  label: string;
  multiline?: boolean;
  value: string;
  onChangeText: (value: string) => void;
  placeholder?: string;
  keyboardType?: "default" | "number-pad" | "decimal-pad";
  autoCapitalize?: "none" | "sentences";
}) {
  return (
    <View style={{ gap: 6 }}>
      <Text style={studentUi.muted}>{label}</Text>
      <TextInput
        accessibilityLabel={label}
        autoCapitalize={autoCapitalize}
        keyboardType={keyboardType}
        multiline={multiline}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor="#94A3B8"
        style={[studentUi.input, multiline ? studentUi.multilineInput : null]}
        value={value}
      />
    </View>
  );
}

function ChoiceRow({
  label,
  options,
  value,
  onChange,
}: {
  label: string;
  options: { value: number; label: string }[];
  value: number;
  onChange: (value: number) => void;
}) {
  return (
    <View style={{ gap: 8 }}>
      <Text style={studentUi.muted}>{label}</Text>
      <View style={studentUi.row}>
        {options.map((option) => {
          const selected = option.value === value;
          return (
            <Pressable
              accessibilityRole="button"
              accessibilityState={{ selected }}
              key={option.value}
              onPress={() => onChange(option.value)}
              style={[styles.choice, selected ? styles.choiceActive : null]}
            >
              <Text style={[styles.choiceText, selected ? styles.choiceTextActive : null]}>{option.label}</Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

function goalToPayload(goal: StudyGoal, overrides: Partial<StudyGoalWriteParams> = {}): StudyGoalWriteParams {
  return {
    title: goal.title,
    description: goal.description,
    goalType: goal.goalType,
    targetValue: goal.targetValue,
    currentValue: goal.currentValue,
    startDate: goal.startDate,
    endDate: goal.endDate,
    status: goal.status,
    ...overrides,
  };
}

function taskToPayload(task: StudyTask, overrides: Partial<StudyTaskWriteParams> = {}): StudyTaskWriteParams {
  return {
    courseId: task.courseId,
    title: task.title,
    description: task.description,
    startAt: task.startAt,
    dueAt: task.dueAt,
    reminderAt: task.reminderAt,
    priority: task.priority,
    status: task.status,
    ...overrides,
  };
}

function valueInput(value: number | null | undefined) {
  return value === null || value === undefined ? "" : String(value);
}

function numberOrNull(value: string) {
  const text = value.trim();
  if (!text) return null;
  const number = Number(text.replace(",", "."));
  return Number.isFinite(number) ? number : null;
}

function dateInput(value: string | null | undefined) {
  return value ? value.slice(0, 10) : "";
}

function isDateOnly(value: string) {
  return /^\d{4}-\d{2}-\d{2}$/.test(value);
}

function dateTimeInput(value: string | null | undefined) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value.slice(0, 16).replace("T", " ");
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function normalizeDateTime(value: string) {
  const text = value.trim();
  if (!text) return null;
  if (/^\d{4}-\d{2}-\d{2}$/.test(text)) return `${text}T00:00:00`;
  if (/^\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}$/.test(text)) return `${text.replace(" ", "T")}:00`;
  return text;
}

const styles = StyleSheet.create({
  loader: {
    marginBottom: 12,
  },
  metrics: {
    flexDirection: "row",
    gap: 12,
  },
  metricItem: {
    flex: 1,
  },
  overlay: {
    backgroundColor: "rgba(15, 23, 42, 0.48)",
    flex: 1,
    justifyContent: "center",
    padding: 20,
  },
  modal: {
    alignSelf: "center",
    backgroundColor: colors.surface,
    borderRadius: 8,
    maxHeight: "90%",
    maxWidth: 640,
    padding: 18,
    width: "100%",
    ...shadows.card,
  },
  modalTitle: {
    color: colors.ink,
    flex: 1,
    fontSize: 18,
    fontWeight: "800",
  },
  modalBody: {
    gap: 14,
    paddingTop: 14,
  },
  progressShell: {
    backgroundColor: colors.surfaceMuted,
    borderRadius: 8,
    height: 10,
    overflow: "hidden",
  },
  progressFill: {
    backgroundColor: colors.success,
    height: "100%",
  },
  twoColumns: {
    gap: 10,
  },
  choice: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 40,
    paddingHorizontal: 12,
  },
  choiceActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  choiceText: {
    color: colors.ink,
    fontSize: 13,
    fontWeight: "800",
  },
  choiceTextActive: {
    color: "#FFFFFF",
  },
});
