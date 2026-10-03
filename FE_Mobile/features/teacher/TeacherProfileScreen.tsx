import { teacherRequest, type Row } from "@/api/teacher";
import { colors } from "@/constants/theme";
import { authSession } from "@/features/auth/authSession";
import { Ionicons } from "@expo/vector-icons";
import { useState } from "react";
import { Image, Text, View } from "react-native";
import { RecordEditor } from "./components/RecordEditor";
import { Button, Page, display, ui, type IconName } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";

const load = () => teacherRequest<Row>("/profile");
const info: [string, string, IconName][] = [
  ["teacherCode", "Mã giảng viên", "id-card-outline"],
  ["email", "Email", "mail-outline"],
  ["phone", "Điện thoại", "call-outline"],
  ["departmentName", "Khoa", "business-outline"],
  ["academicTitle", "Học hàm / học vị", "ribbon-outline"],
  ["specialization", "Chuyên môn", "book-outline"],
  ["dateOfBirth", "Ngày sinh", "gift-outline"],
];

export default function TeacherProfileScreen() {
  const state = useTeacherData(load);
  const [editing, setEditing] = useState(false);
  return <Page home title="Hồ sơ giảng viên" {...state}>{state.data && <>
    <View style={[ui.hero, { alignItems: "center" }]}>
      <Image source={state.data.avatarUrl ? { uri: String(state.data.avatarUrl) } : require("@/assets/images/icon.png")} style={{ width: 88, height: 88, borderRadius: 44, borderWidth: 3, borderColor: "rgba(255,255,255,0.6)" }} />
      <Text style={ui.heroText}>{display(state.data.fullName)}</Text>
      <Text style={ui.heroMuted}>{display(state.data.departmentName)}</Text>
    </View>
    <View style={ui.card}>
      {info.map(([key, label, icon]) => <View key={key} style={[ui.row, { flexWrap: "nowrap", paddingVertical: 8 }]}>
        <View style={ui.tileIcon}><Ionicons name={icon} size={20} color={colors.primary} /></View>
        <View style={ui.grow}>
          <Text style={ui.muted}>{label}</Text>
          <Text style={ui.text}>{key === "dateOfBirth" && state.data?.[key] ? String(state.data[key]).slice(0, 10) : display(state.data?.[key])}</Text>
        </View>
      </View>)}
    </View>
    <Button label="Cập nhật hồ sơ" icon="create-outline" onPress={() => setEditing(true)} />
    {editing && <RecordEditor title="Cập nhật hồ sơ" initial={state.data} fields={[
      {key:"fullName",label:"Họ tên",required:true},{key:"phone",label:"Điện thoại"},{key:"dateOfBirth",label:"Ngày sinh (YYYY-MM-DD)",type:"date"},
      {key:"gender",label:"Giới tính",type:"select",options:[{value:"",label:"Chưa cung cấp"},{value:"1",label:"Nam"},{value:"2",label:"Nữ"},{value:"0",label:"Khác"}]},
      {key:"avatarUrl",label:"Liên kết ảnh đại diện",type:"url"},{key:"academicTitle",label:"Học hàm / học vị"},{key:"specialization",label:"Chuyên môn",type:"multiline"},
    ]} onClose={() => setEditing(false)} save={async body => { const updated = await teacherRequest<Row>("/profile","PUT",body); const user = authSession.getUser(); if(user) authSession.setUser({...user,fullName:String(updated.fullName),phone:updated.phone as string|null,avatarUrl:updated.avatarUrl as string|null}); }} onSaved={() => {setEditing(false); void state.refresh();}} />}
  </>}</Page>;
}
