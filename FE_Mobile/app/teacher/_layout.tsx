import { Redirect, Stack } from "expo-router";
import { authSession } from "@/features/auth/authSession";

export default function TeacherLayout() {
  if (!authSession.getUser()?.teacherId || authSession.getRole() !== "teacher") return <Redirect href="/login" />;
  return (
    <Stack
      screenOptions={{
        headerShown: false,
        contentStyle: { backgroundColor: "#F6F7FB" },
      }}
    />
  );
}
