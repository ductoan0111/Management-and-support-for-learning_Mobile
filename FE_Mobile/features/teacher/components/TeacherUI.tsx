import { Ionicons } from "@expo/vector-icons";
import { router } from "expo-router";
import { useState, type ComponentProps, type ReactNode } from "react";
import { ActivityIndicator, KeyboardAvoidingView, Modal, Platform, Pressable, RefreshControl, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { colors } from "@/constants/theme";

export const ui = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.background },
  content: { padding: 20, paddingBottom: 40, width: "100%", maxWidth: 900, alignSelf: "center", gap: 14 },
  row: { flexDirection: "row", alignItems: "center", gap: 10, flexWrap: "wrap" },
  grow: { flex: 1, minWidth: 0 },
  title: { color: colors.ink, fontSize: 23, fontWeight: "800" },
  heading: { color: colors.ink, fontSize: 17, fontWeight: "700" },
  text: { color: colors.ink, fontSize: 15, lineHeight: 23 },
  muted: { color: colors.muted, fontSize: 13, lineHeight: 21 },
  code: { color: colors.primary, fontSize: 13, fontWeight: "800" },
  card: { backgroundColor: colors.surface, borderWidth: 1, borderColor: colors.border, borderRadius: 8, padding: 16, gap: 8 },
  input: { backgroundColor: colors.surface, borderColor: colors.softBorder, borderWidth: 1, borderRadius: 8, minHeight: 48, padding: 12, color: colors.ink, fontSize: 15 },
  button: { minHeight: 44, paddingHorizontal: 14, paddingVertical: 10, borderRadius: 8, backgroundColor: colors.primary, flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 8 },
  buttonText: { color: "white", fontWeight: "700", fontSize: 14, flexShrink: 1 },
  icon: { height: 44, width: 44, alignItems: "center", justifyContent: "center", borderRadius: 8, backgroundColor: colors.surface, borderWidth: 1, borderColor: colors.border },
  error: { color: "#A32020", backgroundColor: "#FFF0F0", padding: 12, borderRadius: 8, lineHeight: 22 },
  success: { color: colors.success, backgroundColor: "#EAF7F0", padding: 12, borderRadius: 8 },
  overlay: { flex: 1, backgroundColor: "rgba(0,0,0,0.4)", justifyContent: "center", padding: 20 },
  modal: { maxHeight: "90%", width: "100%", maxWidth: 640, alignSelf: "center", backgroundColor: colors.surface, padding: 20, borderRadius: 8, gap: 14 },
});
export type IconName = ComponentProps<typeof Ionicons>["name"];
export function IconButton({ icon, label, onPress, disabled }: { icon: IconName; label: string; onPress: () => void; disabled?: boolean }) {
  return <Pressable accessibilityRole="button" accessibilityLabel={label} disabled={disabled} onPress={onPress} style={[ui.icon, disabled && { opacity: 0.4 }]}><Ionicons name={icon} size={21} color={colors.primary} /></Pressable>;
}
export function Button({ label, onPress, icon = "checkmark-outline", disabled }: { label: string; onPress: () => void; icon?: IconName; disabled?: boolean }) {
  return <Pressable accessibilityRole="button" onPress={onPress} disabled={disabled} style={[ui.button, disabled && { opacity: 0.45 }]}><Ionicons name={icon} color="white" size={18} /><Text style={ui.buttonText}>{label}</Text></Pressable>;
}
export function Page({ title, children, loading = false, error = "", refresh, action, home = false }: { title: string; children: ReactNode; loading?: boolean; error?: string; refresh?: () => void; action?: ReactNode; home?: boolean }) {
  return <SafeAreaView style={ui.page}><ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={ui.content} refreshControl={refresh ? <RefreshControl refreshing={loading} onRefresh={refresh} /> : undefined}>
    <View style={ui.row}>{!home && <IconButton icon="arrow-back" label="Quay lại" onPress={() => router.canGoBack() ? router.back() : router.replace("/teacher")} />}<Text style={[ui.title, ui.grow]}>{title}</Text>{action}{refresh && <IconButton icon="refresh" label="Tải lại" onPress={refresh} disabled={loading} />}</View>
    {!!error && <Text accessibilityRole="alert" style={ui.error}>{error}</Text>}
    {loading ? <ActivityIndicator color={colors.primary} style={{ padding: 20 }} /> : children}
  </ScrollView></SafeAreaView>;
}
export function Dialog({ title, children, onClose, busy = false }: { title: string; children: ReactNode; onClose: () => void; busy?: boolean }) {
  return <Modal transparent animationType="fade" onRequestClose={() => { if (!busy) onClose(); }}><KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined} style={ui.overlay}>
    <View style={ui.modal}><View style={ui.row}><Text style={[ui.heading, ui.grow]}>{title}</Text><IconButton icon="close" label="Đóng" onPress={onClose} disabled={busy} /></View><ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={{ gap: 14 }}>{children}</ScrollView></View>
  </KeyboardAvoidingView></Modal>;
}
export function Select({ label, value, options, onChange }: { label: string; value: string; options: { value: string; label: string }[]; onChange: (value: string) => void }) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  return <View style={{ gap: 6 }}><Text style={ui.muted}>{label}</Text><Pressable accessibilityRole="button" accessibilityLabel={label} onPress={() => { setSearch(""); setOpen(true); }} style={[ui.input, ui.row]}><Text style={[ui.text, ui.grow]}>{options.find(o => o.value === value)?.label ?? "Chọn..."}</Text><Ionicons name="chevron-down" size={18} color={colors.primary} /></Pressable>
    {open && <Dialog title={label} onClose={() => setOpen(false)}><TextInput accessibilityLabel="Tìm kiếm lựa chọn" placeholder="Tìm kiếm" style={ui.input} value={search} onChangeText={setSearch} />{options.filter(o => o.label.toLocaleLowerCase().includes(search.toLocaleLowerCase())).map(o => <Pressable accessibilityRole="button" key={o.value} onPress={() => { onChange(o.value); setOpen(false); }} style={[ui.row, { paddingVertical: 12 }]}><Text style={[ui.text, ui.grow]}>{o.label}</Text>{value === o.value && <Ionicons name="checkmark" size={20} color={colors.primary} />}</Pressable>)}{!options.length && <Text style={ui.muted}>Chưa có lựa chọn.</Text>}</Dialog>}
  </View>;
}
export const display = (value: unknown) => value === null || value === undefined || value === "" ? "—" : String(value);
export const dateText = (value: unknown) => value ? new Date(String(value)).toLocaleString("vi-VN") : "—";
