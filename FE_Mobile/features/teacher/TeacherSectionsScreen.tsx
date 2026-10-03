import { getSections } from "@/api/teacher";
import { router, type Href } from "expo-router";
import { useState } from "react";
import { Pressable, Text, TextInput } from "react-native";
import { useTeacherData } from "./useTeacherData";
import { Page, Select, ui } from "./components/TeacherUI";

export default function TeacherSectionsScreen() {
  const state = useTeacherData(getSections);
  const [search, setSearch] = useState("");
  const [semester, setSemester] = useState("");
  const [status, setStatus] = useState("");
  const labels = ["Đã đóng", "Đang mở", "Đã kết thúc"];
  const semesters = [...new Map(state.data?.map(s => [String(s.semesterId), `${s.semesterName} · ${s.academicYear}`])).entries()];
  const rows = state.data?.filter(s => (!semester || String(s.semesterId) === semester) && (!status || String(s.status) === status) && `${s.sectionCode} ${s.courseName}`.toLocaleLowerCase().includes(search.toLocaleLowerCase())) ?? [];
  return <Page home title="Lớp học phần" {...state}>
    <TextInput accessibilityLabel="Tìm lớp" style={ui.input} placeholder="Tìm mã lớp, tên môn học" value={search} onChangeText={setSearch} />
    <Select label="Học kỳ" value={semester} onChange={setSemester} options={[{value:"",label:"Tất cả học kỳ"}, ...semesters.map(([value,label]) => ({value,label}))]} />
    <Select label="Trạng thái" value={status} onChange={setStatus} options={[{value:"",label:"Tất cả trạng thái"}, ...labels.map((label,index) => ({value:String(index),label}))]} />
    {rows.map(s => <Pressable accessibilityRole="button" key={s.sectionId} style={ui.card} onPress={() => router.push(`/teacher/section?sectionId=${s.sectionId}` as Href)}><Text style={ui.code}>{s.sectionCode} · {labels[s.status]}</Text><Text style={ui.heading}>{s.courseName}</Text><Text style={ui.muted}>{s.semesterName} · {s.academicYear}</Text><Text style={ui.text}>{s.enrolledCount}{s.maxStudents ? ` / ${s.maxStudents}` : ""} sinh viên · {s.credits} tín chỉ</Text><Text style={ui.muted}>{s.isPrimary ? "Giảng viên chính" : "Giảng viên phối hợp"}</Text></Pressable>)}
    {!rows.length && <Text style={ui.muted}>Không có lớp phù hợp.</Text>}
  </Page>;
}
