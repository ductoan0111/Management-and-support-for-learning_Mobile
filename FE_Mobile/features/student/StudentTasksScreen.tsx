import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import InfoCard from "@/components/InfoCard";
import { colors } from "@/constants/theme";
import { Pressable, StyleSheet, Text, View } from "react-native";

const assignments = [
  {
    title: "Bài tập mô hình dữ liệu",
    course: "CT101",
    status: "Chưa nộp",
    due: "Hôm nay, 23:59",
  },
  {
    title: "Thuyết trình nhóm",
    course: "ENG204",
    status: "Đang làm",
    due: "20/09/2026",
  },
];

const goals = [
  "Hoàn thành toàn bộ bài tập trước hạn",
  "Duy trì GPA trên 3.3",
  "Ôn thi 45 phút mỗi ngày",
];

export default function StudentTasksScreen() {
  return (
    <AppScreen>
      <BackHeader
        title="Bài tập"
        subtitle="Theo dõi deadline, nhiệm vụ và mục tiêu học tập"
      />

      <InfoCard icon="flag-outline" title="Mục tiêu học tập" tone="success">
        {goals.map((goal) => (
          <View key={goal} style={styles.goalRow}>
            <View style={styles.dot} />
            <Text style={styles.goalText}>{goal}</Text>
          </View>
        ))}
      </InfoCard>

      {assignments.map((assignment) => (
        <Pressable key={assignment.title} style={styles.assignmentCard}>
          <View style={styles.assignmentHeader}>
            <Text style={styles.course}>{assignment.course}</Text>
            <Text style={styles.status}>{assignment.status}</Text>
          </View>
          <Text style={styles.assignmentTitle}>{assignment.title}</Text>
          <Text style={styles.due}>Hạn nộp: {assignment.due}</Text>
        </Pressable>
      ))}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  goalRow: {
    alignItems: "center",
    flexDirection: "row",
    paddingVertical: 7,
  },
  dot: {
    backgroundColor: colors.success,
    borderRadius: 4,
    height: 8,
    marginRight: 10,
    width: 8,
  },
  goalText: {
    color: colors.ink,
    flex: 1,
    fontSize: 14,
    fontWeight: "600",
  },
  assignmentCard: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
  },
  assignmentHeader: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 10,
  },
  course: {
    color: colors.primary,
    fontSize: 13,
    fontWeight: "800",
  },
  status: {
    color: colors.warning,
    fontSize: 13,
    fontWeight: "800",
  },
  assignmentTitle: {
    color: colors.ink,
    fontSize: 16,
    fontWeight: "800",
    marginBottom: 8,
  },
  due: {
    color: colors.muted,
    fontSize: 13,
  },
});
