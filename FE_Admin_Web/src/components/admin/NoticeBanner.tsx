import { AlertCircle, CheckCircle2 } from "lucide-react";
import type { Notice } from "../../types/admin";

type NoticeBannerProps = {
  notice: Notice | null;
};

export function NoticeBanner({ notice }: NoticeBannerProps) {
  if (!notice) return null;

  return (
    <div className={`notice ${notice.tone}`}>
      {notice.tone === "error" ? <AlertCircle size={18} /> : <CheckCircle2 size={18} />}
      <span>{notice.message}</span>
    </div>
  );
}
