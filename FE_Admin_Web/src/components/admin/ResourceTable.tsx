import { KeyRound, Shield, Users, GraduationCap, Loader2, Pencil, Search, Trash2 } from "lucide-react";
import type {
  AdminRecord,
  LookupState,
  PagedResult,
  ResourceConfig,
} from "../../types/admin";
import { formatCell } from "../../utils/adminDisplay";

type ResourceTableProps = {
  filters: Record<string, string>;
  isLoading: boolean;
  isSaving: boolean;
  lookups: LookupState;
  onDelete: (row: AdminRecord) => void;
  onEdit: (row: AdminRecord) => void;
  onAction: (row: AdminRecord, mode: "role" | "password" | "teachers" | "students") => void;
  onFilterChange: (key: string, value: string) => void;
  onPageChange: (page: number) => void;
  onSearchChange: (value: string) => void;
  pageInfo: PagedResult;
  resource: ResourceConfig;
  rows: AdminRecord[];
  search: string;
};

export function ResourceTable({
  filters,
  isLoading,
  isSaving,
  lookups,
  onDelete,
  onEdit,
  onAction,
  onFilterChange,
  onPageChange,
  onSearchChange,
  pageInfo,
  resource,
  rows,
  search,
}: ResourceTableProps) {
  return (
    <section className="panel">
      <div className="panel-toolbar">
        <label className="search-box">
          <Search size={18} aria-hidden="true" />
          <input
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder={`Tìm trong ${resource.title.toLowerCase()}`}
            type="search"
            value={search}
          />
        </label>

        {resource.filters?.map((filter) => (
          <select
            aria-label={filter.label}
            key={filter.key}
            onChange={(event) => onFilterChange(filter.key, event.target.value)}
            value={filters[filter.key] ?? ""}
          >
            <option value="">{filter.label}</option>
            {filter.getOptions(lookups).map((option) => (
              <option key={String(option.value)} value={String(option.value)}>
                {option.label}
              </option>
            ))}
          </select>
        ))}
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              {resource.columns.map((column) => (
                <th key={column.key}>{column.label}</th>
              ))}
              <th className="actions-col">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={resource.columns.length + 1}>
                  <div className="table-state">
                    <Loader2 className="spin" size={20} aria-hidden="true" />
                    <span>Đang tải dữ liệu...</span>
                  </div>
                </td>
              </tr>
            ) : null}

            {!isLoading && rows.length === 0 ? (
              <tr>
                <td colSpan={resource.columns.length + 1}>
                  <div className="table-state">Chưa có dữ liệu phù hợp.</div>
                </td>
              </tr>
            ) : null}

            {!isLoading
              ? rows.map((row) => (
                  <tr key={String(row[resource.idKey])}>
                    {resource.columns.map((column) => (
                      <td key={column.key}>
                        {column.render
                          ? column.render(row, lookups)
                          : formatCell(row[column.key])}
                      </td>
                    ))}
                    <td>
                      <div className="row-actions">
                        {resource.key === "users" && <>
                          <button type="button" className="icon-button small" title="Đổi vai trò" disabled={isSaving} onClick={() => onAction(row, "role")}><Shield size={16} /></button>
                          <button type="button" className="icon-button small" title="Đổi mật khẩu" disabled={isSaving} onClick={() => onAction(row, "password")}><KeyRound size={16} /></button>
                        </>}
                        {resource.key === "courseSections" && <>
                          <button type="button" className="icon-button small" title="Phân công giảng viên" disabled={isSaving} onClick={() => onAction(row, "teachers")}><GraduationCap size={16} /></button>
                          <button type="button" className="icon-button small" title="Quản lý sinh viên" disabled={isSaving} onClick={() => onAction(row, "students")}><Users size={16} /></button>
                        </>}
                        <button
                          className="icon-button small"
                          onClick={() => onEdit(row)}
                          title="Sửa"
                          type="button"
                        >
                          <Pencil size={16} aria-hidden="true" />
                        </button>
                        {resource.canDelete === false ? null : (
                          <button
                            className="icon-button small danger"
                            disabled={isSaving}
                            onClick={() => onDelete(row)}
                            title="Xóa"
                            type="button"
                          >
                            <Trash2 size={16} aria-hidden="true" />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              : null}
          </tbody>
        </table>
      </div>

      <footer className="pagination">
        <span>
          {pageInfo.totalCount} bản ghi · Trang {pageInfo.page}/
          {Math.max(pageInfo.totalPages, 1)}
        </span>
        <div>
          <button
            className="secondary-button"
            disabled={pageInfo.page <= 1 || isLoading}
            onClick={() => onPageChange(Math.max(1, pageInfo.page - 1))}
            type="button"
          >
            Trước
          </button>
          <button
            className="secondary-button"
            disabled={pageInfo.page >= pageInfo.totalPages || isLoading}
            onClick={() => onPageChange(pageInfo.page + 1)}
            type="button"
          >
            Sau
          </button>
        </div>
      </footer>
    </section>
  );
}
