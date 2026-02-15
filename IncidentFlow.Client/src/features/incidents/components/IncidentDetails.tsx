import { IncidentSeverity, IncidentStatus, type Incident } from "../types";

type IncidentDetailsProps = {
  incident: Incident | null;
  onEdit?: () => void;
};

const statusStyles: Record<string, string> = {
  Open: "bg-sky-500/15 text-sky-300 ring-1 ring-sky-400/30",
  InProgress: "bg-amber-500/15 text-amber-300 ring-1 ring-amber-400/30",
  Resolved: "bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-400/30",
  Closed: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"
};

const severityStyles: Record<string, string> = {
  Low: "bg-slate-500 text-sky-800 text-xs font-medium px-2.5 py-0.5 rounded-full",
  Medium: "bg-yellow-100 text-yellow-800 text-xs font-medium px-2.5 py-0.5 rounded-full",
  High: "bg-amber-500 text-white text-xs font-medium px-2.5 py-0.5 rounded-full",
  Critical: "bg-red-500 text-white text-xs font-medium px-2.5 py-0.5 rounded-full"
};

const getDisplayStatus = (rawStatus: string) => {
  if (rawStatus in IncidentStatus) {
    return IncidentStatus[rawStatus as keyof typeof IncidentStatus];
  }

  return rawStatus;
};

const getDisplaySeverity = (rawSeverity: string) => {
  if (rawSeverity in IncidentSeverity) {
    return IncidentSeverity[rawSeverity as keyof typeof IncidentSeverity];
  }

  return rawSeverity;
};

export const IncidentDetails = ({ incident, onEdit }: IncidentDetailsProps) => {
  if (!incident) {
    return (
      <aside className="rounded-2xl border border-dashed border-slate-700/90 bg-slate-900/40 p-6 lg:sticky lg:top-6">
        <p className="text-sm font-medium text-slate-200">Incident details</p>
        <p className="mt-2 text-sm text-slate-400">Select an incident from the list to view its details.</p>
      </aside>
    );
  }

  const displayStatus = getDisplayStatus(incident.status);
  const displaySeverity = getDisplaySeverity(incident.severity);
  const assignedUser = incident.assignedTo ?? "Unassigned";
  const lastUpdatedAt = incident.updatedAt ?? incident.resolvedAt ?? incident.createdAt;

  return (
    <div className="space-y-3 lg:sticky lg:top-6">
      <aside className="rounded-2xl border border-slate-800/80 bg-slate-900/70 p-6 shadow-xl shadow-slate-950/40">
        <div className="flex items-start justify-between gap-3">
          <h2 className="text-lg font-semibold text-slate-100">Incident Details</h2>
          <span
            className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[displayStatus] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}
          >
            {displayStatus}
          </span>
        </div>

        <div className="mt-6 space-y-5">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Title</p>
            <p className="mt-1 text-sm text-slate-100">{incident.title}</p>
          </div>

          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Description</p>
            <p className="mt-1 whitespace-pre-wrap text-sm leading-relaxed text-slate-200">{incident.description}</p>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Severity</p>
              <p className={`mt-1 text-sm text-slate-200 ${severityStyles[displaySeverity] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}>{displaySeverity}</p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Assigned User</p>
              <p className="mt-1 break-all text-sm text-slate-200">{assignedUser}</p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Incident ID</p>
              <p className="mt-1 break-all text-xs text-slate-300">{incident.id}</p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Created At</p>
              <p className="mt-1 text-sm text-slate-200">{new Date(incident.createdAt).toLocaleString()}</p>
            </div>

            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Last Updated</p>
              <p className="mt-1 text-sm text-slate-200">{new Date(lastUpdatedAt).toLocaleString()}</p>
            </div>
          </div>
        </div>
      </aside>

      <div className="grid w-full grid-cols-2 gap-3">
        <button
          type="button"
          onClick={onEdit}
          className="inline-flex w-full items-center justify-center rounded-xl border border-slate-700 bg-slate-900 px-4 py-2.5 text-sm font-semibold text-slate-100 transition hover:border-slate-500"
        >
          Edit
        </button>
      </div>
    </div>
  );
};
