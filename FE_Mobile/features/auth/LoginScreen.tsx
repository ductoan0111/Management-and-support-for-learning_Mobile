import { colors, shadows } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { Link, router, type Href } from "expo-router";
import type { ComponentProps } from "react";
import { useState } from "react";
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  SafeAreaView,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";

type LoginRole = "student" | "teacher";
type IconName = ComponentProps<typeof Ionicons>["name"];

type RoleOption = {
  id: LoginRole;
  label: string;
  icon: IconName;
};

const registerHref = "/register" as Href;
const studentHref = "/student" as Href;
const teacherHref = "/teacher" as Href;

const roleOptions: RoleOption[] = [
  { id: "student", label: "Sinh viên", icon: "school-outline" },
  { id: "teacher", label: "Giảng viên", icon: "people-outline" },
];

export default function LoginScreen() {
  const [identifier, setIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(true);
  const [role, setRole] = useState<LoginRole>("student");

  const handleLogin = () => {
    if (!identifier.trim() || !password) {
      Alert.alert("Thiếu thông tin", "Vui lòng nhập tài khoản và mật khẩu.");
      return;
    }

    router.replace(role === "student" ? studentHref : teacherHref);
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : undefined}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.header}>
            <View style={styles.logoMark}>
              <Ionicons name="school-outline" size={30} color="#FFFFFF" />
            </View>
            <Text style={styles.appName}>Study Support</Text>
            <Text style={styles.title}>Đăng nhập</Text>
            <Text style={styles.subtitle}>
              Truy cập khu vực học tập theo đúng vai trò của bạn.
            </Text>
          </View>

          <View style={styles.form}>
            <Text style={styles.groupTitle}>Vai trò</Text>
            <View style={styles.roleGrid}>
              {roleOptions.map((item) => {
                const active = item.id === role;
                return (
                  <Pressable
                    key={item.id}
                    onPress={() => setRole(item.id)}
                    style={[styles.roleButton, active ? styles.roleActive : null]}
                  >
                    <Ionicons
                      name={item.icon}
                      size={18}
                      color={active ? "#FFFFFF" : colors.primary}
                    />
                    <Text
                      style={[
                        styles.roleText,
                        active ? styles.roleTextActive : null,
                      ]}
                    >
                      {item.label}
                    </Text>
                  </Pressable>
                );
              })}
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Email hoặc mã tài khoản</Text>
              <View style={styles.inputShell}>
                <Ionicons name="person-outline" size={20} color={colors.muted} />
                <TextInput
                  autoCapitalize="none"
                  autoCorrect={false}
                  keyboardType="email-address"
                  onChangeText={setIdentifier}
                  placeholder="sv001 hoặc email@school.edu"
                  placeholderTextColor="#94A3B8"
                  style={styles.input}
                  value={identifier}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Mật khẩu</Text>
              <View style={styles.inputShell}>
                <Ionicons
                  name="lock-closed-outline"
                  size={20}
                  color={colors.muted}
                />
                <TextInput
                  onChangeText={setPassword}
                  placeholder="Nhập mật khẩu"
                  placeholderTextColor="#94A3B8"
                  secureTextEntry={!showPassword}
                  style={styles.input}
                  value={password}
                />
                <Pressable
                  accessibilityLabel={
                    showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"
                  }
                  hitSlop={10}
                  onPress={() => setShowPassword((value) => !value)}
                  style={styles.iconButton}
                >
                  <Ionicons
                    name={showPassword ? "eye-off-outline" : "eye-outline"}
                    size={20}
                    color="#475569"
                  />
                </Pressable>
              </View>
            </View>

            <View style={styles.formRow}>
              <Pressable
                accessibilityRole="checkbox"
                accessibilityState={{ checked: rememberMe }}
                onPress={() => setRememberMe((value) => !value)}
                style={styles.checkRow}
              >
                <View
                  style={[
                    styles.checkbox,
                    rememberMe ? styles.checkboxActive : null,
                  ]}
                >
                  {rememberMe ? (
                    <Ionicons name="checkmark" size={14} color="#FFFFFF" />
                  ) : null}
                </View>
                <Text style={styles.checkText}>Ghi nhớ</Text>
              </Pressable>

              <Pressable hitSlop={8}>
                <Text style={styles.linkText}>Quên mật khẩu?</Text>
              </Pressable>
            </View>

            <Pressable onPress={handleLogin} style={styles.primaryButton}>
              <Text style={styles.primaryButtonText}>Đăng nhập</Text>
              <Ionicons name="log-in-outline" size={20} color="#FFFFFF" />
            </Pressable>

            <View style={styles.switchRow}>
              <Text style={styles.switchText}>Chưa có tài khoản?</Text>
              <Link href={registerHref} asChild>
                <Pressable hitSlop={8}>
                  <Text style={styles.switchLink}>Đăng ký</Text>
                </Pressable>
              </Link>
            </View>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    backgroundColor: colors.background,
    flex: 1,
  },
  keyboardView: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: "center",
    paddingHorizontal: 24,
    paddingVertical: 28,
  },
  header: {
    alignItems: "center",
    marginBottom: 28,
  },
  logoMark: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 18,
    height: 62,
    justifyContent: "center",
    marginBottom: 18,
    width: 62,
  },
  appName: {
    color: colors.accent,
    fontSize: 14,
    fontWeight: "800",
    letterSpacing: 0,
    marginBottom: 8,
    textTransform: "uppercase",
  },
  title: {
    color: colors.ink,
    fontSize: 34,
    fontWeight: "800",
    letterSpacing: 0,
    marginBottom: 8,
  },
  subtitle: {
    color: colors.muted,
    fontSize: 15,
    lineHeight: 22,
    maxWidth: 320,
    textAlign: "center",
  },
  form: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    padding: 20,
    ...shadows.card,
  },
  groupTitle: {
    color: colors.ink,
    fontSize: 14,
    fontWeight: "800",
    marginBottom: 10,
  },
  roleGrid: {
    flexDirection: "row",
    gap: 8,
    marginBottom: 18,
  },
  roleButton: {
    alignItems: "center",
    backgroundColor: colors.surfaceMuted,
    borderColor: colors.softBorder,
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    minHeight: 64,
    justifyContent: "center",
    paddingHorizontal: 6,
  },
  roleActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  roleText: {
    color: colors.primary,
    fontSize: 12,
    fontWeight: "800",
    marginTop: 6,
    textAlign: "center",
  },
  roleTextActive: {
    color: "#FFFFFF",
  },
  inputGroup: {
    marginBottom: 16,
  },
  label: {
    color: "#334155",
    fontSize: 14,
    fontWeight: "700",
    marginBottom: 8,
  },
  inputShell: {
    alignItems: "center",
    backgroundColor: colors.surfaceMuted,
    borderColor: colors.softBorder,
    borderRadius: 8,
    borderWidth: 1,
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
  iconButton: {
    alignItems: "center",
    height: 36,
    justifyContent: "center",
    width: 36,
  },
  formRow: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 18,
  },
  checkRow: {
    alignItems: "center",
    flexDirection: "row",
    minHeight: 36,
  },
  checkbox: {
    alignItems: "center",
    borderColor: "#94A3B8",
    borderRadius: 6,
    borderWidth: 1,
    height: 22,
    justifyContent: "center",
    marginRight: 8,
    width: 22,
  },
  checkboxActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  checkText: {
    color: "#475569",
    fontSize: 14,
    fontWeight: "600",
  },
  linkText: {
    color: colors.accent,
    fontSize: 14,
    fontWeight: "700",
  },
  primaryButton: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 8,
    flexDirection: "row",
    justifyContent: "center",
    minHeight: 52,
  },
  primaryButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "800",
    marginRight: 8,
  },
  switchRow: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "center",
    marginTop: 18,
  },
  switchText: {
    color: colors.muted,
    fontSize: 14,
    marginRight: 6,
  },
  switchLink: {
    color: colors.primary,
    fontSize: 14,
    fontWeight: "800",
  },
});
