import { getStudentAnnouncements, getStudentDashboard, markStudentAnnouncementAsRead, type StudentAnnouncement, type StudentDashboard } from "@/api/student";
import ActionTile from "@/components/ActionTile";
import AppScreen from "@/components/AppScreen";
import InfoCard from "@/components/InfoCard";
import RoleHomeHeader from "@/components/RoleHomeHeader";
import { colors } from "@/constants/theme";
import { authSession } from "@/features/auth/authSession";
import type { Href } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from "react-native";
import { EmptyState, FeedbackText, formatDate, formatDateTime, formatNumber, formatTime, StatusPill, studentUi } from "./components/StudentUI";
import { useStudentData } from "./useStudentData";

type StudentHomeData = {
  dashboard: StudentDashboard;
  announcements: StudentAnnouncement[];
};

const learningHref = "/student/learning" as Href;
const materialsHref = "/student/materials" as Href;
const tasksHref = "/student/tasks" as Href;

async function loadHome(): Promise<StudentHomeData> {
  const [dashboard, announcements] = await Promise.all([
    getStudentDashboard(),
    getStudentAnnouncements({ activeOnly: true, page: 1, pageSize: 4 }),
  ]);
  return { dashboard, announcements: announcements.items };
}

export default function StudentHomeScreen() {
  const state = useStudentData(loadHome);
  const [readBusyId, setReadBusyId] = useState<number | null>(null);
  const [localMessage, setLocalMessage] = useState("");
  const user = authSession.getUser();
  const dashboard = state.data?.dashboard;
  const student = dashboard?.student;
  const deadlines = dashboard?.deadlines ?? [];
  const exams = dashboard?.exams ?? [];
  const unreadAnnouncements = state.data?.announcements.filter((item) => !item.isRead).length ?? 0;

  async function markRead(announcementId: number) {
    if (readBusyId) return;
    setLocalMessage("");
    setReadBusyId(announcementId);
    try {
      await markStudentAnnouncementAsRead(announcementId);
      await state.refresh();
    } catch (error) {
      setLocalMessage(error instanceof Error ? error.message : "Không cập nhật được thông báo.");
    } finally {
      setReadBusyId(null);
    }
  }

  return (
    <AppScreen>
      <RoleHomeHeader
        eyebrow="Khu vực sinh viên"
        title={`Xin chào, ${student?.fullName ?? user?.fullName ?? "sinh viên"}`}
        subtitle="Theo dõi môn học, lịch học, lịch thi, bài tập, tài liệu, điểm số và mục tiêu học tập."
      />

      {state.loading ? (
        <ActivityIndicator color={colors.primary} style={styles.loader} />
      ) : null}
      <FeedbackText message={state.error || localMessage} />

      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="trending-up-outline" title="GPA hiện tại" value={formatNumber(student?.gpa)} tone="success" />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="time-outline" title="Deadline gần" value={String(deadlines.length)} tone="warning" />
        </View>
      </View>
      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="calendar-outline" title="Lịch thi" value={String(exams.length)} />
        </View>
        <View style={styles.metricItem}>
          <InfoCard icon="notifications-outline" title="Thông báo mới" value={String(unreadAnnouncements)} tone="accent" />
        </View>
      </View>

      <Text style={studentUi.sectionTitle}>Truy cập nhanh</Text>
      <ActionTile
        href={learningHref}
        icon="book-outline"
        title="Học tập"
        subtitle="Môn học, lịch học, lịch thi và điểm số"
      />
      <ActionTile
        href={materialsHref}
        icon="document-text-outline"
        title="Tài liệu học tập"
        subtitle="Tài liệu và liên kết từ giảng viên"
      />
      <ActionTile
        href={tasksHref}
        icon="checkmark-done-outline"
        title="Bài tập và mục tiêu"
        subtitle="Deadline, nộp bài, nhiệm vụ cá nhân và mục tiêu học tập"
      />

      <Text style={studentUi.sectionTitle}>Deadline gần nhất</Text>
      {deadlines.length ? (
        deadlines.map((item) => (
          <View key={item.assignmentId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{item.courseCode}</Text>
              <StatusPill label={item.submissionStatus} tone={item.submittedAt ? "success" : "warning"} />
            </View>
            <Text style={studentUi.title}>{item.title}</Text>
            <Text style={studentUi.muted}>{item.courseName}</Text>
            <Text style={studentUi.text}>Hạn nộp: {formatDateTime(item.dueAt)}</Text>
            <Text style={studentUi.muted}>Điểm: {item.score === null ? "Chưa chấm" : `${formatNumber(item.score)}/${formatNumber(item.maxScore)}`}</Text>
          </View>
        ))
      ) : (
        <EmptyState title="Chưa có deadline gần" detail="Các bài tập mới sẽ xuất hiện tại đây khi giảng viên công bố." icon="checkmark-circle-outline" />
      )}

      <Text style={studentUi.sectionTitle}>Lịch thi sắp tới</Text>
      {exams.length ? (
        exams.map((item) => (
          <View key={item.examId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{item.courseCode}</Text>
              <StatusPill label={formatTime(item.startTime)} />
            </View>
            <Text style={studentUi.title}>{item.examName}</Text>
            <Text style={studentUi.muted}>{item.courseName}</Text>
            <Text style={studentUi.text}>{formatDate(item.examDate)} · {item.durationMinutes} phút · Phòng {item.room ?? "chưa có"}</Text>
          </View>
        ))
      ) : (
        <EmptyState title="Chưa có lịch thi" detail="Lịch thi sẽ hiển thị khi phòng đào tạo hoặc giảng viên cập nhật." icon="calendar-clear-outline" />
      )}

      <Text style={studentUi.sectionTitle}>Thông báo</Text>
      {state.data?.announcements.length ? (
        state.data.announcements.map((item) => (
          <View key={item.announcementId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{item.courseCode ?? item.sectionCode ?? "Chung"}</Text>
              <StatusPill label={item.isRead ? "Đã đọc" : "Mới"} tone={item.isRead ? "muted" : "warning"} />
            </View>
            <Text style={studentUi.title}>{item.title}</Text>
            <Text style={studentUi.text}>{item.content}</Text>
            <Text style={studentUi.muted}>Đăng bởi {item.createdByFullName} · {formatDateTime(item.publishedAt)}</Text>
            {!item.isRead ? (
              <Pressable
                accessibilityRole="button"
                disabled={readBusyId === item.announcementId}
                onPress={() => void markRead(item.announcementId)}
                style={({ pressed }) => [
                  styles.readButton,
                  pressed ? { opacity: 0.76 } : null,
                  readBusyId === item.announcementId ? { opacity: 0.45 } : null,
                ]}
              >
                <Text style={styles.readButtonText}>{readBusyId === item.announcementId ? "Đang lưu..." : "Đánh dấu đã đọc"}</Text>
              </Pressable>
            ) : null}
          </View>
        ))
      ) : (
        <EmptyState title="Chưa có thông báo" detail="Thông báo từ lớp học sẽ được cập nhật tại đây." icon="notifications-outline" />
      )}
    </AppScreen>
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
  readButton: {
    alignItems: "center",
    alignSelf: "flex-start",
    borderColor: colors.primary,
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 44,
    paddingHorizontal: 14,
  },
  readButtonText: {
    color: colors.primary,
    fontSize: 14,
    fontWeight: "800",
  },
});
