import { getStudentMaterials } from "@/api/student";
import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useCallback, useState } from "react";
import { ActivityIndicator, Linking, Pressable, StyleSheet, Text, TextInput, View } from "react-native";
import { EmptyState, FeedbackText, formatDateTime, httpUrl, IconButton, StatusPill, studentUi } from "./components/StudentUI";
import { useStudentData } from "./useStudentData";

export default function StudentMaterialsScreen() {
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [linkError, setLinkError] = useState("");
  const loadMaterials = useCallback(() => getStudentMaterials({ search: appliedSearch.trim() || undefined }), [appliedSearch]);
  const state = useStudentData(loadMaterials);
  const materials = state.data ?? [];

  async function openLink(url: string) {
    setLinkError("");
    try {
      await Linking.openURL(url);
    } catch {
      setLinkError("Không mở được liên kết. Vui lòng thử lại.");
    }
  }

  return (
    <AppScreen>
      <BackHeader title="Tài liệu học tập" subtitle="Xem tài liệu, file và liên kết từ các lớp học phần" />

      <View style={styles.searchRow}>
        <View style={styles.inputWrap}>
          <Ionicons name="search-outline" size={18} color={colors.muted} />
          <TextInput
            accessibilityLabel="Tìm tài liệu"
            autoCapitalize="none"
            onChangeText={setSearch}
            onSubmitEditing={() => setAppliedSearch(search)}
            placeholder="Tìm theo tiêu đề hoặc mô tả"
            placeholderTextColor="#94A3B8"
            returnKeyType="search"
            style={styles.searchInput}
            value={search}
          />
        </View>
        <IconButton icon="search" label="Tìm kiếm tài liệu" onPress={() => setAppliedSearch(search)} />
        <IconButton icon="refresh" label="Tải lại tài liệu" disabled={state.loading} onPress={() => void state.refresh()} />
      </View>

      <View style={studentUi.splitRow}>
        <Text style={studentUi.sectionTitle}>{state.loading ? "Đang tải..." : `${materials.length} tài liệu`}</Text>
        {appliedSearch ? <StatusPill label={`Từ khóa: ${appliedSearch}`} tone="muted" /> : null}
      </View>
      {state.loading ? <ActivityIndicator color={colors.primary} style={styles.loader} /> : null}
      <FeedbackText message={state.error || linkError} />

      {!state.loading && !state.error && !materials.length ? (
        <EmptyState
          title="Chưa có tài liệu phù hợp"
          detail="Thử đổi từ khóa tìm kiếm hoặc chờ giảng viên công bố tài liệu mới."
          icon="document-text-outline"
        />
      ) : null}

      {materials.map((material) => {
        const fileUrl = httpUrl(material.fileUrl);
        const externalUrl = httpUrl(material.externalUrl);
        return (
          <View key={material.materialId} style={studentUi.flatCard}>
            <View style={studentUi.splitRow}>
              <Text style={studentUi.code}>{material.courseCode}</Text>
              {material.materialType ? <StatusPill label={material.materialType} /> : null}
            </View>
            <Text style={studentUi.title}>{material.title}</Text>
            <Text style={studentUi.muted}>{material.courseName}</Text>
            {material.description ? <Text style={studentUi.text}>{material.description}</Text> : null}
            <Text style={studentUi.muted}>Đăng bởi {material.uploadedByFullName} · {formatDateTime(material.createdAt)}</Text>
            <View style={styles.actions}>
              {fileUrl ? (
                <Pressable
                  accessibilityRole="link"
                  onPress={() => void openLink(fileUrl)}
                  style={({ pressed }) => [styles.linkButton, pressed ? { opacity: 0.76 } : null]}
                >
                  <Ionicons name="document-attach-outline" size={18} color="#FFFFFF" />
                  <Text style={styles.linkText}>Mở tài liệu</Text>
                </Pressable>
              ) : null}
              {externalUrl && externalUrl !== fileUrl ? (
                <Pressable
                  accessibilityRole="link"
                  onPress={() => void openLink(externalUrl)}
                  style={({ pressed }) => [styles.linkButton, pressed ? { opacity: 0.76 } : null]}
                >
                  <Ionicons name="open-outline" size={18} color="#FFFFFF" />
                  <Text style={styles.linkText}>Mở liên kết</Text>
                </Pressable>
              ) : null}
            </View>
            {!fileUrl && !externalUrl ? (
              <Text style={styles.invalidLink}>Tài liệu này chưa có liên kết HTTP hợp lệ.</Text>
            ) : null}
          </View>
        );
      })}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  searchRow: {
    alignItems: "center",
    flexDirection: "row",
    gap: 8,
    marginBottom: 14,
  },
  inputWrap: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    flexDirection: "row",
    minHeight: 48,
    paddingHorizontal: 12,
  },
  searchInput: {
    color: colors.ink,
    flex: 1,
    fontSize: 15,
    minHeight: 46,
    paddingHorizontal: 8,
  },
  loader: {
    marginBottom: 12,
  },
  actions: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
    marginTop: 2,
  },
  linkButton: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 8,
    flexDirection: "row",
    gap: 7,
    minHeight: 44,
    paddingHorizontal: 13,
  },
  linkText: {
    color: "#FFFFFF",
    fontSize: 14,
    fontWeight: "800",
  },
  invalidLink: {
    color: colors.danger,
    fontSize: 13,
    lineHeight: 20,
  },
});
