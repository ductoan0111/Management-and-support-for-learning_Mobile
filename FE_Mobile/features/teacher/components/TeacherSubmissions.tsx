import { teacherRequest, type Row } from "@/api/teacher";
import { useCallback, useState } from "react";
import { Linking, Text, View } from "react-native";
import { useTeacherData } from "../useTeacherData";
import { Button, Dialog, IconButton, Select, dateText, display, ui } from "./TeacherUI";
import { RecordEditor } from "./RecordEditor";

export function TeacherSubmissions({sectionId,assignment,onClose}:{sectionId:string;assignment:Row;onClose:()=>void}) {
  const path = `/sections/${sectionId}/assignments/${assignment.assignmentId}/submissions`;
  const load = useCallback(() => teacherRequest<Row[]>(path),[path]);
  const state = useTeacherData(load);
  const [filter,setFilter] = useState("");
  const [editing,setEditing] = useState<Row|null>(null);
  const [error,setError] = useState("");
  const rows = state.data?.filter(r => filter === "" || (filter === "graded" ? r.score !== null : r.score === null)) ?? [];
  return <Dialog title={`Bài nộp: ${assignment.title}`} onClose={onClose}>
    <Select label="Trạng thái chấm" value={filter} onChange={setFilter} options={[{value:"",label:"Tất cả"},{value:"graded",label:"Đã chấm"},{value:"pending",label:"Chưa chấm"}]} />
    <IconButton label="Tải lại" icon="refresh" onPress={() => void state.refresh()} disabled={state.loading} />
    {!!(state.error||error) && <Text style={ui.error}>{state.error||error}</Text>}
    {state.loading ? <Text style={ui.muted}>Đang tải...</Text> : rows.map(r => <View key={String(r.submissionId)} style={{gap:8,borderBottomWidth:1,borderBottomColor:"#E2E8F0",paddingBottom:16}}><Text style={ui.heading}>{r.fullName} · {r.studentCode}</Text><Text style={ui.muted}>{dateText(r.submittedAt)}{r.isLate ? " · Nộp muộn" : ""}</Text><Text style={ui.text}>{display(r.textContent)}</Text>{!!r.fileUrl && <Button label="Mở bài nộp" icon="open-outline" onPress={() => { const url=String(r.fileUrl); if(!/^https?:\/\//i.test(url)){setError("Liên kết không hợp lệ.");return;} void Linking.openURL(url).catch(()=>setError("Không mở được bài nộp.")); }} />}<Text style={ui.code}>Điểm: {display(r.score)} / {assignment.maxScore}</Text>{!!r.feedback && <Text style={ui.text}>{r.feedback}</Text>}<Button label={r.score === null ? "Chấm điểm" : "Sửa điểm"} icon="create-outline" onPress={()=>setEditing(r)} /></View>)}
    {!state.loading && !rows.length && <Text style={ui.muted}>Chưa có bài nộp phù hợp.</Text>}
    {editing && <RecordEditor title={`Chấm bài: ${editing.fullName}`} initial={editing} fields={[{key:"score",label:"Điểm",type:"number",required:true,min:0,max:Number(assignment.maxScore)},{key:"feedback",label:"Nhận xét",type:"multiline"}]} onClose={()=>setEditing(null)} save={body=>teacherRequest(`${path}/${editing.submissionId}/grade`,"PUT",body)} onSaved={()=>{setEditing(null);void state.refresh();}} />}
  </Dialog>;
}
