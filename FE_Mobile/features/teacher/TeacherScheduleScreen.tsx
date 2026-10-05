import { getExams, getSchedule, type ExamSchedule, type Schedule } from "@/api/teacher";
import { colors } from "@/constants/theme";
import { Ionicons } from "@expo/vector-icons";
import { useState } from "react";
import { Pressable, ScrollView, Text, View } from "react-native";
import { Page, display, ui } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

type ScheduleData = {
  schedules: Schedule[];
  exams: ExamSchedule[];
};

const dayLabel = (day: number) => day === 8 ? "CN" : `T${day}`;
const dayName = (day: number) => day === 8 ? "Chủ nhật" : `Thứ ${day}`;
const examTypeLabel = (type: number) => ({
  1: "Kiểm tra",
  2: "Giữa kỳ",
  3: "Cuối kỳ",
  4: "Khác",
}[type] ?? "Khác");

function formatDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value.slice(0, 10) : date.toLocaleDateString("vi-VN");
}

async function loadScheduleData(): Promise<ScheduleData> {
  const [schedules, exams] = await Promise.all([getSchedule(), getExams()]);
  return { schedules, exams };
}

function FilterPill({ label, active, onPress }: { label: string; active: boolean; onPress: () => void }) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ selected: active }}
      onPress={onPress}
      style={{
        alignItems: "center",
        backgroundColor: active ? colors.primary : colors.surface,
        borderColor: active ? colors.primary : colors.border,
        borderRadius: 14,
        borderWidth: 1,
        minWidth: 52,
        paddingHorizontal: 14,
        paddingVertical: 10,
      }}
    >
      <Text style={{ color: active ? "white" : colors.ink, fontWeight: "800" }}>{label}</Text>
    </Pressable>
  );
}

export default function TeacherScheduleScreen() {
  const state = useTeacherData(loadScheduleData);
  const [tab, setTab] = useState<"teaching" | "exams">("teaching");
  const [day, setDay] = useState("");
  const days = [{ value: "", label: "Tất cả" }, ...Array.from({ length: 7 }, (_, i) => ({ value: String(i + 2), label: dayLabel(i + 2) }))];
  const schedules = state.data?.schedules.filter(s => !day || String(s.dayOfWeek) === day) ?? [];
  const exams = state.data?.exams ?? [];

  return (
    <Page home title="Lịch dạy" {...state}>
      <View style={[ui.row, { flexWrap: "nowrap" }]}>
        <FilterPill label="Lịch dạy" active={tab === "teaching"} onPress={() => setTab("teaching")} />
        <FilterPill label="Lịch thi" active={tab === "exams"} onPress={() => setTab("exams")} />
      </View>

      {tab === "teaching" && (
        <>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={{ gap: 8 }}>
            {days.map(item => (
              <FilterPill
                key={item.value}
                label={item.label}
                active={day === item.value}
                onPress={() => setDay(item.value)}
              />
            ))}
          </ScrollView>
          {schedules.map(s => (
            <View key={s.scheduleId} style={[ui.card, { flexDirection: "row", gap: 14 }]}>
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
            </View>
          ))}
          {!schedules.length && <Text style={ui.muted}>Chưa có lịch dạy.</Text>}
        </>
      )}

      {tab === "exams" && (
        <>
          {exams.map(exam => (
            <View key={exam.examId} style={[ui.card, { gap: 10 }]}>
              <View style={ui.row}>
                <Text style={[ui.chip, { backgroundColor: "#FFF7E6", color: colors.warning }]}>{examTypeLabel(exam.examType)}</Text>
                <Text style={[ui.code, ui.grow]}>{exam.courseCode}</Text>
              </View>
              <Text style={ui.heading}>{exam.examName}</Text>
              <Text style={ui.text}>{exam.courseName}</Text>
              <Text style={ui.muted}>{exam.sectionCode}</Text>
              <View style={[ui.row, { gap: 6 }]}>
                <Ionicons name="calendar-outline" size={15} color={colors.muted} />
                <Text style={ui.muted}>{formatDate(exam.examDate)} · {exam.startTime.slice(0, 5)} · {exam.durationMinutes} phút</Text>
              </View>
              <View style={[ui.row, { gap: 6 }]}>
                <Ionicons name="location-outline" size={15} color={colors.muted} />
                <Text style={ui.muted}>Phòng {display(exam.room)}</Text>
              </View>
              {!!exam.note && <Text style={ui.text}>{exam.note}</Text>}
            </View>
          ))}
          {!exams.length && <Text style={ui.muted}>Chưa có lịch thi.</Text>}
        </>
      )}
    </Page>
  );
}
