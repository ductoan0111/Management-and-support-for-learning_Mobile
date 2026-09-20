import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import { colors } from "@/constants/theme";
import { StyleSheet, Text, View } from "react-native";

const sections = [
  {
    id: "1001",
    code: "SE205.01",
    name: "Phát triển ứng dụng di động",
    students: 42,
    room: "Lab B1",
  },
  {
    id: "1002",
    code: "CT101.03",
    name: "Cơ sở dữ liệu",
    students: 38,
    room: "A203",
  },
  {
    id: "1003",
    code: "PRJ301.02",
    name: "Dự án phần mềm",
    students: 31,
    room: "B405",
  },
];

export default function TeacherSectionsScreen() {
  return (
    <AppScreen>
      <BackHeader
        title="Danh sách lớp"
        subtitle="Các lớp học phần giảng viên đang phụ trách"
      />

      {sections.map((section) => (
        <View key={section.id} style={styles.card}>
          <View style={styles.cardHeader}>
            <Text style={styles.code}>{section.code}</Text>
            <Text style={styles.count}>{section.students} SV</Text>
          </View>
          <Text style={styles.name}>{section.name}</Text>
          <Text style={styles.meta}>Mã lớp: {section.id}</Text>
          <Text style={styles.meta}>Phòng học: {section.room}</Text>
        </View>
      ))}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
  },
  cardHeader: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 10,
  },
  code: {
    color: colors.primary,
    fontSize: 13,
    fontWeight: "800",
  },
  count: {
    color: colors.accent,
    fontSize: 13,
    fontWeight: "800",
  },
  name: {
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
