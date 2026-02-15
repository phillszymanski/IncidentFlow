
import { useMemo, useState } from "react";
import { useIncidents } from "../../../hooks/useIncidents";
import { IncidentDetails } from "../components/IncidentDetails";
import { type Incident, IncidentStatus } from "../types";

type IncidentListPageProps = {
  onReportIncident: () => void;
  onEditIncident: (incident: Incident) => void;
};

const statusStyles: Record<string, string> = {
  Open: "bg-sky-500/15 text-sky-300 ring-1 ring-sky-400/30",
  InProgress: "bg-amber-500/15 text-amber-300 ring-1 ring-amber-400/30",
  Resolved: "bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-400/30",
  Closed: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"
};

const normalizeStatus = (rawStatus: string) => {
  if (rawStatus in IncidentStatus) {
    return IncidentStatus[rawStatus as keyof typeof IncidentStatus];
  }

  return rawStatus;
};

export const IncidentListPage = ({ onReportIncident, onEditIncident }: IncidentListPageProps) => {
  const { incidents, loading, error, refetch } = useIncidents();
  const [selectedIncidentId, setSelectedIncidentId] = useState<string | null>(null);

  const selectedIncident = useMemo(
    () => incidents.find((incident) => incident.id === selectedIncidentId) ?? null,
    [incidents, selectedIncidentId]
  );

  if (loading) {
    return (
      <section className="mx-auto flex min-h-screen w-full max-w-5xl items-center px-6 py-12">
        <div className="w-full rounded-3xl border border-slate-800/80 bg-slate-900/60 p-10 shadow-2xl shadow-slate-950/60 backdrop-blur">
          <p className="text-lg font-medium text-slate-200">Loading incidents...</p>
          <div className="mt-6 h-2 w-full overflow-hidden rounded-full bg-slate-800">
            <div className="h-full w-1/2 animate-pulse rounded-full bg-sky-400" />
          </div>
        </div>
      </section>
    );
  }

  if (error) {
    return (
      <section className="mx-auto flex min-h-screen w-full max-w-5xl items-center px-6 py-12">
        <div className="w-full rounded-3xl border border-rose-400/25 bg-rose-500/10 p-8 text-rose-100 shadow-2xl shadow-rose-950/30 backdrop-blur">
          <h2 className="text-xl font-semibold">Unable to load incidents</h2>
          <p className="mt-2 text-sm text-rose-100/90">{error}</p>
          <button
            type="button"
            onClick={refetch}
            className="mt-6 inline-flex items-center rounded-xl bg-rose-400 px-4 py-2.5 text-sm font-semibold text-slate-900 transition hover:bg-rose-300"
          >
            Retry
          </button>
        </div>
      </section>
    );
  }

  return (
    <section className="mx-auto w-full max-w-5xl px-6 py-12">
      <div className="relative overflow-hidden rounded-3xl border border-slate-800/70 bg-slate-900/70 p-8 shadow-2xl shadow-slate-950/60 backdrop-blur">
        <div className="pointer-events-none absolute -right-20 -top-16 h-56 w-56 rounded-full bg-sky-500/20 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-20 -left-16 h-56 w-56 rounded-full bg-indigo-500/15 blur-3xl" />

        <div className="relative flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-sky-300/90">IncidentFlow</p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight text-white">Incidents</h1>
            <p className="mt-2 text-sm text-slate-300">Monitor and track active operational issues.</p>
          </div>

          <button
            type="button"
            onClick={onReportIncident}
            className="inline-flex items-center rounded-xl bg-white/100 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 cursor-pointer"
          >
            Report an Incident
          </button>

          <button
            type="button"
            onClick={refetch}
            className="inline-flex items-center rounded-xl bg-sky-400 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 cursor-pointer"
          >
            Refresh
          </button>
        </div>

        {incidents.length === 0 ? (
          <div className="relative mt-8 rounded-2xl border border-dashed border-slate-700/90 bg-slate-900/40 px-6 py-10 text-center">
            <p className="text-sm text-slate-300">No incidents found.</p>
          </div>
        ) : (
          <div className="relative mt-8 grid gap-6 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,1fr)] lg:items-start">
            <ul className="grid gap-4">
              {incidents.map((incident) => {
                const displayStatus = normalizeStatus(incident.status);
                const isSelected = selectedIncidentId === incident.id;

                return (
                  <li key={incident.id}>
                    <button
                      type="button"
                      onClick={() => setSelectedIncidentId(incident.id)}
                      className={`group w-full rounded-2xl border bg-slate-900/70 p-5 text-left transition hover:border-slate-700 hover:bg-slate-900 ${
                        isSelected
                          ? "border-sky-400/60 ring-1 ring-sky-400/40"
                          : "border-slate-800/80"
                      }`}
                    >
                      <div className="flex flex-wrap items-start justify-between gap-3">
                        <div className="space-y-2">
                          <h2 className="text-base font-semibold text-slate-100">{incident.title}</h2>
                          <p className="line-clamp-2 text-sm leading-relaxed text-slate-300">{incident.description}</p>
                        </div>

                        <span
                          className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[displayStatus] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}
                        >
                          {displayStatus}
                        </span>
                      </div>

                      <p className="mt-4 text-xs text-slate-400">
                        Created {new Date(incident.createdAt).toLocaleString()}
                      </p>
                    </button>
                  </li>
                );
              })}
            </ul>

            <IncidentDetails
              incident={selectedIncident}
              onEdit={() => {
                if (selectedIncident) {
                  onEditIncident(selectedIncident);
                }
              }}
            />
          </div>
        )}
      </div>
    </section>
  );
};
