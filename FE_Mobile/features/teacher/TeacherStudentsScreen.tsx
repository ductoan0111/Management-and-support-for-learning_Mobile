import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useState } from "react";
import { Pressable, StyleSheet, Text, TextInput, View } from "react-native";

const students = [
  {
    id: "SV001",
    name: "Nguyễn Văn A",
    email: "sv001@school.edu",
    score: "8.6",
  },
  {
    id: "SV014",
    name: "Trần Thị B",
    email: "sv014@school.edu",
    score: "9.1",
  },
  {
    id: "SV027",
    name: "Lê Minh C",
    email: "sv027@school.edu",
    score: "7.9",
  },
];

export default function TeacherStudentsScreen() {
  const [sectionId, setSectionId] = useState("1001");

  return (
    <AppScreen>
      <BackHeader
        title="Sinh viên theo lớp"
        subtitle="Tra cứu danh sách sinh viên bằng mã lớp học phần"
      />

      <View style={styles.searchBox}>
        <View style={styles.inputShell}>
          <Ionicons name="search-outline" size={20} color={colors.muted} />
          <TextInput
            keyboardType="number-pad"
            onChangeText={setSectionId}
            placeholder="Nhập mã lớp học phần"
            placeholderTextColor="#94A3B8"
            style={styles.input}
            value={sectionId}
          />
        </View>
        <Pressable style={styles.searchButton}>
          <Ionicons name="refresh-outline" size={20} color="#FFFFFF" />
        </Pressable>
      </View>

      {students.map((student) => (
        <View key={student.id} style={styles.studentCard}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>{student.name.slice(0, 1)}</Text>
          </View>
          <View style={styles.studentCopy}>
            <Text style={styles.studentName}>{student.name}</Text>
            <Text style={styles.studentMeta}>
              {student.id} · {student.email}
            </Text>
          </View>
          <Text style={styles.score}>{student.score}</Text>
        </View>
      ))}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  searchBox: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    marginBottom: 16,
  },
  inputShell: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.softBorder,
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    flexDirection: "row",
    minHeight: 52,
    paddingHorizontal: 14,
  },
  input: {
    color: colors.ink,
    flex: 1,
    fontSize: 15,
    minHeight: 50,
    paddingHorizontal: 10,
  },
  searchButton: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 8,
    height: 52,
    justifyContent: "center",
    width: 52,
  },
  studentCard: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: "row",
    marginBottom: 10,
    padding: 14,
  },
  avatar: {
    alignItems: "center",
    backgroundColor: "#E8F3F4",
    borderRadius: 8,
    height: 44,
    justifyContent: "center",
    marginRight: 12,
    width: 44,
  },
  avatarText: {
    color: colors.primary,
    fontSize: 18,
    fontWeight: "800",
  },
  studentCopy: {
    flex: 1,
  },
  studentName: {
    color: colors.ink,
    fontSize: 15,
    fontWeight: "800",
  },
  studentMeta: {
    color: colors.muted,
    fontSize: 12,
    lineHeight: 18,
    marginTop: 3,
  },
  score: {
    color: colors.success,
    fontSize: 18,
    fontWeight: "800",
    marginLeft: 10,
  },
});
