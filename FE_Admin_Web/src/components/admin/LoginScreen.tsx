import { AlertCircle, CheckCircle2, Loader2 } from "lucide-react";
import { FormEvent, useState } from "react";
import { ADMIN_SESSION_KEY, API_BASE_URL, loginAdmin } from "../../api/admin";
import type { AdminSession } from "../../types/admin";

type LoginScreenProps = {
  onLogin: (session: AdminSession) => void;
};

export function LoginScreen({ onLogin }: LoginScreenProps) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const session = await loginAdmin(username.trim(), password);
      localStorage.setItem(ADMIN_SESSION_KEY, JSON.stringify(session));
      onLogin(session);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Không đăng nhập được.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="login-page">
      <section className="login-card">
        <div className="brand big">
          <div className="brand-mark">SS</div>
          <div>
            <strong>Study Support</strong>
            <span>Admin Web</span>
          </div>
        </div>

        <div>
          <p className="eyebrow">Quản trị hệ thống</p>
          <h1>Đăng nhập admin</h1>
        </div>

        {error ? (
          <div className="notice error">
            <AlertCircle size={18} aria-hidden="true" />
            <span>{error}</span>
          </div>
        ) : null}

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            Tên đăng nhập
            <input
              autoComplete="username"
              onChange={(event) => setUsername(event.target.value)}
              required
              value={username}
            />
          </label>
          <label>
            Mật khẩu
            <input
              autoComplete="current-password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
          </label>
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? <Loader2 className="spin" size={18} /> : <CheckCircle2 size={18} />}
            <span>Đăng nhập</span>
          </button>
        </form>

        <p className="api-note">API: {API_BASE_URL}</p>
      </section>
    </main>
  );
}
