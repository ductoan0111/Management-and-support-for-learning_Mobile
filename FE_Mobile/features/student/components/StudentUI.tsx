import { colors, shadows } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import type { ComponentProps } from "react";
import { Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

export type IconName = ComponentProps<typeof Ionicons>["name"];

export const studentUi = StyleSheet.create({
  row: {
    alignItems: "center",
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
  },
  splitRow: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    justifyContent: "space-between",
  },
  grow: {
    flex: 1,
    minWidth: 0,
  },
  sectionTitle: {
    color: colors.ink,
    fontSize: 17,
    fontWeight: "800",
    marginBottom: 10,
    marginTop: 6,
  },
  card: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    gap: 10,
    marginBottom: 12,
    padding: 16,
    ...shadows.card,
  },
  flatCard: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    gap: 10,
    marginBottom: 12,
    padding: 16,
  },
  mutedCard: {
    backgroundColor: colors.surfaceMuted,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    padding: 12,
  },
  iconButton: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    height: 44,
    justifyContent: "center",
    width: 44,
  },
  button: {
    alignItems: "center",
    borderRadius: 8,
    flexDirection: "row",
    gap: 7,
    justifyContent: "center",
    minHeight: 44,
    paddingHorizontal: 14,
  },
  buttonText: {
    fontSize: 14,
    fontWeight: "800",
  },
  title: {
    color: colors.ink,
    fontSize: 16,
    fontWeight: "800",
    lineHeight: 22,
  },
  text: {
    color: colors.ink,
    fontSize: 14,
    lineHeight: 21,
  },
  muted: {
    color: colors.muted,
    fontSize: 13,
    lineHeight: 20,
  },
  code: {
    color: colors.primary,
    fontSize: 12,
    fontWeight: "800",
    letterSpacing: 0,
    textTransform: "uppercase",
  },
  input: {
    backgroundColor: colors.surfaceMuted,
    borderColor: colors.softBorder,
    borderRadius: 8,
    borderWidth: 1,
    color: colors.ink,
    fontSize: 15,
    minHeight: 48,
    paddingHorizontal: 12,
    paddingVertical: 10,
  },
  multilineInput: {
    minHeight: 96,
    textAlignVertical: "top",
  },
  error: {
    backgroundColor: "#FFF1F1",
    borderColor: "#FECACA",
    borderRadius: 8,
    borderWidth: 1,
    color: colors.danger,
    fontSize: 14,
    lineHeight: 21,
    marginBottom: 12,
    padding: 12,
  },
  success: {
    backgroundColor: "#ECFDF3",
    borderColor: "#BBF7D0",
    borderRadius: 8,
    borderWidth: 1,
    color: colors.success,
    fontSize: 14,
    lineHeight: 21,
    marginBottom: 12,
    padding: 12,
  },
});

export type SegmentOption<T extends string> = {
  value: T;
  label: string;
  icon: IconName;
};

export function IconButton({
  icon,
  label,
  onPress,
  disabled,
}: {
  icon: IconName;
  label: string;
  onPress: () => void;
  disabled?: boolean;
}) {
  return (
    <Pressable
      accessibilityLabel={label}
      accessibilityRole="button"
      disabled={disabled}
      onPress={onPress}
      style={({ pressed }) => [
        studentUi.iconButton,
        pressed && !disabled ? { opacity: 0.72 } : null,
        disabled ? { opacity: 0.45 } : null,
      ]}
    >
      <Ionicons name={icon} size={21} color={colors.primary} />
    </Pressable>
  );
}

export function ActionButton({
  label,
  icon,
  onPress,
  disabled,
  variant = "primary",
}: {
  label: string;
  icon: IconName;
  onPress: () => void;
  disabled?: boolean;
  variant?: "primary" | "outline" | "danger" | "ghost";
}) {
  const backgroundColor = variant === "primary" ? colors.primary : variant === "danger" ? colors.danger : colors.surface;
  const borderColor = variant === "ghost" ? "transparent" : variant === "primary" ? colors.primary : variant === "danger" ? colors.danger : colors.border;
  const foreground = variant === "primary" || variant === "danger" ? "#FFFFFF" : variant === "ghost" ? colors.muted : colors.primary;

  return (
    <Pressable
      accessibilityRole="button"
      disabled={disabled}
      onPress={onPress}
      style={({ pressed }) => [
        studentUi.button,
        { backgroundColor, borderColor, borderWidth: 1 },
        pressed && !disabled ? { opacity: 0.76 } : null,
        disabled ? { opacity: 0.45 } : null,
      ]}
    >
      <Ionicons name={icon} size={18} color={foreground} />
      <Text style={[studentUi.buttonText, { color: foreground }]}>{label}</Text>
    </Pressable>
  );
}

export function SegmentedTabs<T extends string>({
  options,
  value,
  onChange,
}: {
  options: SegmentOption<T>[];
  value: T;
  onChange: (value: T) => void;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={{ gap: 8, paddingBottom: 4 }}
    >
      {options.map((item) => {
        const selected = item.value === value;
        return (
          <Pressable
            accessibilityRole="tab"
            accessibilityState={{ selected }}
            key={item.value}
            onPress={() => onChange(item.value)}
            style={({ pressed }) => [
              studentUi.row,
              {
                backgroundColor: selected ? colors.primary : colors.surface,
                borderColor: selected ? colors.primary : colors.border,
                borderRadius: 8,
                borderWidth: 1,
                flexWrap: "nowrap",
                minHeight: 44,
                paddingHorizontal: 13,
              },
              pressed ? { opacity: 0.76 } : null,
            ]}
          >
            <Ionicons name={item.icon} size={16} color={selected ? "#FFFFFF" : colors.primary} />
            <Text style={{ color: selected ? "#FFFFFF" : colors.ink, fontSize: 13, fontWeight: "800" }}>
              {item.label}
            </Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

export function StatusPill({
  label,
  tone = "primary",
}: {
  label: string;
  tone?: "primary" | "success" | "warning" | "danger" | "muted";
}) {
  const toneMap = {
    primary: { bg: "#E8F3F4", fg: colors.primary },
    success: { bg: "#EAF7F0", fg: colors.success },
    warning: { bg: "#FFF7E6", fg: colors.warning },
    danger: { bg: "#FFF2EC", fg: colors.danger },
    muted: { bg: colors.surfaceMuted, fg: colors.muted },
  };
  const toneColor = toneMap[tone];
  return (
    <Text
      style={{
        alignSelf: "flex-start",
        backgroundColor: toneColor.bg,
        borderRadius: 8,
        color: toneColor.fg,
        fontSize: 12,
        fontWeight: "800",
        overflow: "hidden",
        paddingHorizontal: 9,
        paddingVertical: 5,
      }}
    >
      {label}
    </Text>
  );
}

export function EmptyState({ title, detail, icon = "file-tray-outline" }: { title: string; detail?: string; icon?: IconName }) {
  return (
    <View style={[studentUi.flatCard, { alignItems: "center", paddingVertical: 22 }]}>
      <Ionicons name={icon} size={28} color={colors.muted} />
      <Text style={[studentUi.title, { textAlign: "center" }]}>{title}</Text>
      {detail ? <Text style={[studentUi.muted, { textAlign: "center" }]}>{detail}</Text> : null}
    </View>
  );
}

export function FeedbackText({ message, type = "error" }: { message: string; type?: "error" | "success" }) {
  if (!message) return null;
  return <Text accessibilityRole="alert" style={type === "error" ? studentUi.error : studentUi.success}>{message}</Text>;
}

export function display(value: unknown) {
  return value === null || value === undefined || value === "" ? "Chưa có" : String(value);
}

export function formatNumber(value: number | null | undefined, digits = 2) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return "Chưa có";
  return Number(value).toLocaleString("vi-VN", { maximumFractionDigits: digits });
}

export function formatDate(value: string | null | undefined) {
  if (!value) return "Chưa có";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value.slice(0, 10);
  return date.toLocaleDateString("vi-VN");
}

export function formatDateTime(value: string | null | undefined) {
  if (!value) return "Chưa có";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString("vi-VN", { dateStyle: "short", timeStyle: "short" });
}

export function formatTime(value: string | null | undefined) {
  if (!value) return "Chưa có";
  return value.length >= 5 ? value.slice(0, 5) : value;
}

export function dayLabel(day: number) {
  if (day === 1 || day === 8) return "Chủ nhật";
  return `Thứ ${day}`;
}

export function examTypeLabel(type: number) {
  return {
    1: "Giữa kỳ",
    2: "Cuối kỳ",
    3: "Thực hành",
    4: "Khác",
  }[type] ?? "Khác";
}

export function goalTypeLabel(type: number) {
  return {
    1: "GPA",
    2: "Điểm môn",
    3: "Tín chỉ",
    4: "Thói quen học",
    5: "Khác",
  }[type] ?? "Khác";
}

export function goalStatusLabel(status: number) {
  return {
    1: "Đang thực hiện",
    2: "Hoàn thành",
    3: "Tạm dừng",
  }[status] ?? "Không rõ";
}

export function taskStatusLabel(status: number) {
  return {
    1: "Chưa làm",
    2: "Đang làm",
    3: "Hoàn thành",
    4: "Tạm dừng",
  }[status] ?? "Không rõ";
}

export function priorityLabel(priority: number) {
  return {
    1: "Thấp",
    2: "Vừa",
    3: "Cao",
  }[priority] ?? "Vừa";
}

export function statusTone(labelOrStatus: string | number | null | undefined): "success" | "warning" | "danger" | "muted" | "primary" {
  const value = String(labelOrStatus ?? "").toLocaleLowerCase("vi-VN");
  if (value.includes("quá") || value.includes("muộn") || value === "4") return "danger";
  if (value.includes("chưa") || value.includes("đang") || value === "1" || value === "2") return "warning";
  if (value.includes("hoàn") || value.includes("chấm") || value.includes("nộp") || value === "3") return "success";
  return "primary";
}

export function httpUrl(value: string | null | undefined): string | null {
  if (!value) return null;
  try {
    const url = new URL(value.trim());
    return (url.protocol === "https:" || url.protocol === "http:") && url.hostname ? url.href : null;
  } catch {
    return null;
  }
}

export function todayInputDate() {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
}
