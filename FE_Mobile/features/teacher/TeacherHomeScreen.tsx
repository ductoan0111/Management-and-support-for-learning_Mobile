import { getSections } from "@/api/teacher";
import { authSession } from "@/features/auth/authSession";
import { router, type Href } from "expo-router";
import { Image, Pressable, Text, View } from "react-native";
import { IconButton, Page, ui } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

const quickLinks: { route: Href; label: string; detail: string }[] = [
  { route: "/teacher/sections" as Href, label: "Lớp học phần", detail: "Danh sách lớp phụ trách" },
  { route: "/teacher/grades" as Href, label: "Bảng điểm", detail: "Thành phần, trọng số, tổng kết" },
  { route: "/teacher/schedule" as Href, label: "Lịch dạy", detail: "Thời khóa biểu học kỳ" },
  { route: "/teacher/students" as Href, label: "Sinh viên", detail: "Danh sách theo lớp" },
  { route: "/teacher/profile" as Href, label: "Hồ sơ", detail: "Thông tin tài khoản" },
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
    <View style={ui.row}>
      <Image source={user?.avatarUrl ? { uri: user.avatarUrl } : require("@/assets/images/icon.png")} style={{ width: 52, height: 52, borderRadius: 8 }} />
      <View style={ui.grow}>
        <Text style={ui.code}>GIẢNG VIÊN</Text>
        <Text style={ui.heading}>{user?.fullName}</Text>
        <Text style={ui.muted}>{user?.email}</Text>
      </View>
    </View>

    <View style={ui.row}>
      <View style={[ui.grow, ui.card]}>
        <Text style={ui.muted}>Lớp phụ trách</Text>
        <Text style={ui.title}>{state.data?.length ?? 0}</Text>
      </View>
      <View style={[ui.grow, ui.card]}>
        <Text style={ui.muted}>Lượt đăng ký</Text>
        <Text style={ui.title}>{totalEnrollments}</Text>
      </View>
    </View>

    <Text style={ui.heading}>Giảng dạy</Text>
    {quickLinks.map(item => <Pressable accessibilityRole="button" key={String(item.route)} style={ui.card} onPress={() => router.push(item.route)}>
      <Text style={ui.heading}>{item.label}</Text>
      <Text style={ui.muted}>{item.route === "/teacher/profile" ? user?.email ?? item.detail : item.detail}</Text>
    </Pressable>)}

    <Text style={ui.heading}>Lớp đang mở</Text>
    {state.data?.filter(section => section.status === 1).map(section => <Pressable
      accessibilityRole="button"
      key={section.sectionId}
      style={ui.card}
      onPress={() => router.push(`/teacher/section?sectionId=${section.sectionId}` as Href)}
    >
      <Text style={ui.code}>{section.sectionCode}</Text>
      <Text style={ui.heading}>{section.courseName}</Text>
      <Text style={ui.muted}>{section.semesterName} · {section.enrolledCount} sinh viên</Text>
    </Pressable>)}
    {!state.data?.some(section => section.status === 1) && <Text style={ui.muted}>Chưa có lớp đang mở.</Text>}
  </Page>;
}
