import Constants from "expo-constants";
import { Platform } from "react-native";
import { authSession } from "@/features/auth/authSession";

export function getBaseUrl() {
  if (Platform.OS === "web" && process.env.EXPO_PUBLIC_WEB_API_URL) return process.env.EXPO_PUBLIC_WEB_API_URL.replace(/\/$/, "");
  if (process.env.EXPO_PUBLIC_API_URL) return process.env.EXPO_PUBLIC_API_URL.replace(/\/$/, "");
  if (Platform.OS === "web") return `http://${globalThis.location?.hostname || "localhost"}:5113`;
  const host = Constants.expoConfig?.hostUri?.split(":")[0];
  if (host && host !== "localhost" && host !== "127.0.0.1") return `http://${host}:5113`;
  return `http://${Platform.OS === "android" ? "10.0.2.2" : "localhost"}:5113`;
}

export class ApiError extends Error {
  constructor(message: string, public status: number) { super(message); }
}

export async function request<T>(path: string, method = "GET", body?: unknown): Promise<T> {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 15000);
  try {
    const token = authSession.getToken();
    const response = await fetch(`${getBaseUrl()}${path}`, {
      method, signal: controller.signal,
      headers: { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}) },
      ...(body === undefined ? {} : { body: JSON.stringify(body) }),
    });
    const text = await response.text();
    let data;
    try { data = text ? JSON.parse(text) : undefined; } catch { data = undefined; }
    if (!response.ok) {
      const validation = data?.errors ? Object.values(data.errors).flat().join(" ") : null;
      throw new ApiError(data?.message || validation || data?.title || (typeof data === "string" ? data : `Yêu cầu thất bại (${response.status}).`), response.status);
    }
    return data as T;
  } catch (error) {
    if (error instanceof ApiError) throw error;
    throw new Error("Không kết nối được máy chủ. Vui lòng kiểm tra mạng và thử lại.");
  } finally { clearTimeout(timeout); }
}
