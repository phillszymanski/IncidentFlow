
import { useEffect, useMemo, useState } from "react";
import type { AuthUser } from "../../auth/authApi";
import { useIncidents } from "../../../hooks/useIncidents";
import { IncidentDetails } from "../components/IncidentDetails";
import { IncidentDashboard } from "../components/IncidentDashboard";
import {
  deleteIncident,
  getIncidentDashboardSummary,
  type IncidentDashboardSummary,
  type IncidentListFilter
} from "../incidentApi";
import { type Incident } from "../types";
import {
  incidentSeverityStyles,
  incidentStatusStyles,
  normalizeIncidentSeverity,
  normalizeIncidentStatus
} from "../presentation";

type IncidentListPageProps = {
  currentUser: AuthUser;
  onReportIncident: () => void;
  onEditIncident: (incident: Incident) => void;
};

export const IncidentListPage = ({ currentUser, onReportIncident, onEditIncident }: IncidentListPageProps) => {
  const [pageSize, setPageSize] = useState(10);
  const [currentPage, setCurrentPage] = useState(1);
  const [activeFilter, setActiveFilter] = useState<IncidentListFilter>("total");
  const { incidents, totalCount, totalPages, loading, error, refetch } = useIncidents(currentPage, pageSize, activeFilter);
  const [selectedIncidentId, setSelectedIncidentId] = useState<string | null>(null);
  const [dashboardSummary, setDashboardSummary] = useState<IncidentDashboardSummary | null>(null);
  const [dashboardLoading, setDashboardLoading] = useState(false);

  const hasPermission = (permission: string) => currentUser.permissions.includes(permission);
  const canCreateIncident = hasPermission("incidents:create");
  const canViewAuditLogs = hasPermission("incidents:audit:read");
  const canDeleteIncident = hasPermission("incidents:delete");
  const canEditAny = hasPermission("incidents:edit:any");
  const canEditOwn = hasPermission("incidents:edit:own");
  const canSetAnyStatus = hasPermission("incidents:status:any");
  const canSetOwnStatus = hasPermission("incidents:status:limited");
  const dashboardMode = hasPermission("dashboard:full") ? "full" : "basic";

  const refreshDashboardSummary = async () => {
    setDashboardLoading(true);
    try {
      const summary = await getIncidentDashboardSummary();
      setDashboardSummary(summary);
    } finally {
      setDashboardLoading(false);
    }
  };

  const refreshPageAndDashboard = async () => {
    await Promise.all([refetch(), refreshDashboardSummary()]);
  };

  const selectedIncident = useMemo(
    () => incidents.find((incident) => incident.id === selectedIncidentId) ?? null,
    [incidents, selectedIncidentId]
  );

  useEffect(() => {
    setCurrentPage(1);
  }, [pageSize]);

  useEffect(() => {
    setCurrentPage(1);
  }, [activeFilter]);

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [currentPage, totalPages]);

  useEffect(() => {
    void refreshDashboardSummary();
  }, []);

  const isSelectedIncidentOwned = useMemo(() => {
    if (!selectedIncident) {
      return false;
    }

    return selectedIncident.createdBy === currentUser.id || selectedIncident.assignedTo === currentUser.id;
  }, [currentUser.id, selectedIncident]);

  const canEditSelectedIncident = useMemo(() => {
    if (!selectedIncident) {
      return false;
    }

    return canEditAny || (canEditOwn && selectedIncident.createdBy === currentUser.id);
  }, [canEditAny, canEditOwn, currentUser.id, selectedIncident]);

  const canChangeSelectedIncidentStatus = useMemo(() => {
    if (!selectedIncident) {
      return false;
    }

    return canSetAnyStatus || (canSetOwnStatus && isSelectedIncidentOwned);
  }, [canSetAnyStatus, canSetOwnStatus, isSelectedIncidentOwned, selectedIncident]);

  const handleDeleteSelectedIncident = async () => {
    if (!selectedIncident || !canDeleteIncident) {
      return;
    }

    const confirmed = window.confirm("Are you sure you want to delete this incident?");
    if (!confirmed) {
      return;
    }

    await deleteIncident(selectedIncident.id);
    setSelectedIncidentId(null);
    await refreshPageAndDashboard();
  };

  const handleDashboardMetricClick = (metric: IncidentListFilter) => {
    setActiveFilter(metric);
  };

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
            onClick={() => {
              void refreshPageAndDashboard();
            }}
            className="mt-6 inline-flex items-center rounded-xl bg-rose-400 px-4 py-2.5 text-sm font-semibold text-slate-900 transition hover:bg-rose-300"
          >
            Retry
          </button>
        </div>
      </section>
    );
  }

  return (
    <section className="mx-auto w-full max-w-7xl px-6 py-12">
      <div className="relative overflow-hidden rounded-3xl border border-slate-800/70 bg-slate-900/70 p-8 shadow-2xl shadow-slate-950/60 backdrop-blur">
        <div className="pointer-events-none absolute -right-20 -top-16 h-56 w-56 rounded-full bg-sky-500/20 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-20 -left-16 h-56 w-56 rounded-full bg-indigo-500/15 blur-3xl" />

        <div className="relative flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-sky-300/90">IncidentFlow</p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight text-white">Incidents</h1>
            <p className="mt-2 text-sm text-slate-300">Monitor and track active operational issues.</p>
          </div>

          {canCreateIncident ? (
            <button
              type="button"
              onClick={onReportIncident}
              className="inline-flex items-center rounded-xl bg-white/100 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 cursor-pointer"
            >
              Report an Incident
            </button>
          ) : null}

          <button
            type="button"
            onClick={() => {
              void refreshPageAndDashboard();
            }}
            className="inline-flex items-center rounded-xl bg-sky-400 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 cursor-pointer"
          >
            Refresh
          </button>
        </div>

        <IncidentDashboard
          summary={dashboardSummary}
          loading={dashboardLoading}
          mode={dashboardMode}
          activeFilter={activeFilter}
          onMetricClick={handleDashboardMetricClick}
        />

        {totalCount === 0 ? (
          <div className="relative mt-8 rounded-2xl border border-dashed border-slate-700/90 bg-slate-900/40 px-6 py-10 text-center">
            <p className="text-sm text-slate-300">No incidents found.</p>
          </div>
        ) : (
          <div className="relative mt-8 grid gap-6 lg:grid-cols-[minmax(0,1.5fr)_minmax(0,2fr)] lg:items-start">
            <div className="space-y-4">
              <div className="flex items-center justify-between gap-3 rounded-xl border border-slate-800/80 bg-slate-900/60 px-4 py-3">
                <p className="text-xs text-slate-300">
                  Showing {incidents.length === 0 ? 0 : (currentPage - 1) * pageSize + 1}
                  -{Math.min(currentPage * pageSize, totalCount)} of {totalCount}
                </p>
                <div className="flex items-center gap-2">
                  <label htmlFor="pageSize" className="text-xs text-slate-300">Page size</label>
                  <select
                    id="pageSize"
                    value={pageSize}
                    onChange={(event) => setPageSize(Number(event.target.value))}
                    className="rounded-lg border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-100 outline-none transition focus:border-sky-400/70"
                  >
                    <option value={10}>10</option>
                    <option value={25}>25</option>
                    <option value={50}>50</option>
                  </select>
                </div>
              </div>

              <ul className="grid max-h-[calc(100vh-18rem)] gap-4 overflow-y-auto pr-1">
                {incidents.map((incident) => {
                  const displayStatus = normalizeIncidentStatus(incident.status);
                  const displaySeverity = normalizeIncidentSeverity(incident.severity);
                  const isSelected = selectedIncidentId === incident.id;

                  return (
                    <li key={incident.id}>
                      <button
                        type="button"
                        onClick={() => setSelectedIncidentId(incident.id)}
                        className={`group relative w-full rounded-2xl border bg-slate-900/70 p-5 text-left transition hover:border-slate-700 hover:bg-slate-900 ${
                          isSelected
                            ? "border-sky-400/60 ring-1 ring-sky-400/40"
                            : "border-slate-800/80"
                        }`}
                      >
                        <span
                          className={`absolute right-4 top-4 inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${incidentStatusStyles[displayStatus] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}
                        >
                          {displayStatus}
                        </span>

                        <div className="flex flex-wrap items-start justify-between gap-3">
                          <div className="space-y-2">
                            <h2 className="text-base font-semibold text-slate-100">{incident.title}</h2>
                            <span
                              className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${incidentSeverityStyles[displaySeverity] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}
                            >
                              {displaySeverity}
                            </span>
                            <p className="line-clamp-2 text-sm leading-relaxed text-slate-300">{incident.description}</p>
                          </div>
                        </div>

                        <p className="mt-4 text-xs text-slate-400">
                          Created {new Date(incident.createdAt).toLocaleString()}
                        </p>
                      </button>
                    </li>
                  );
                })}
              </ul>

              <div className="flex items-center justify-between gap-3 rounded-xl border border-slate-800/80 bg-slate-900/60 px-4 py-3">
                <button
                  type="button"
                  onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
                  disabled={currentPage === 1}
                  className="rounded-lg border border-slate-700 px-3 py-1.5 text-xs font-semibold text-slate-100 transition hover:border-slate-500 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Previous
                </button>
                <p className="text-xs text-slate-300">Page {currentPage} of {totalPages}</p>
                <button
                  type="button"
                  onClick={() => setCurrentPage((page) => Math.min(totalPages, page + 1))}
                  disabled={currentPage === totalPages}
                  className="rounded-lg border border-slate-700 px-3 py-1.5 text-xs font-semibold text-slate-100 transition hover:border-slate-500 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Next
                </button>
              </div>
            </div>

            <IncidentDetails
              incident={selectedIncident}
              canViewTimeline={canViewAuditLogs}
              canDelete={canDeleteIncident}
              canEdit={canEditSelectedIncident}
              canChangeStatus={canChangeSelectedIncidentStatus}
              onDelete={() => {
                void handleDeleteSelectedIncident();
              }}
              onIncidentUpdated={refreshPageAndDashboard}
              onEdit={() => {
                if (selectedIncident && canEditSelectedIncident) {
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
