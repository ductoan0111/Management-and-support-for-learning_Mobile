import type {
  AdminQuery,
  AdminPrimitive,
  AdminRecord,
  AdminRole,
  AdminSession,
  AdminStatistics,
  PagedResult,
} from "../types/admin";

export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ??
  "http://localhost:5113";

export const ADMIN_SESSION_KEY = "study-support-admin-session";

type ApiPagedResult<T> =
  | PagedResult<T>
  | {
      Items?: T[];
      Page?: number;
      PageSize?: number;
      TotalCount?: number;
      TotalPages?: number;
    };

export class AdminApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "AdminApiError";
    this.status = status;
  }
}

function normalizePagedResult<T>(payload: ApiPagedResult<T>): PagedResult<T> {
  return {
    items: "items" in payload ? payload.items : payload.Items ?? [],
    page: "page" in payload ? payload.page : payload.Page ?? 1,
    pageSize: "pageSize" in payload ? payload.pageSize : payload.PageSize ?? 20,
    totalCount:
      "totalCount" in payload ? payload.totalCount : payload.TotalCount ?? 0,
    totalPages:
      "totalPages" in payload ? payload.totalPages : payload.TotalPages ?? 1,
  };
}

function buildUrl(path: string, query?: AdminQuery) {
  const url = new URL(`${API_BASE_URL}${path}`);

  Object.entries(query ?? {}).forEach(([key, value]) => {
    if (value !== "" && value !== null && value !== undefined) {
      url.searchParams.set(key, String(value));
    }
  });

  return url.toString();
}

async function readErrorMessage(response: Response) {
  const fallback =
    response.status === 401
      ? "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
      : `Yêu cầu thất bại (${response.status}).`;

  try {
    const payload = await response.json();
    if (typeof payload?.message === "string") {
      return payload.message;
    }
    if (typeof payload?.title === "string") {
      return payload.title;
    }
    if (payload?.errors && typeof payload.errors === "object") {
      return Object.entries(payload.errors)
        .flatMap(([, messages]) =>
          Array.isArray(messages) ? messages.map(String) : [String(messages)],
        )
        .join(" ");
    }
  } catch {
    // Some endpoints return an empty body for errors such as 404.
  }

  return fallback;
}

async function request<T>(
  path: string,
  token: string | null,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);

  if (options.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new AdminApiError(await readErrorMessage(response), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

export async function loginAdmin(
  username: string,
  password: string,
): Promise<AdminSession> {
  return request<AdminSession>("/api/admin/auth/login", null, {
    body: JSON.stringify({ username, password }),
    method: "POST",
  });
}

export async function listAdminResource<T = AdminRecord>(
  path: string,
  token: string,
  query: AdminQuery,
): Promise<PagedResult<T>> {
  const payload = await request<ApiPagedResult<T>>(
    buildUrl(path, query).replace(API_BASE_URL, ""),
    token,
  );

  return normalizePagedResult(payload);
}

export async function createAdminResource<T = AdminRecord>(
  path: string,
  token: string,
  data: AdminRecord,
): Promise<T> {
  return request<T>(path, token, {
    body: JSON.stringify(data),
    method: "POST",
  });
}

export async function updateAdminResource<T = AdminRecord>(
  path: string,
  token: string,
  id: AdminPrimitive,
  data: AdminRecord,
): Promise<T> {
  return request<T>(`${path}/${id}`, token, {
    body: JSON.stringify(data),
    method: "PUT",
  });
}

export async function deleteAdminResource(
  path: string,
  token: string,
  id: AdminPrimitive,
): Promise<void> {
  await request<void>(`${path}/${id}`, token, { method: "DELETE" });
}

export async function getAdminRoles(token: string): Promise<AdminRole[]> {
  return request<AdminRole[]>("/api/admin/roles", token);
}

export function setAdminUserRole(token: string, id: number, roleId: number) {
  return request<AdminRecord>(`/api/admin/users/${id}/role`, token, {
    method: "PUT", body: JSON.stringify({ roleId }),
  });
}

export function resetAdminUserPassword(token: string, id: number, password: string) {
  return request<void>(`/api/admin/users/${id}/password`, token, {
    method: "PUT", body: JSON.stringify({ password }),
  });
}

export async function getAdminStatistics(
  token: string,
): Promise<AdminStatistics> {
  return request<AdminStatistics>("/api/admin/statistics", token);
}
