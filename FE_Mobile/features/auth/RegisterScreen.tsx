import { registerStudentApi } from "@/api/auth";
import { colors, shadows } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { Link, router, type Href } from "expo-router";
import { useState } from "react";
import {
  ActivityIndicator,
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

const loginHref = "/login" as Href;
const studentHref = "/student" as Href;

export default function RegisterScreen() {
  const [fullName, setFullName] = useState("");
  const [studentCode, setStudentCode] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [accepted, setAccepted] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleRegister = async () => {
    if (
      !fullName.trim() ||
      !studentCode.trim() ||
      !email.trim() ||
      !password ||
      !confirmPassword
    ) {
      Alert.alert(
        "Thiếu thông tin",
        "Vui lòng nhập đầy đủ các trường bắt buộc.",
      );
      return;
    }

    if (!email.includes("@")) {
      Alert.alert("Email chưa hợp lệ", "Vui lòng kiểm tra lại email.");
      return;
    }

    if (password.length < 6) {
      Alert.alert("Mật khẩu ngắn", "Mật khẩu cần có ít nhất 6 ký tự.");
      return;
    }

    if (password !== confirmPassword) {
      Alert.alert("Mật khẩu không khớp", "Vui lòng nhập lại mật khẩu.");
      return;
    }

    if (!accepted) {
      Alert.alert(
        "Điều khoản",
        "Vui lòng xác nhận điều khoản sử dụng.",
      );
      return;
    }

    try {
      setIsSubmitting(true);
      const result = await registerStudentApi({
        fullName,
        studentCode,
        email,
        phone,
        password,
      });

      if (!result.success) {
        Alert.alert("Đăng ký thất bại", result.message ?? "Vui lòng thử lại.");
        return;
      }

      Alert.alert("Thành công", "Tạo tài khoản sinh viên thành công!", [
        {
          text: "Vào ứng dụng",
          onPress: () => router.replace(studentHref),
        },
      ]);
    } catch (err: any) {
      Alert.alert("Lỗi", "Không thể hoàn tất đăng ký. Vui lòng thử lại.");
    } finally {
      setIsSubmitting(false);
    }
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
              <Ionicons name="person-add-outline" size={30} color="#FFFFFF" />
            </View>
            <Text style={styles.appName}>Study Support</Text>
            <Text style={styles.title}>Đăng ký</Text>
            <Text style={styles.subtitle}>
              Tạo tài khoản sinh viên để theo dõi tiến độ học tập.
            </Text>
          </View>

          <View style={styles.form}>
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Họ và tên *</Text>
              <View style={styles.inputShell}>
                <Ionicons name="person-outline" size={20} color={colors.muted} />
                <TextInput
                  autoCapitalize="words"
                  onChangeText={setFullName}
                  placeholder="Nguyễn Văn A"
                  placeholderTextColor="#94A3B8"
                  style={styles.input}
                  value={fullName}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Mã sinh viên *</Text>
              <View style={styles.inputShell}>
                <Ionicons name="school-outline" size={20} color={colors.muted} />
                <TextInput
                  autoCapitalize="characters"
                  autoCorrect={false}
                  onChangeText={setStudentCode}
                  placeholder="SV001"
                  placeholderTextColor="#94A3B8"
                  style={styles.input}
                  value={studentCode}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Email *</Text>
              <View style={styles.inputShell}>
                <Ionicons name="mail-outline" size={20} color={colors.muted} />
                <TextInput
                  autoCapitalize="none"
                  autoCorrect={false}
                  keyboardType="email-address"
                  onChangeText={setEmail}
                  placeholder="email@school.edu"
                  placeholderTextColor="#94A3B8"
                  style={styles.input}
                  value={email}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Số điện thoại</Text>
              <View style={styles.inputShell}>
                <Ionicons name="call-outline" size={20} color={colors.muted} />
                <TextInput
                  keyboardType="phone-pad"
                  onChangeText={setPhone}
                  placeholder="0901234567"
                  placeholderTextColor="#94A3B8"
                  style={styles.input}
                  value={phone}
                />
              </View>
            </View>

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Mật khẩu *</Text>
              <View style={styles.inputShell}>
                <Ionicons
                  name="lock-closed-outline"
                  size={20}
                  color={colors.muted}
                />
                <TextInput
                  onChangeText={setPassword}
                  placeholder="Ít nhất 6 ký tự"
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

            <View style={styles.inputGroup}>
              <Text style={styles.label}>Nhập lại mật khẩu *</Text>
              <View style={styles.inputShell}>
                <Ionicons
                  name="shield-checkmark-outline"
                  size={20}
                  color={colors.muted}
                />
                <TextInput
                  onChangeText={setConfirmPassword}
                  placeholder="Nhập lại mật khẩu"
                  placeholderTextColor="#94A3B8"
                  secureTextEntry={!showPassword}
                  style={styles.input}
                  value={confirmPassword}
                />
              </View>
            </View>

            <Pressable
              accessibilityRole="checkbox"
              accessibilityState={{ checked: accepted }}
              onPress={() => setAccepted((value) => !value)}
              style={styles.checkRow}
            >
              <View
                style={[styles.checkbox, accepted ? styles.checkboxActive : null]}
              >
                {accepted ? (
                  <Ionicons name="checkmark" size={14} color="#FFFFFF" />
                ) : null}
              </View>
              <Text style={styles.checkText}>
                Tôi đồng ý với điều khoản sử dụng
              </Text>
            </Pressable>

            <Pressable
              onPress={handleRegister}
              disabled={isSubmitting}
              style={[styles.primaryButton, isSubmitting ? styles.buttonDisabled : null]}
            >
              {isSubmitting ? (
                <ActivityIndicator color="#FFFFFF" size="small" />
              ) : (
                <>
                  <Text style={styles.primaryButtonText}>Tạo tài khoản</Text>
                  <Ionicons name="arrow-forward" size={20} color="#FFFFFF" />
                </>
              )}
            </Pressable>

            <View style={styles.switchRow}>
              <Text style={styles.switchText}>Đã có tài khoản?</Text>
              <Link href={loginHref} asChild>
                <Pressable hitSlop={8}>
                  <Text style={styles.switchLink}>Đăng nhập</Text>
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
    marginBottom: 24,
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
  inputGroup: {
    marginBottom: 14,
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
  checkRow: {
    alignItems: "center",
    flexDirection: "row",
    marginBottom: 18,
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
    flex: 1,
    fontSize: 14,
    fontWeight: "600",
    lineHeight: 20,
  },
  primaryButton: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 8,
    flexDirection: "row",
    justifyContent: "center",
    minHeight: 52,
  },
  buttonDisabled: {
    opacity: 0.6,
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
