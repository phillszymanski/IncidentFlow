import type { IncidentLog } from "../types";

type IncidentTimelineSectionProps = {
  logsLoading: boolean;
  logsError: string | null;
  logs: IncidentLog[];
};

export const IncidentTimelineSection = ({
  logsLoading,
  logsError,
  logs
}: IncidentTimelineSectionProps) => {
  return (
    <section className="h-fit rounded-2xl border border-slate-800/80 bg-slate-900/70 p-5 shadow-xl shadow-slate-950/30">
      <h3 className="text-base font-semibold text-white">Timeline</h3>

      <div className="mt-4 space-y-3">
        {logsLoading ? (
          <p className="text-sm text-slate-400">Loading timeline...</p>
        ) : logsError ? (
          <p className="text-sm text-rose-300">{logsError}</p>
        ) : logs.length === 0 ? (
          <p className="text-sm text-slate-400">No activity logged yet.</p>
        ) : (
          logs.map((log) => (
            <article
              key={log.id}
              className="rounded-xl border border-slate-800 bg-slate-900/90 px-4 py-3"
            >
              <div className="flex items-center justify-between gap-3">
                <p className="text-sm font-semibold text-slate-100">{log.action}</p>
                <p className="text-xs text-slate-400">{new Date(log.createdAt).toLocaleString()}</p>
              </div>
              <p className="mt-2 whitespace-pre-wrap text-sm text-slate-200">{log.details}</p>
              <p className="mt-2 text-xs text-slate-400">By {log.performedByUserId}</p>
            </article>
          ))
        )}
      </div>
    </section>
  );
};
