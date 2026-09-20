import { colors, shadows } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import type { ComponentProps, PropsWithChildren } from "react";
import { StyleSheet, Text, View } from "react-native";

type IconName = ComponentProps<typeof Ionicons>["name"];

type InfoCardProps = PropsWithChildren<{
  icon?: IconName;
  title: string;
  value?: string;
  tone?: "primary" | "accent" | "success" | "warning";
}>;

const toneColors = {
  primary: colors.primary,
  accent: colors.accent,
  success: colors.success,
  warning: colors.warning,
};

export default function InfoCard({
  children,
  icon,
  title,
  value,
  tone = "primary",
}: InfoCardProps) {
  const toneColor = toneColors[tone];

  return (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        {icon ? (
          <View style={[styles.iconBadge, { backgroundColor: toneColor }]}>
            <Ionicons name={icon} size={18} color="#FFFFFF" />
          </View>
        ) : null}
        <Text style={styles.title}>{title}</Text>
      </View>
      {value ? <Text style={styles.value}>{value}</Text> : null}
      {children ? <View style={styles.body}>{children}</View> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
    ...shadows.card,
  },
  cardHeader: {
    alignItems: "center",
    flexDirection: "row",
  },
  iconBadge: {
    alignItems: "center",
    borderRadius: 8,
    height: 34,
    justifyContent: "center",
    marginRight: 10,
    width: 34,
  },
  title: {
    color: colors.ink,
    flex: 1,
    fontSize: 15,
    fontWeight: "800",
  },
  value: {
    color: colors.primaryDark,
    fontSize: 28,
    fontWeight: "800",
    marginTop: 12,
  },
  body: {
    marginTop: 12,
  },
});
