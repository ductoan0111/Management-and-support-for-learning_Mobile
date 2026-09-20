import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { Link, type Href } from "expo-router";
import type { ComponentProps } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";

type IconName = ComponentProps<typeof Ionicons>["name"];

type ActionTileProps = {
  href: Href;
  icon: IconName;
  title: string;
  subtitle: string;
};

export default function ActionTile({
  href,
  icon,
  title,
  subtitle,
}: ActionTileProps) {
  return (
    <Link href={href} asChild>
      <Pressable style={styles.tile}>
        <View style={styles.iconWrap}>
          <Ionicons name={icon} size={21} color={colors.primary} />
        </View>
        <View style={styles.copy}>
          <Text style={styles.title}>{title}</Text>
          <Text style={styles.subtitle}>{subtitle}</Text>
        </View>
        <Ionicons name="chevron-forward" size={18} color={colors.muted} />
      </Pressable>
    </Link>
  );
}

const styles = StyleSheet.create({
  tile: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: "row",
    marginBottom: 10,
    minHeight: 74,
    paddingHorizontal: 14,
  },
  iconWrap: {
    alignItems: "center",
    backgroundColor: "#E8F3F4",
    borderRadius: 8,
    height: 42,
    justifyContent: "center",
    marginRight: 12,
    width: 42,
  },
  copy: {
    flex: 1,
  },
  title: {
    color: colors.ink,
    fontSize: 15,
    fontWeight: "800",
  },
  subtitle: {
    color: colors.muted,
    fontSize: 13,
    lineHeight: 18,
    marginTop: 3,
  },
});
