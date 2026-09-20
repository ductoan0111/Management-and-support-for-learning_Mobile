import ActionTile from "@/components/ActionTile";
import AppScreen from "@/components/AppScreen";
import InfoCard from "@/components/InfoCard";
import RoleHomeHeader from "@/components/RoleHomeHeader";
import { colors } from "@/constants/theme";
import type { Href } from "expo-router";
import { StyleSheet, Text, View } from "react-native";

const sectionsHref = "/teacher/sections" as Href;
const studentsHref = "/teacher/students" as Href;

const todayClasses = [
  {
    code: "SE205",
    name: "Phát triển ứng dụng di động",
    time: "13:00 - 15:30",
    room: "Lab B1",
  },
  {
    code: "CT101",
    name: "Cơ sở dữ liệu",
    time: "15:45 - 17:15",
    room: "A203",
  },
];

export default function TeacherHomeScreen() {
  return (
    <AppScreen>
      <RoleHomeHeader
        eyebrow="Khu vực giảng viên"
        title="Lớp học của tôi"
        subtitle="Quản lý lớp phụ trách và theo dõi danh sách sinh viên."
      />

      <View style={styles.metrics}>
        <View style={styles.metricItem}>
          <InfoCard icon="albums-outline" title="Lớp phụ trách" value="4" />
        </View>
        <View style={styles.metricItem}>
          <InfoCard
            icon="people-outline"
            title="Sinh viên"
            value="128"
            tone="accent"
          />
        </View>
      </View>

      <Text style={styles.sectionTitle}>Truy cập nhanh</Text>
      <ActionTile
        href={sectionsHref}
        icon="library-outline"
        title="Danh sách lớp"
        subtitle="Xem lớp học phần đang phụ trách"
      />
      <ActionTile
        href={studentsHref}
        icon="people-circle-outline"
        title="Sinh viên theo lớp"
        subtitle="Tra cứu danh sách sinh viên của một lớp"
      />

      <Text style={styles.sectionTitle}>Lịch dạy hôm nay</Text>
      {todayClasses.map((item) => (
        <View key={item.code} style={styles.classCard}>
          <View style={styles.classBadge}>
            <Text style={styles.classCode}>{item.code}</Text>
          </View>
          <View style={styles.classCopy}>
            <Text style={styles.className}>{item.name}</Text>
            <Text style={styles.classMeta}>
              {item.time} · {item.room}
            </Text>
          </View>
        </View>
      ))}
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
  classCard: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: "row",
    marginBottom: 10,
    padding: 14,
  },
  classBadge: {
    alignItems: "center",
    backgroundColor: "#FFF2EC",
    borderRadius: 8,
    height: 44,
    justifyContent: "center",
    marginRight: 12,
    minWidth: 62,
  },
  classCode: {
    color: colors.accent,
    fontSize: 12,
    fontWeight: "800",
  },
  classCopy: {
    flex: 1,
  },
  className: {
    color: colors.ink,
    fontSize: 14,
    fontWeight: "800",
  },
  classMeta: {
    color: colors.muted,
    fontSize: 13,
    marginTop: 4,
  },
});
