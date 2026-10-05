import { getStudentExams, getStudentGpa, getStudentGrades, getStudentSchedule, getStudentSectionScores, getStudentSections, type StudentExamSchedule, type StudentGrade, type StudentGpa, type StudentSchedule, type StudentSection, type StudentSectionScore } from "@/api/student";
import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import InfoCard from "@/components/InfoCard";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useState } from "react";
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";
import { dayLabel, display, EmptyState, examTypeLabel, FeedbackText, formatDate, formatNumber, formatTime, SegmentedTabs, StatusPill, studentUi, type SegmentOption } from "./components/StudentUI";
import { useStudentData } from "./useStudentData";

type LearningData = {
  sections: StudentSection[];
  schedules: StudentSchedule[];
  exams: StudentExamSchedule[];
  grades: StudentGrade[];
  scores: StudentSectionScore[];
  gpa: StudentGpa;
};

type LearningTab = "courses" | "schedule" | "exams" | "grades";

const tabs: SegmentOption<LearningTab>[] = [
  { value: "courses", label: "Môn học", icon: "school-outline" },
  { value: "schedule", label: "Lịch học", icon: "calendar-outline" },
  { value: "exams", label: "Lịch thi", icon: "alarm-outline" },
  { value: "grades", label: "Điểm", icon: "stats-chart-outline" },
];

async function loadLearning(): Promise<LearningData> {
  const [sections, schedules, exams, grades, scores, gpa] = await Promise.all([
    getStudentSections(),
    getStudentSchedule(),
    getStudentExams(),
    getStudentGrades(),
    getStudentSectionScores(),
    getStudentGpa(),
  ]);
  return { sections, schedules, exams, grades, scores, gpa };
}

export default function StudentLearningScreen() {
  const state = useStudentData(loadLearning);
  const [tab, setTab] = useState<LearningTab>("courses");
  const [selectedDay, setSelectedDay] = useState("");
  const data = state.data;
  const totalCredits = data?.sections.reduce((sum, section) => sum + section.credits, 0) ?? 0;
  const scheduleDays = Array.from(new Set((data?.schedules ?? []).map((item) => item.dayOfWeek))).sort((a, b) => a - b);
  const visibleSchedules = selectedDay
    ? (data?.schedules ?? []).filter((item) => String(item.dayOfWeek) === selectedDay)
    : data?.schedules ?? [];

  return (
    <AppScreen>
      <BackHeader
        title="Học tập"
        subtitle="Xem môn học, lịch học, lịch thi và điểm số theo dữ liệu backend"
      />

      {state.loading ? <ActivityIndicator color={colors.primary} style={styles.loader} /> : null}
      <FeedbackText message={state.error} />

      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="book-outline" title="Môn đang học" value={String(data?.sections.length ?? 0)} />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="ribbon-outline" title="Tín chỉ" value={String(totalCredits)} tone="accent" />
        </View>
      </View>
      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="trending-up-outline" title="GPA" value={formatNumber(data?.gpa.gpa)} tone="success" />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="stats-chart-outline" title="Môn có điểm" value={String(data?.scores.length ?? 0)} tone="warning" />
        </View>
      </View>

      <SegmentedTabs options={tabs} value={tab} onChange={setTab} />

      {tab === "courses" ? <CoursesView sections={data?.sections ?? []} /> : null}
      {tab === "schedule" ? (
        <ScheduleView
          days={scheduleDays}
          selectedDay={selectedDay}
          schedules={visibleSchedules}
          onSelectDay={setSelectedDay}
        />
      ) : null}
      {tab === "exams" ? <ExamsView exams={data?.exams ?? []} /> : null}
      {tab === "grades" ? <GradesView grades={data?.grades ?? []} scores={data?.scores ?? []} /> : null}
    </AppScreen>
  );
}

function CoursesView({ sections }: { sections: StudentSection[] }) {
  if (!sections.length) {
    return <EmptyState title="Chưa có môn học" detail="Danh sách lớp học phần sẽ hiển thị khi bạn được ghi danh." icon="school-outline" />;
  }

  return (
    <View>
      <Text style={studentUi.sectionTitle}>Môn học đã đăng ký</Text>
      {sections.map((section) => (
        <View key={section.enrollmentId} style={studentUi.flatCard}>
          <View style={studentUi.splitRow}>
            <Text style={studentUi.code}>{section.courseCode}</Text>
            <StatusPill label={`${section.credits} tín chỉ`} />
          </View>
          <Text style={studentUi.title}>{section.courseName}</Text>
          <Text style={studentUi.muted}>{section.sectionCode} · {section.semesterName} · {section.academicYear}</Text>
          <View style={studentUi.row}>
            <StatusPill label={section.enrollmentStatus === 1 ? "Đang học" : section.enrollmentStatus === 2 ? "Hoàn thành" : "Đã hủy"} tone={section.enrollmentStatus === 2 ? "success" : section.enrollmentStatus === 0 ? "danger" : "primary"} />
            <StatusPill label={`Điểm: ${formatNumber(section.finalScore10)}`} tone={section.finalScore10 === null ? "muted" : "success"} />
            {section.letterGrade ? <StatusPill label={section.letterGrade} tone="warning" /> : null}
          </View>
        </View>
      ))}
    </View>
  );
}

function ScheduleView({
  days,
  selectedDay,
  schedules,
  onSelectDay,
}: {
  days: number[];
  selectedDay: string;
  schedules: StudentSchedule[];
  onSelectDay: (value: string) => void;
}) {
  if (!days.length) {
    return <EmptyState title="Chưa có lịch học" detail="Lịch học sẽ xuất hiện khi lớp học phần có thời khóa biểu." icon="calendar-outline" />;
  }

  return (
    <View>
      <Text style={studentUi.sectionTitle}>Lịch học</Text>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.dayTabs}>
        <Pressable
          accessibilityRole="button"
          accessibilityState={{ selected: selectedDay === "" }}
          onPress={() => onSelectDay("")}
          style={[styles.dayButton, selectedDay === "" ? styles.dayButtonActive : null]}
        >
          <Text style={[styles.dayText, selectedDay === "" ? styles.dayTextActive : null]}>Tất cả</Text>
        </Pressable>
        {days.map((day) => (
          <Pressable
            accessibilityRole="button"
            accessibilityState={{ selected: selectedDay === String(day) }}
            key={day}
            onPress={() => onSelectDay(String(day))}
            style={[styles.dayButton, selectedDay === String(day) ? styles.dayButtonActive : null]}
          >
            <Text style={[styles.dayText, selectedDay === String(day) ? styles.dayTextActive : null]}>{dayLabel(day)}</Text>
          </Pressable>
        ))}
      </ScrollView>
      {schedules.map((item) => (
        <View key={item.scheduleId} style={studentUi.flatCard}>
          <View style={styles.scheduleCard}>
            <View style={styles.timeRail}>
              <Text style={studentUi.code}>{dayLabel(item.dayOfWeek)}</Text>
              <Text style={styles.timeText}>{formatTime(item.startTime)}</Text>
              <Text style={studentUi.muted}>{formatTime(item.endTime)}</Text>
            </View>
            <View style={studentUi.grow}>
              <Text style={studentUi.title}>{item.courseName}</Text>
              <Text style={studentUi.muted}>{item.courseCode} · {item.sectionCode}</Text>
              <View style={studentUi.row}>
                <Ionicons name="location-outline" size={15} color={colors.muted} />
                <Text style={studentUi.muted}>Phòng {display(item.room)} · {display(item.building)}</Text>
              </View>
              <Text style={studentUi.muted}>{formatDate(item.effectiveFrom)} - {formatDate(item.effectiveTo)}</Text>
              {item.note ? <Text style={studentUi.text}>{item.note}</Text> : null}
            </View>
          </View>
        </View>
      ))}
    </View>
  );
}

function ExamsView({ exams }: { exams: StudentExamSchedule[] }) {
  if (!exams.length) {
    return <EmptyState title="Chưa có lịch thi" detail="Lịch thi sẽ được cập nhật khi có dữ liệu từ backend." icon="alarm-outline" />;
  }

  return (
    <View>
      <Text style={studentUi.sectionTitle}>Lịch thi</Text>
      {exams.map((exam) => (
        <View key={exam.examId} style={studentUi.flatCard}>
          <View style={studentUi.splitRow}>
            <Text style={studentUi.code}>{exam.courseCode}</Text>
            <StatusPill label={examTypeLabel(exam.examType)} tone="warning" />
          </View>
          <Text style={studentUi.title}>{exam.examName}</Text>
          <Text style={studentUi.muted}>{exam.courseName} · {exam.sectionCode}</Text>
          <Text style={studentUi.text}>{formatDate(exam.examDate)} · {formatTime(exam.startTime)} · {exam.durationMinutes} phút</Text>
          <Text style={studentUi.muted}>Phòng {display(exam.room)}</Text>
          {exam.note ? <Text style={studentUi.text}>{exam.note}</Text> : null}
        </View>
      ))}
    </View>
  );
}

function GradesView({ grades, scores }: { grades: StudentGrade[]; scores: StudentSectionScore[] }) {
  const gradesBySection = new Map<number, StudentGrade[]>();
  for (const grade of grades) {
    gradesBySection.set(grade.sectionId, [...(gradesBySection.get(grade.sectionId) ?? []), grade]);
  }
  const sectionIds = Array.from(new Set([...scores.map((item) => item.sectionId), ...grades.map((item) => item.sectionId)]));

  if (!sectionIds.length) {
    return <EmptyState title="Chưa có điểm" detail="Bảng điểm sẽ hiển thị khi giảng viên nhập điểm thành phần hoặc điểm tổng kết." icon="stats-chart-outline" />;
  }

  return (
    <View>
      <Text style={studentUi.sectionTitle}>Điểm học phần</Text>
      {sectionIds.map((sectionId) => {
        const score = scores.find((item) => item.sectionId === sectionId);
        const sectionGrades = (gradesBySection.get(sectionId) ?? []).sort((a, b) => a.displayOrder - b.displayOrder);
        const title = score?.courseName ?? sectionGrades[0]?.courseName ?? "Học phần";
        const code = score?.courseCode ?? sectionGrades[0]?.courseCode ?? "";
        return (
          <View key={sectionId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{code}</Text>
              <StatusPill label={`${score?.credits ?? sectionGrades.length} tín chỉ`} />
            </View>
            <Text style={studentUi.title}>{title}</Text>
            <View style={studentUi.row}>
              <StatusPill label={`Tổng kết: ${formatNumber(score?.weightedScore10)}`} tone={score ? "success" : "muted"} />
              <StatusPill label={`GPA4: ${formatNumber(score?.gpa4)}`} tone={score?.gpa4 === null || score?.gpa4 === undefined ? "muted" : "primary"} />
            </View>
            {sectionGrades.map((grade) => (
              <View key={grade.gradeComponentId} style={styles.gradeRow}>
                <View style={studentUi.grow}>
                  <Text style={studentUi.text}>{grade.componentName}</Text>
                  <Text style={studentUi.muted}>Trọng số {formatNumber(grade.weightPercent, 1)}% · Tối đa {formatNumber(grade.maxScore)}</Text>
                  {grade.note ? <Text style={studentUi.muted}>Ghi chú: {grade.note}</Text> : null}
                </View>
                <Text style={styles.gradeScore}>{grade.score === null ? "-" : formatNumber(grade.score)}</Text>
              </View>
            ))}
          </View>
        );
      })}
    </View>
  );
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
  dayTabs: {
    gap: 8,
    paddingBottom: 10,
  },
  dayButton: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 44,
    paddingHorizontal: 13,
  },
  dayButtonActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  dayText: {
    color: colors.ink,
    fontSize: 13,
    fontWeight: "800",
  },
  dayTextActive: {
    color: "#FFFFFF",
  },
  scheduleCard: {
    flexDirection: "row",
    gap: 14,
  },
  timeRail: {
    alignItems: "center",
    borderRightColor: colors.border,
    borderRightWidth: 1,
    justifyContent: "center",
    minWidth: 74,
    paddingRight: 14,
  },
  timeText: {
    color: colors.ink,
    fontSize: 19,
    fontWeight: "800",
    marginTop: 4,
  },
  gradeRow: {
    alignItems: "center",
    borderTopColor: colors.border,
    borderTopWidth: 1,
    flexDirection: "row",
    gap: 12,
    paddingTop: 10,
  },
  gradeScore: {
    color: colors.primaryDark,
    fontSize: 22,
    fontWeight: "800",
    minWidth: 48,
    textAlign: "right",
  },
});
