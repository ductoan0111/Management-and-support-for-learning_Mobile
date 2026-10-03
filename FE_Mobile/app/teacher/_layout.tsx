import { Ionicons } from "@expo/vector-icons";
import { Redirect, Tabs } from "expo-router";
import type { ColorValue } from "react-native";
import { authSession } from "@/features/auth/authSession";
import { colors } from "@/constants/theme";

type IconName = React.ComponentProps<typeof Ionicons>["name"];

const tab = (title: string, icon: IconName) => ({
  title,
  tabBarIcon: ({ color, size }: { color: ColorValue; size: number }) => <Ionicons name={icon} size={size} color={color as string} />,
});

export default function TeacherLayout() {
  if (!authSession.getUser()?.teacherId || authSession.getRole() !== "teacher") return <Redirect href="/login" />;
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        sceneStyle: { backgroundColor: colors.background },
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        tabBarLabelStyle: { fontSize: 11, fontWeight: "700" },
        tabBarStyle: { backgroundColor: colors.surface, borderTopColor: colors.border, height: 62, paddingTop: 6, paddingBottom: 8 },
      }}
    >
      <Tabs.Screen name="index" options={tab("Trang chủ", "home-outline")} />
      <Tabs.Screen name="sections" options={tab("Lớp học", "school-outline")} />
      <Tabs.Screen name="schedule" options={tab("Lịch dạy", "calendar-outline")} />
      <Tabs.Screen name="grades" options={tab("Điểm", "stats-chart-outline")} />
      <Tabs.Screen name="profile" options={tab("Hồ sơ", "person-circle-outline")} />
      <Tabs.Screen name="section" options={{ href: null }} />
      <Tabs.Screen name="students" options={{ href: null }} />
    </Tabs>
  );
}
