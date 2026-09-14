import { Redirect, type Href } from "expo-router";

const loginHref = "/login" as Href;

export default function Index() {
  return <Redirect href={loginHref} />;
}
