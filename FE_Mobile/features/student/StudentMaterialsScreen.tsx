import { studentRequest, type StudentMaterial } from "@/api/student";
import AppScreen from "@/components/AppScreen";
import BackHeader from "@/components/BackHeader";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useEffect, useState } from "react";
import { Linking, Pressable, StyleSheet, Text, View } from "react-native";

function httpUrl(value: string | null): string | null {
  if (!value) return null;
  try {
    const url = new URL(value.trim());
    return (url.protocol === "https:" || url.protocol === "http:") && url.hostname
      ? url.href
      : null;
  } catch {
    return null;
  }
}

export default function StudentMaterialsScreen() {
  const [materials, setMaterials] = useState<StudentMaterial[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [linkError, setLinkError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    void studentRequest<StudentMaterial[]>("/materials")
      .then((items) => {
        if (active) setMaterials(items);
      })
      .catch((reason: unknown) => {
        if (!active) return;
        setMaterials([]);
        setError(reason instanceof Error ? reason.message : "Không tải được tài liệu.");
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, [refreshKey]);

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
      <BackHeader title="Tài liệu học tập" subtitle="Tài liệu từ các lớp học phần của bạn" />
      <View style={styles.toolbar}>
        <Text style={styles.count}>{loading ? "Đang tải..." : `${materials.length} tài liệu`}</Text>
        <Pressable
          accessibilityLabel="Tải lại tài liệu"
          accessibilityRole="button"
          disabled={loading}
          onPress={() => setRefreshKey((key) => key + 1)}
          style={[styles.refreshButton, loading && styles.disabled]}
        >
          <Ionicons name="refresh" size={20} color={colors.primary} />
        </Pressable>
      </View>

      {loading ? <Text style={styles.message}>Đang tải tài liệu...</Text> : null}
      {!loading && error ? <Text accessibilityRole="alert" style={styles.error}>{error}</Text> : null}
      {linkError ? <Text accessibilityRole="alert" style={styles.error}>{linkError}</Text> : null}
      {!loading && !error && materials.length === 0 ? (
        <Text style={styles.message}>Chưa có tài liệu nào được giảng viên công bố.</Text>
      ) : null}

      {!loading && !error && materials.map((material) => {
        const fileUrl = httpUrl(material.fileUrl);
        const externalUrl = httpUrl(material.externalUrl);
        return (
          <View key={material.materialId} style={styles.card}>
            <Text style={styles.course}>{material.courseCode} · {material.courseName}</Text>
            <Text style={styles.title}>{material.title}</Text>
            {material.description ? <Text style={styles.description}>{material.description}</Text> : null}
            {material.materialType ? <Text style={styles.type}>{material.materialType}</Text> : null}
            <View style={styles.actions}>
              {fileUrl ? (
                <Pressable
                  accessibilityRole="link"
                  onPress={() => void openLink(fileUrl)}
                  style={styles.linkButton}
                >
                  <Ionicons name="open-outline" size={17} color={colors.surface} />
                  <Text style={styles.linkText}>Mở tài liệu</Text>
                </Pressable>
              ) : null}
              {externalUrl && externalUrl !== fileUrl ? (
                <Pressable
                  accessibilityRole="link"
                  onPress={() => void openLink(externalUrl)}
                  style={styles.linkButton}
                >
                  <Ionicons name="open-outline" size={17} color={colors.surface} />
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
  toolbar: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 14,
  },
  count: {
    color: colors.muted,
    fontSize: 14,
  },
  refreshButton: {
    alignItems: "center",
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    height: 40,
    justifyContent: "center",
    width: 40,
  },
  disabled: { opacity: 0.5 },
  message: {
    color: colors.muted,
    fontSize: 14,
    lineHeight: 21,
    marginTop: 10,
  },
  error: {
    color: colors.danger,
    fontSize: 14,
    lineHeight: 21,
    marginBottom: 12,
  },
  card: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderRadius: 8,
    borderWidth: 1,
    marginBottom: 12,
    padding: 16,
  },
  course: {
    color: colors.primary,
    fontSize: 13,
    fontWeight: "700",
    marginBottom: 7,
  },
  title: {
    color: colors.ink,
    fontSize: 17,
    fontWeight: "800",
  },
  description: {
    color: colors.muted,
    fontSize: 14,
    lineHeight: 21,
    marginTop: 7,
  },
  type: {
    color: colors.muted,
    fontSize: 12,
    marginTop: 8,
  },
  actions: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
    marginTop: 12,
  },
  linkButton: {
    alignItems: "center",
    backgroundColor: colors.primary,
    borderRadius: 8,
    flexDirection: "row",
    gap: 6,
    minHeight: 40,
    paddingHorizontal: 12,
  },
  linkText: {
    color: colors.surface,
    fontSize: 14,
    fontWeight: "700",
  },
  invalidLink: {
    color: colors.danger,
    fontSize: 13,
    marginTop: 8,
  },
});
