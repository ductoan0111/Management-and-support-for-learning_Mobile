import { teacherRequest, type Row } from "@/api/teacher";
import { authSession } from "@/features/auth/authSession";
import { useState } from "react";
import { Text, View } from "react-native";
import { RecordEditor } from "./components/RecordEditor";
import { Button, Page, display, ui } from "./components/TeacherUI";
import { useTeacherData } from "./useTeacherData";
const load = () => teacherRequest<Row>("/profile");
export default function TeacherProfileScreen() {
  const state = useTeacherData(load);
  const [editing, setEditing] = useState(false);
  return <Page title="Hồ sơ giảng viên" {...state}>{state.data && <>
    <Text style={ui.heading}>{state.data.fullName}</Text>
    {[["teacherCode","Mã giảng viên"],["email","Email"],["phone","Điện thoại"],["departmentName","Khoa"],["academicTitle","Học hàm / học vị"],["specialization","Chuyên môn"],["dateOfBirth","Ngày sinh"]].map(([key,label]) => <View key={key} style={{gap:4}}><Text style={ui.muted}>{label}</Text><Text style={ui.text}>{display(state.data?.[key])}</Text></View>)}
    <Button label="Cập nhật hồ sơ" icon="create-outline" onPress={() => setEditing(true)} />
    {editing && <RecordEditor title="Cập nhật hồ sơ" initial={state.data} fields={[
      {key:"fullName",label:"Họ tên",required:true},{key:"phone",label:"Điện thoại"},{key:"dateOfBirth",label:"Ngày sinh (YYYY-MM-DD)",type:"date"},
      {key:"gender",label:"Giới tính",type:"select",options:[{value:"",label:"Chưa cung cấp"},{value:"1",label:"Nam"},{value:"2",label:"Nữ"},{value:"0",label:"Khác"}]},
      {key:"avatarUrl",label:"Liên kết ảnh đại diện",type:"url"},{key:"academicTitle",label:"Học hàm / học vị"},{key:"specialization",label:"Chuyên môn",type:"multiline"},
    ]} onClose={() => setEditing(false)} save={async body => { const updated = await teacherRequest<Row>("/profile","PUT",body); const user = authSession.getUser(); if(user) authSession.setUser({...user,fullName:String(updated.fullName),phone:updated.phone as string|null,avatarUrl:updated.avatarUrl as string|null}); }} onSaved={() => {setEditing(false); void state.refresh();}} />}
  </>}</Page>;
}
