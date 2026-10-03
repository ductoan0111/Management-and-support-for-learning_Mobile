import { getSchedule } from "@/api/teacher";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useState } from "react";
import { Pressable, ScrollView, Text, View } from "react-native";
import { Page, display, ui } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

const dayLabel = (day: number) => day === 8 ? "CN" : `T${day}`;
const dayName = (day: number) => day === 8 ? "Chủ nhật" : `Thứ ${day}`;

export default function TeacherScheduleScreen() {
  const state = useTeacherData(getSchedule);
  const [day, setDay] = useState("");
  const days = [{ value: "", label: "Tất cả" }, ...Array.from({ length: 7 }, (_, i) => ({ value: String(i + 2), label: dayLabel(i + 2) }))];
  const rows = state.data?.filter(s => !day || String(s.dayOfWeek) === day) ?? [];
  return <Page home title="Lịch dạy" {...state}>
    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 8 }}>
      {days.map(item => <Pressable
        key={item.value}
        accessibilityRole="button"
        accessibilityState={{ selected: day === item.value }}
        onPress={() => setDay(item.value)}
        style={{ minWidth: 52, alignItems: "center", paddingHorizontal: 14, paddingVertical: 10, borderRadius: 14, backgroundColor: day === item.value ? colors.primary : colors.surface, borderWidth: 1, borderColor: day === item.value ? colors.primary : colors.border }}
      >
        <Text style={{ color: day === item.value ? "white" : colors.ink, fontWeight: "800" }}>{item.label}</Text>
      </Pressable>)}
    </ScrollView>
    {rows.map(s => <View key={s.scheduleId} style={[ui.card, { flexDirection: "row", gap: 14 }]}>
      <View style={{ alignItems: "center", justifyContent: "center", minWidth: 64, paddingRight: 14, borderRightWidth: 1, borderRightColor: colors.border }}>
        <Text style={ui.code}>{dayName(s.dayOfWeek)}</Text>
        <Text style={[ui.heading, { marginTop: 4 }]}>{s.startTime.slice(0, 5)}</Text>
        <Text style={ui.muted}>{s.endTime.slice(0, 5)}</Text>
      </View>
      <View style={ui.grow}>
        <Text style={ui.heading}>{s.courseName}</Text>
        <Text style={ui.chip}>{s.sectionCode}</Text>
        <View style={[ui.row, { gap: 6, marginTop: 6 }]}>
          <Ionicons name="location-outline" size={15} color={colors.muted} />
          <Text style={ui.muted}>Phòng {display(s.room)} · {display(s.building)}</Text>
        </View>
        <Text style={ui.muted}>{s.effectiveFrom} → {s.effectiveTo}</Text>
        {!!s.note && <Text style={ui.text}>{s.note}</Text>}
      </View>
    </View>)}
    {!rows.length && <Text style={ui.muted}>Chưa có lịch dạy.</Text>}
  </Page>;
}
