import { useEffect, useRef, type ReactNode } from "react";
import { X } from "lucide-react";

export function ActionModal({ title, busy, onClose, children }: {
  title: string; busy: boolean; onClose: () => void; children: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    const dialog = ref.current!;
    dialog.showModal();
    return () => dialog.close();
  }, []);
  return <dialog ref={ref} className="dialog-card action-modal" aria-labelledby="action-title"
    onCancel={(event) => { event.preventDefault(); if (!busy) onClose(); }}>
    <div className="dialog-header">
      <h2 id="action-title">{title}</h2>
      <button type="button" className="icon-button" title="Đóng" disabled={busy} onClick={onClose}><X size={18} /></button>
    </div>
    {children}
  </dialog>;
}
