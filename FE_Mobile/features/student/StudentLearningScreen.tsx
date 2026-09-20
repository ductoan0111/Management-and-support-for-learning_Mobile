import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import InfoCard from "@/components/InfoCard";
import { colors } from "@/constants/theme";
import { StyleSheet, Text, View } from "react-native";

const sections = [
  {
    code: "CT101",
    name: "Cơ sở dữ liệu",
    schedule: "Thứ 2, 07:30 - 10:00",
    room: "A203",
    score: "8.6",
  },
  {
    code: "SE205",
    name: "Phát triển ứng dụng di động",
    schedule: "Thứ 4, 13:00 - 15:30",
    room: "Lab B1",
    score: "9.0",
  },
  {
    code: "ENG204",
    name: "Tiếng Anh chuyên ngành",
    schedule: "Thứ 6, 09:30 - 11:30",
    room: "C102",
    score: "7.8",
  },
];

export default function StudentLearningScreen() {
  return (
    <AppScreen>
      <BackHeader
        title="Học tập"
        subtitle="Môn học, lịch học, lịch thi và điểm số"
      />

      <InfoCard icon="bar-chart-outline" title="Tổng quan điểm" value="3.42">
        <Text style={styles.helperText}>GPA tạm tính trong học kỳ hiện tại</Text>
      </InfoCard>

      {sections.map((section) => (
        <View key={section.code} style={styles.sectionCard}>
          <View style={styles.sectionHeader}>
            <View style={styles.codeBadge}>
              <Text style={styles.codeText}>{section.code}</Text>
            </View>
            <Text style={styles.score}>{section.score}</Text>
          </View>
          <Text style={styles.courseName}>{section.name}</Text>
          <Text style={styles.meta}>{section.schedule}</Text>
          <Text style={styles.meta}>Phòng {section.room}</Text>
        </View>
      ))}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  helperText: {
    color: colors.muted,
    fontSize: 13,
  },
  sectionCard: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
  },
  sectionHeader: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 10,
  },
  codeBadge: {
    backgroundColor: "#E8F3F4",
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
  },
  codeText: {
    color: colors.primary,
    fontSize: 12,
    fontWeight: "800",
  },
  score: {
    color: colors.success,
    fontSize: 20,
    fontWeight: "800",
  },
  courseName: {
    color: colors.ink,
    fontSize: 16,
    fontWeight: "800",
    marginBottom: 8,
  },
  meta: {
    color: colors.muted,
    fontSize: 13,
    lineHeight: 20,
  },
});
