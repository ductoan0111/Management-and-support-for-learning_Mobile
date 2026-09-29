import { LayoutDashboard, LogOut, Plus, RefreshCw } from "lucide-react";
import type { ReactNode } from "react";
import { resourceConfigs } from "../../config/adminResources";
import type { AdminSession, ResourceConfig } from "../../types/admin";

type AdminLayoutProps = {
  activeKey: string;
  activeResource: ResourceConfig | null;
  children: ReactNode;
  isBusy: boolean;
  onCreate: () => void;
  onLogout: () => void;
  onRefresh: () => void;
  onSelect: (key: string) => void;
  session: AdminSession;
};

export function AdminLayout({
  activeKey,
  activeResource,
  children,
  isBusy,
  onCreate,
  onLogout,
  onRefresh,
  onSelect,
  session,
}: AdminLayoutProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">SS</div>
          <div>
            <strong>Study Support</strong>
            <span>Admin Web</span>
          </div>
        </div>

        <nav className="nav-list" aria-label="Admin navigation">
          <button
            className={`nav-item ${activeKey === "dashboard" ? "active" : ""}`}
            onClick={() => onSelect("dashboard")}
            type="button"
          >
            <LayoutDashboard size={18} aria-hidden="true" />
            <span>Tổng quan</span>
          </button>
          {resourceConfigs.map(({ icon: Icon, key, navLabel }) => (
            <button
              className={`nav-item ${activeKey === key ? "active" : ""}`}
              key={key}
              onClick={() => onSelect(key)}
              type="button"
            >
              <Icon size={18} aria-hidden="true" />
              <span>{navLabel}</span>
            </button>
          ))}
        </nav>
      </aside>

      <main className="main">
        <header className="topbar">
          <div>
            <p className="eyebrow">Quản trị hệ thống</p>
            <h1>{activeResource?.title ?? "Tổng quan"}</h1>
            <p className="page-description">
              {activeResource?.description ??
                "Theo dõi nhanh dữ liệu vận hành của hệ thống."}
            </p>
          </div>
          <div className="topbar-actions">
            <button
              className="icon-button"
              disabled={isBusy}
              onClick={onRefresh}
              title="Tải lại"
              type="button"
            >
              <RefreshCw size={18} aria-hidden="true" />
            </button>
            {activeResource ? (
              <button className="primary-button" onClick={onCreate} type="button">
                <Plus size={18} aria-hidden="true" />
                <span>Thêm</span>
              </button>
            ) : null}
            <button className="secondary-button" onClick={onLogout} type="button">
              <LogOut size={18} aria-hidden="true" />
              <span>Đăng xuất</span>
            </button>
          </div>
        </header>

        <div className="session-strip">
          <span>{session.user.fullName}</span>
          <strong>{session.user.roleCode}</strong>
        </div>

        {children}
      </main>
    </div>
  );
}
