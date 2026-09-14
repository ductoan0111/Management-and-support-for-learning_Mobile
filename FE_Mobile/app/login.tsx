import { Ionicons } from "@expo/vector-icons";
import { Link, type Href } from "expo-router";
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

const registerHref = "/register" as Href;

export default function LoginScreen() {
  const [identifier, setIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(true);

  const handleLogin = () => {
    if (!identifier.trim() || !password) {
      Alert.alert("Thiếu thông tin", "Vui lòng nhập tài khoản và mật khẩu.");
      return;
    }

    Alert.alert("Đăng nhập", "Form đã sẵn sàng để kết nối API xác thực.");
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
              Truy cập hồ sơ học tập và lịch học của bạn.
            </Text>
          </View>

          <View style={styles.form}>
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Email hoặc mã sinh viên</Text>
              <View style={styles.inputShell}>
                <Ionicons name="person-outline" size={20} color="#64748B" />
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
                <Ionicons name="lock-closed-outline" size={20} color="#64748B" />
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
    flex: 1,
    backgroundColor: "#F6F7FB",
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
    backgroundColor: "#246B73",
    borderRadius: 18,
    height: 62,
    justifyContent: "center",
    marginBottom: 18,
    width: 62,
  },
  appName: {
    color: "#D96C4A",
    fontSize: 14,
    fontWeight: "800",
    letterSpacing: 0,
    marginBottom: 8,
    textTransform: "uppercase",
  },
  title: {
    color: "#17202A",
    fontSize: 34,
    fontWeight: "800",
    letterSpacing: 0,
    marginBottom: 8,
  },
  subtitle: {
    color: "#64748B",
    fontSize: 15,
    lineHeight: 22,
    maxWidth: 310,
    textAlign: "center",
  },
  form: {
    backgroundColor: "#FFFFFF",
    borderColor: "#E2E8F0",
    borderRadius: 8,
    borderWidth: 1,
    padding: 20,
    shadowColor: "#17202A",
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.08,
    shadowRadius: 24,
    elevation: 3,
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
    backgroundColor: "#F8FAFC",
    borderColor: "#CBD5E1",
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: "row",
    minHeight: 52,
    paddingHorizontal: 14,
  },
  input: {
    color: "#17202A",
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
    backgroundColor: "#246B73",
    borderColor: "#246B73",
  },
  checkText: {
    color: "#475569",
    fontSize: 14,
    fontWeight: "600",
  },
  linkText: {
    color: "#D96C4A",
    fontSize: 14,
    fontWeight: "700",
  },
  primaryButton: {
    alignItems: "center",
    backgroundColor: "#246B73",
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
    color: "#64748B",
    fontSize: 14,
    marginRight: 6,
  },
  switchLink: {
    color: "#246B73",
    fontSize: 14,
    fontWeight: "800",
  },
});
