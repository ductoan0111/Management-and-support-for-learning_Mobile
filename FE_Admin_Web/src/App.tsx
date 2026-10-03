import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  ADMIN_SESSION_KEY,
  AdminApiError,
  createAdminResource,
  deleteAdminResource,
  getAdminRoles,
  getAdminReport,
  getAdminStatistics,
  listAdminResource,
  updateAdminResource,
} from "./api/admin";
import { AdminLayout } from "./components/admin/AdminLayout";
import { Dashboard } from "./components/admin/Dashboard";
import { LoginScreen } from "./components/admin/LoginScreen";
import { NoticeBanner } from "./components/admin/NoticeBanner";
import { ResourceDialog } from "./components/admin/ResourceDialog";
import { ResourceTable } from "./components/admin/ResourceTable";
import { UserAccessDialog } from "./components/admin/UserAccessDialog";
import { SectionMembersDialog } from "./components/admin/SectionMembersDialog";
import { emptyLookups } from "./config/adminOptions";
import { resourceConfigs } from "./config/adminResources";
import type {
  AdminFormState,
  AdminRecord,
  AdminRole,
  AdminSession,
  AdminReport,
  AdminStatistics,
  DialogState,
  LookupState,
  Notice,
  PagedResult,
} from "./types/admin";
import {
  getVisibleFields,
  hasMissingRequiredField,
  makePayload,
  rowToForm,
} from "./utils/adminForms";
import { readStoredSession } from "./utils/adminSession";

const PAGE_SIZE = 20;

const initialPageInfo: PagedResult<AdminRecord> = {
  items: [],
  page: 1,
  pageSize: PAGE_SIZE,
  totalCount: 0,
  totalPages: 1,
};

export default function App() {
  const [session, setSession] = useState<AdminSession | null>(() => readStoredSession());
  const [activeKey, setActiveKey] = useState("dashboard");
  const [lookups, setLookups] = useState<LookupState>(emptyLookups);
  const [statistics, setStatistics] = useState<AdminStatistics | null>(null);
  const [report, setReport] = useState<AdminReport | null>(null);
  const [rows, setRows] = useState<AdminRecord[]>([]);
  const [pageInfo, setPageInfo] = useState<PagedResult<AdminRecord>>(initialPageInfo);
  const [search, setSearch] = useState("");
  const [filters, setFilters] = useState<Record<string, string>>({});
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [isDashboardLoading, setIsDashboardLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [notice, setNotice] = useState<Notice | null>(null);
  const [dialog, setDialog] = useState<DialogState | null>(null);
  const [form, setForm] = useState<AdminFormState>({});
  const [refreshKey, setRefreshKey] = useState(0);
  const [action, setAction] = useState<{ row: AdminRecord; mode: "role" | "password" | "teachers" | "students" } | null>(null);

  const activeResource = useMemo(
    () => resourceConfigs.find((resource) => resource.key === activeKey) ?? null,
    [activeKey],
  );

  const logout = () => {
    setAction(null);
    localStorage.removeItem(ADMIN_SESSION_KEY);
    setSession(null);
    setActiveKey("dashboard");
    setRows([]);
    setStatistics(null);
    setReport(null);
    setNotice(null);
  };

  const handleError = (error: unknown) => {
    if (error instanceof AdminApiError && error.status === 401) {
      logout();
      return;
    }

    setNotice({
      tone: "error",
      message: error instanceof Error ? error.message : "Có lỗi khi kết nối API.",
    });
  };

  const loadLookups = async (token: string) => {
    const settled = await Promise.allSettled([
      getAdminRoles(token),
      listAdminResource("/api/admin/users", token, { page: 1, pageSize: 100 }),
      listAdminResource("/api/admin/departments", token, { page: 1, pageSize: 100 }),
      listAdminResource("/api/admin/majors", token, { page: 1, pageSize: 100 }),
      listAdminResource("/api/admin/academic-classes", token, { page: 1, pageSize: 100 }),
      listAdminResource("/api/admin/courses", token, { page: 1, pageSize: 100 }),
      listAdminResource("/api/admin/semesters", token, { page: 1, pageSize: 100 }),
    ]);

    const roles = settled[0].status === "fulfilled" ? settled[0].value : [];
    const getItems = (index: number) =>
      settled[index].status === "fulfilled"
        ? (settled[index].value as PagedResult<AdminRecord>).items
        : [];

    setLookups({
      roles: (roles as AdminRole[]).map((role) => ({ ...role })),
      users: getItems(1),
      departments: getItems(2),
      majors: getItems(3),
      academicClasses: getItems(4),
      courses: getItems(5),
      semesters: getItems(6),
    });

    const rejected = settled.find((result) => result.status === "rejected");
    if (rejected) {
      throw rejected.reason;
    }
  };

  const loadStatistics = async (token: string) => {
    setIsDashboardLoading(true);
    try {
      const [stats, reportData] = await Promise.all([
        getAdminStatistics(token),
        getAdminReport(token).catch(() => null),
      ]);
      setStatistics(stats);
      setReport(reportData);
    } catch (error) {
      handleError(error);
    } finally {
      setIsDashboardLoading(false);
    }
  };

  useEffect(() => {
    if (!session) return;

    void loadLookups(session.accessToken).catch(handleError);
    void loadStatistics(session.accessToken);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session?.accessToken]);

  useEffect(() => {
    setSearch("");
    setFilters({});
    setPage(1);
    setDialog(null);
    setNotice(null);
    setAction(null);
  }, [activeKey]);

  useEffect(() => {
    if (!session || !activeResource) return;

    let ignored = false;
    setIsLoading(true);
    listAdminResource(activeResource.path, session.accessToken, {
      search: search.trim() || undefined,
      page,
      pageSize: PAGE_SIZE,
      ...filters,
    })
      .then((result) => {
        if (ignored) return;
        setRows(result.items);
        setPageInfo(result);
      })
      .catch((error) => {
        if (!ignored) handleError(error);
      })
      .finally(() => {
        if (!ignored) setIsLoading(false);
      });

    return () => {
      ignored = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeResource, filters, page, refreshKey, search, session?.accessToken]);

  const refresh = () => {
    if (!session) return;

    setNotice(null);
    void loadLookups(session.accessToken).catch(handleError);
    void loadStatistics(session.accessToken);
    setRefreshKey((value) => value + 1);
  };

  const openCreateDialog = () => {
    if (!activeResource) return;
    setForm({ ...activeResource.defaultValues });
    setDialog({ mode: "create", row: null });
  };

  const openEditDialog = (row: AdminRecord) => {
    if (!activeResource) return;
    setForm(rowToForm(activeResource, row));
    setDialog({ mode: "edit", row });
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!session || !activeResource || !dialog) return;

    const visibleFields = getVisibleFields(activeResource, dialog.mode);
    const missingField = visibleFields.find((field) =>
      hasMissingRequiredField(field, form[field.key]),
    );

    if (missingField) {
      setNotice({ tone: "error", message: `Vui lòng nhập ${missingField.label}.` });
      return;
    }

    const payload = makePayload(activeResource, form, dialog.mode);
    setIsSaving(true);
    setNotice(null);

    try {
      if (dialog.mode === "create") {
        await createAdminResource(activeResource.path, session.accessToken, payload);
        setNotice({
          tone: "success",
          message: `Đã thêm ${activeResource.title.toLowerCase()}.`,
        });
      } else if (dialog.row) {
        await updateAdminResource(
          activeResource.path,
          session.accessToken,
          dialog.row[activeResource.idKey],
          payload,
        );
        setNotice({
          tone: "success",
          message: `Đã cập nhật ${activeResource.title.toLowerCase()}.`,
        });
      }

      setDialog(null);
      setRefreshKey((value) => value + 1);
      void loadLookups(session.accessToken).catch(handleError);
      void loadStatistics(session.accessToken);
    } catch (error) {
      handleError(error);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (row: AdminRecord) => {
    if (!session || !activeResource || activeResource.canDelete === false) return;

    const confirmed = window.confirm(`Xóa ${activeResource.title.toLowerCase()} này?`);
    if (!confirmed) return;

    setIsSaving(true);
    setNotice(null);

    try {
      await deleteAdminResource(
        activeResource.path,
        session.accessToken,
        row[activeResource.idKey],
      );
      setNotice({
        tone: "success",
        message: `Đã xóa ${activeResource.title.toLowerCase()}.`,
      });
      setRefreshKey((value) => value + 1);
      void loadLookups(session.accessToken).catch(handleError);
      void loadStatistics(session.accessToken);
    } catch (error) {
      handleError(error);
    } finally {
      setIsSaving(false);
    }
  };

  if (!session) {
    return <LoginScreen onLogin={setSession} />;
  }

  return (
    <>
      <AdminLayout
        activeKey={activeKey}
        activeResource={activeResource}
        isBusy={isLoading || isDashboardLoading}
        onCreate={openCreateDialog}
        onLogout={logout}
        onRefresh={refresh}
        onSelect={setActiveKey}
        session={session}
      >
        <NoticeBanner notice={notice} />

        {!activeResource ? (
          <Dashboard
            isLoading={isDashboardLoading}
            statistics={statistics}
            report={report}
          />
        ) : (
          <ResourceTable
            filters={filters}
            isLoading={isLoading}
            isSaving={isSaving}
            lookups={lookups}
            onDelete={(row) => void handleDelete(row)}
            onEdit={openEditDialog}
            onAction={(row, mode) => setAction({ row, mode })}
            onFilterChange={(key, value) => {
              setFilters((current) => ({ ...current, [key]: value }));
              setPage(1);
            }}
            onPageChange={setPage}
            onSearchChange={(value) => {
              setSearch(value);
              setPage(1);
            }}
            pageInfo={pageInfo}
            resource={activeResource}
            rows={rows}
            search={search}
          />
        )}
      </AdminLayout>

      <ResourceDialog
        dialog={dialog}
        form={form}
        isSaving={isSaving}
        lookups={lookups}
        onChange={(key, value) =>
          setForm((current) => ({
            ...current,
            [key]: value,
          }))
        }
        onClose={() => setDialog(null)}
        onSubmit={handleSubmit}
        resource={activeResource}
      />
      {action && (action.mode === "role" || action.mode === "password") && <UserAccessDialog
        row={action.row} mode={action.mode} roles={lookups.roles} token={session.accessToken}
        onClose={() => setAction(null)} onUnauthorized={logout}
        onSaved={() => {
          if (Number(action.row.userId) === session.user.userId) { logout(); return; }
          setAction(null); refresh(); setNotice({ tone: "success", message: "Đã cập nhật tài khoản." });
        }} />}
      {action && (action.mode === "teachers" || action.mode === "students") && <SectionMembersDialog
        row={action.row} kind={action.mode} token={session.accessToken}
        onClose={() => setAction(null)} onUnauthorized={logout} onChanged={refresh} />}
    </>
  );
}
