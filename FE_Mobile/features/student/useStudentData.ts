import { ApiError } from "@/api/client";
import { authSession } from "@/features/auth/authSession";
import { router, useFocusEffect } from "expo-router";
import { useCallback, useRef, useState } from "react";

export function useStudentData<T>(load: () => Promise<T>) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const sequence = useRef(0);

  const refresh = useCallback(async () => {
    const id = ++sequence.current;
    setLoading(true);
    setError("");
    try {
      const value = await load();
      if (id === sequence.current) setData(value);
    } catch (err) {
      if (id !== sequence.current) return;
      setData(null);
      if (err instanceof ApiError && err.status === 401) {
        authSession.logout();
        router.replace("/login");
      } else if (err instanceof ApiError && err.status === 403) {
        setError("Bạn không có quyền truy cập khu vực sinh viên.");
      } else {
        setError(err instanceof Error ? err.message : "Không tải được dữ liệu.");
      }
    } finally {
      if (id === sequence.current) setLoading(false);
    }
  }, [load]);

  useFocusEffect(useCallback(() => {
    void refresh();
    return () => { sequence.current++; };
  }, [refresh]));

  return { data, loading, error, refresh };
}
