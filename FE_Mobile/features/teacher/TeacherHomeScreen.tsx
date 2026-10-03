import { getSections } from "@/api/teacher";
import { authSession } from "@/features/auth/authSession";
import { Ionicons } from "@expo/vector-icons";
import { colors } from "@/constants/theme";
import { router, type Href } from "expo-router";
import { Image, Pressable, Text, View } from "react-native";
import { IconButton, Page, ui, type IconName } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

const quickLinks: { route: Href; label: string; detail: string; icon: IconName }[] = [
  { route: "/teacher/sections" as Href, label: "Lớp học phần", detail: "Danh sách lớp phụ trách", icon: "school-outline" },
  { route: "/teacher/grades" as Href, label: "Bảng điểm", detail: "Thành phần, trọng số, tổng kết", icon: "stats-chart-outline" },
  { route: "/teacher/schedule" as Href, label: "Lịch dạy", detail: "Thời khóa biểu học kỳ", icon: "calendar-outline" },
  { route: "/teacher/students" as Href, label: "Sinh viên", detail: "Danh sách theo lớp", icon: "people-outline" },
  { route: "/teacher/profile" as Href, label: "Hồ sơ", detail: "Thông tin tài khoản", icon: "person-circle-outline" },
];

export default function TeacherHomeScreen() {
  const state = useTeacherData(getSections);
  const user = authSession.getUser();
  const totalEnrollments = state.data?.reduce((sum, section) => sum + section.enrolledCount, 0) ?? 0;

  return <Page
    home
    title="Study Support"
    {...state}
    action={<IconButton icon="log-out-outline" label="Đăng xuất" onPress={() => { authSession.logout(); router.replace("/login"); }} />}
  >
    <View style={ui.hero}>
      <View style={ui.row}>
        <Image source={user?.avatarUrl ? { uri: user.avatarUrl } : require("@/assets/images/icon.png")} style={{ width: 60, height: 60, borderRadius: 30, borderWidth: 2, borderColor: "rgba(255,255,255,0.6)" }} />
        <View style={ui.grow}>
          <Text style={[ui.heroMuted, { fontWeight: "800", letterSpacing: 1 }]}>GIẢNG VIÊN</Text>
          <Text style={ui.heroText}>{user?.fullName}</Text>
          <Text style={ui.heroMuted}>{user?.email}</Text>
        </View>
      </View>
      <View style={ui.row}>
        <View style={[ui.grow, { backgroundColor: "rgba(255,255,255,0.15)", borderRadius: 16, padding: 14 }]}>
          <Text style={ui.heroMuted}>Lớp phụ trách</Text>
          <Text style={[ui.heroText, { fontSize: 28 }]}>{state.data?.length ?? 0}</Text>
        </View>
        <View style={[ui.grow, { backgroundColor: "rgba(255,255,255,0.15)", borderRadius: 16, padding: 14 }]}>
          <Text style={ui.heroMuted}>Lượt đăng ký</Text>
          <Text style={[ui.heroText, { fontSize: 28 }]}>{totalEnrollments}</Text>
        </View>
      </View>
    </View>

    <Text style={ui.heading}>Giảng dạy</Text>
    <View style={ui.row}>
      {quickLinks.map(item => <Pressable accessibilityRole="button" key={String(item.route)} style={({ pressed }) => [ui.card, ui.tile, pressed && { opacity: 0.7 }]} onPress={() => router.push(item.route)}>
        <View style={ui.tileIcon}><Ionicons name={item.icon} size={22} color={colors.primary} /></View>
        <Text style={ui.heading}>{item.label}</Text>
        <Text style={ui.muted} numberOfLines={2}>{item.route === "/teacher/profile" ? user?.email ?? item.detail : item.detail}</Text>
      </Pressable>)}
    </View>

    <Text style={ui.heading}>Lớp đang mở</Text>
    {state.data?.filter(section => section.status === 1).map(section => <Pressable
      accessibilityRole="button"
      key={section.sectionId}
      style={({ pressed }) => [ui.card, pressed && { opacity: 0.7 }]}
      onPress={() => router.push(`/teacher/section?sectionId=${section.sectionId}` as Href)}
    >
      <Text style={ui.chip}>{section.sectionCode}</Text>
      <Text style={ui.heading}>{section.courseName}</Text>
      <Text style={ui.muted}>{section.semesterName} · {section.enrolledCount} sinh viên</Text>
    </Pressable>)}
    {!state.data?.some(section => section.status === 1) && <Text style={ui.muted}>Chưa có lớp đang mở.</Text>}
  </Page>;
}
