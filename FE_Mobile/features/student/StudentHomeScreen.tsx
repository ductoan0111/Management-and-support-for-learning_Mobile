import ActionTile from "@/components/ActionTile";
import AppScreen from "@/components/AppScreen";
import InfoCard from "@/components/InfoCard";
import RoleHomeHeader from "@/components/RoleHomeHeader";
import { colors } from "@/constants/theme";
import type { Href } from "expo-router";
import { StyleSheet, Text, View } from "react-native";

const learningHref = "/student/learning" as Href;
const tasksHref = "/student/tasks" as Href;

const upcomingDeadlines = [
  {
    course: "CT101",
    title: "Bài tập mô hình dữ liệu",
    due: "Hôm nay, 23:59",
  },
  {
    course: "ENG204",
    title: "Nộp thuyết trình nhóm",
    due: "Thứ sáu, 17:00",
  },
];

export default function StudentHomeScreen() {
  return (
    <AppScreen>
      <RoleHomeHeader
        eyebrow="Khu vực sinh viên"
        title="Xin chào, sinh viên"
        subtitle="Theo dõi lịch học, bài tập, điểm số và mục tiêu học tập."
      />

      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="calendar-outline" title="Lịch hôm nay" value="3" />
        </View>
        <View style={styles.metricItem}>
          <InfoCard
            icon="trending-up-outline"
            title="GPA hiện tại"
            value="3.42"
            tone="success"
          />
        </View>
      </View>

      <Text style={styles.sectionTitle}>Truy cập nhanh</Text>
      <ActionTile
        href={learningHref}
        icon="book-outline"
        title="Học tập"
        subtitle="Môn học, lịch học, lịch thi và điểm số"
      />
      <ActionTile
        href={tasksHref}
        icon="checkmark-done-outline"
        title="Bài tập và mục tiêu"
        subtitle="Deadline, nhiệm vụ cá nhân và mục tiêu học tập"
      />

      <Text style={styles.sectionTitle}>Deadline gần nhất</Text>
      <InfoCard icon="time-outline" title="Cần xử lý" tone="warning">
        {upcomingDeadlines.map((item) => (
          <View key={`${item.course}-${item.title}`} style={styles.deadlineRow}>
            <View style={styles.courseBadge}>
              <Text style={styles.courseText}>{item.course}</Text>
            </View>
            <View style={styles.deadlineCopy}>
              <Text style={styles.deadlineTitle}>{item.title}</Text>
              <Text style={styles.deadlineDue}>{item.due}</Text>
            </View>
          </View>
        ))}
      </InfoCard>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  metrics: {
    flexDirection: "row",
    gap: 12,
  },
  metricItem: {
    flex: 1,
  },
  sectionTitle: {
    color: colors.ink,
    fontSize: 17,
    fontWeight: "800",
    marginBottom: 10,
    marginTop: 8,
  },
  deadlineRow: {
    alignItems: "center",
    borderTopColor: colors.border,
    borderTopWidth: 1,
    flexDirection: "row",
    paddingVertical: 12,
  },
  courseBadge: {
    alignItems: "center",
    backgroundColor: "#FFF2EC",
    borderRadius: 8,
    justifyContent: "center",
    marginRight: 10,
    minHeight: 36,
    minWidth: 58,
    paddingHorizontal: 8,
  },
  courseText: {
    color: colors.accent,
    fontSize: 12,
    fontWeight: "800",
  },
  deadlineCopy: {
    flex: 1,
  },
  deadlineTitle: {
    color: colors.ink,
    fontSize: 14,
    fontWeight: "700",
  },
  deadlineDue: {
    color: colors.muted,
    fontSize: 13,
    marginTop: 3,
  },
});
