import { getSchedule } from "@/api/teacher";
import { useState } from "react";
import { Text, View } from "react-native";
import { Page, Select, display, ui } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

export default function TeacherScheduleScreen() {
  const state = useTeacherData(getSchedule);
  const [day, setDay] = useState("");
  const days = [{value:"",label:"Tất cả các ngày"}, ...Array.from({length:7}, (_, i) => ({value:String(i+2),label:i===6 ? "Chủ nhật" : `Thứ ${i+2}`}))];
  const rows = state.data?.filter(s => !day || String(s.dayOfWeek) === day) ?? [];
  return <Page title="Lịch dạy" {...state}><Select label="Ngày trong tuần" value={day} options={days} onChange={setDay} />
    {rows.map(s => <View key={s.scheduleId} style={ui.card}><Text style={ui.code}>{s.dayOfWeek === 8 ? "Chủ nhật" : `Thứ ${s.dayOfWeek}`} · {s.startTime.slice(0,5)} – {s.endTime.slice(0,5)}</Text><Text style={ui.heading}>{s.courseName}</Text><Text style={ui.text}>{s.sectionCode} · Phòng {display(s.room)} · {display(s.building)}</Text><Text style={ui.muted}>{s.effectiveFrom} → {s.effectiveTo}</Text>{!!s.note && <Text style={ui.text}>{s.note}</Text>}</View>)}
    {!rows.length && <Text style={ui.muted}>Chưa có lịch dạy.</Text>}
  </Page>;
}
