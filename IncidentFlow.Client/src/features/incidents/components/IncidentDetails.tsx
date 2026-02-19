import { useEffect, useState } from "react";
import {
  createIncidentComment,
  getIncidentComments,
  getIncidentLogs,
  getUserDirectory,
  updateIncident,
  type AssignableUser
} from "../incidentApi";
import { IncidentCommentsSection } from "./IncidentCommentsSection";
import { IncidentTimelineSection } from "./IncidentTimelineSection";
import {
  IncidentSeverity,
  IncidentStatus,
  type Incident,
  type IncidentComment,
  type IncidentLog
} from "../types";

type IncidentDetailsProps = {
  incident: Incident | null;
  canEdit?: boolean;
  canDelete?: boolean;
  canViewTimeline?: boolean;
  canChangeStatus?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
  onIncidentUpdated?: () => Promise<void>;
};

const statusOptions = ["Open", "InProgress", "Resolved", "Closed"] as const;

const statusStyles: Record<string, string> = {
  Open: "bg-sky-500/15 text-sky-300 ring-1 ring-sky-400/30",
  InProgress: "bg-amber-500/15 text-amber-300 ring-1 ring-amber-400/30",
  Resolved: "bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-400/30",
  Closed: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"
};

const severityStyles: Record<string, string> = {
  Low: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30",
  Medium: "bg-cyan-500/15 text-cyan-300 ring-1 ring-cyan-400/30",
  High: "bg-orange-500/20 text-orange-300 ring-1 ring-orange-400/40",
  Critical: "bg-rose-500/15 text-rose-300 ring-1 ring-rose-400/30"
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

export const IncidentDetails = ({
  incident,
  canEdit = false,
  canDelete = false,
  canViewTimeline = false,
  canChangeStatus = false,
  onEdit,
  onDelete,
  onIncidentUpdated
}: IncidentDetailsProps) => {
  const [newComment, setNewComment] = useState("");
  const [statusValue, setStatusValue] = useState<(typeof statusOptions)[number]>("Open");
  const [statusSubmitting, setStatusSubmitting] = useState(false);
  const [comments, setComments] = useState<IncidentComment[]>([]);
  const [logs, setLogs] = useState<IncidentLog[]>([]);
  const [users, setUsers] = useState<AssignableUser[]>([]);
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [logsLoading, setLogsLoading] = useState(false);
  const [usersLoading, setUsersLoading] = useState(false);
  const [commentSubmitting, setCommentSubmitting] = useState(false);
  const [commentError, setCommentError] = useState<string | null>(null);
  const [logsError, setLogsError] = useState<string | null>(null);
  const [statusError, setStatusError] = useState<string | null>(null);

  useEffect(() => {
    if (!incident) {
      return;
    }

    const currentStatus = getDisplayStatus(incident.status);
    if (statusOptions.includes(currentStatus as (typeof statusOptions)[number])) {
      setStatusValue(currentStatus as (typeof statusOptions)[number]);
    }
  }, [incident]);

  useEffect(() => {
    const loadUsers = async () => {
      if (!incident) {
        setUsers([]);
        return;
      }

      setUsersLoading(true);
      try {
        const directory = await getUserDirectory();
        setUsers(directory);
      } finally {
        setUsersLoading(false);
      }
    };

    void loadUsers();
  }, [incident]);

  useEffect(() => {
    const loadLogs = async () => {
      if (!incident) {
        setLogs([]);
        return;
      }

      if (!canViewTimeline) {
        setLogs([]);
        setLogsLoading(false);
        setLogsError(null);
        return;
      }

      setLogsLoading(true);
      setLogsError(null);

      try {
        const response = await getIncidentLogs(incident.id);
        setLogs(response);
      } catch (error) {
        setLogsError(error instanceof Error ? error.message : "Failed to load activity timeline.");
      } finally {
        setLogsLoading(false);
      }
    };

    void loadLogs();
  }, [canViewTimeline, incident]);

  useEffect(() => {
    const loadComments = async () => {
      if (!incident) {
        setComments([]);
        return;
      }

      setCommentsLoading(true);
      setCommentError(null);

      try {
        const response = await getIncidentComments(incident.id);
        setComments(response);
      } catch (error) {
        setCommentError(error instanceof Error ? error.message : "Failed to load comments.");
      } finally {
        setCommentsLoading(false);
      }
    };

    void loadComments();
  }, [incident]);

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
  const assignedUser = incident.assignedTo
    ? users.find((user) => user.id === incident.assignedTo) ?? null
    : null;
  const assignedUserDisplay = incident.assignedTo
    ? assignedUser
      ? `${assignedUser.fullName} (${assignedUser.role})`
      : usersLoading
        ? "Loading..."
        : "Unknown user"
    : "Unassigned";
  const lastUpdatedAt = incident.updatedAt ?? incident.resolvedAt ?? incident.createdAt;

  const handleStatusChange = async (nextStatus: (typeof statusOptions)[number]) => {
    if (!incident || !canChangeStatus) {
      return;
    }

    setStatusSubmitting(true);
    setStatusError(null);
    setStatusValue(nextStatus);

    try {
      await updateIncident(incident.id, {
        status: IncidentStatus[nextStatus],
        resolvedAt: nextStatus === "Resolved" ? new Date().toISOString() : null
      });

      if (onIncidentUpdated) {
        await onIncidentUpdated();
      }
    } catch (error) {
      setStatusError(error instanceof Error ? error.message : "Failed to update status.");
      setStatusValue(displayStatus as (typeof statusOptions)[number]);
    } finally {
      setStatusSubmitting(false);
    }
  };

  const handlePostComment = async () => {
    if (!newComment.trim()) {
      return;
    }

    setCommentSubmitting(true);
    setCommentError(null);

    try {
      await createIncidentComment({
        incidentId: incident.id,
        content: newComment.trim()
      });

      const latestComments = await getIncidentComments(incident.id);
      setComments(latestComments);
      setNewComment("");
    } catch (error) {
      setCommentError(error instanceof Error ? error.message : "Failed to post comment.");
    } finally {
      setCommentSubmitting(false);
    }
  };

  return (
    <div className="lg:sticky lg:top-6">
      <div className={`grid gap-3 lg:items-start ${canViewTimeline ? "lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]" : "lg:grid-cols-1"}`}>
        <div className="space-y-3">
          <aside className="rounded-2xl border border-slate-800/80 bg-slate-900/70 p-6 shadow-xl shadow-slate-950/40">
            <div className="flex items-start justify-between gap-3">
              <h2 className="text-lg font-semibold text-slate-100">Incident Details</h2>
              {canChangeStatus ? (
                <select
                  value={statusValue}
                  onChange={(event) => {
                    void handleStatusChange(event.target.value as (typeof statusOptions)[number]);
                  }}
                  disabled={statusSubmitting}
                  className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs font-semibold text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
                >
                  {statusOptions.map((statusOption) => (
                    <option key={statusOption} value={statusOption}>
                      {statusOption}
                    </option>
                  ))}
                </select>
              ) : (
                <span
                  className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${statusStyles[displayStatus] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}
                >
                  {displayStatus}
                </span>
              )}
            </div>
            {statusError ? <p className="mt-3 text-xs text-rose-300">{statusError}</p> : null}

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
                  <span className={`mt-1 inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${severityStyles[displaySeverity] ?? "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"}`}>
                    {displaySeverity}
                  </span>
                </div>

                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Assigned User</p>
                  <p className="mt-1 text-sm text-slate-200">{assignedUserDisplay}</p>
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

          {canEdit || canDelete ? (
            <div className="grid w-full grid-cols-2 gap-3">
              {canEdit ? (
                <button
                  type="button"
                  onClick={onEdit}
                  className="inline-flex w-full items-center justify-center rounded-xl border border-slate-700 bg-slate-900 px-4 py-2.5 text-sm font-semibold text-slate-100 transition hover:border-slate-500"
                >
                  Edit
                </button>
              ) : (
                <div />
              )}
              {canDelete ? (
                <button
                  type="button"
                  onClick={onDelete}
                  className="inline-flex w-full items-center justify-center rounded-xl bg-rose-500/90 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-rose-400"
                >
                  Delete
                </button>
              ) : (
                <div />
              )}
            </div>
          ) : null}

          <IncidentCommentsSection
            newComment={newComment}
            onNewCommentChange={setNewComment}
            onPostComment={() => {
              void handlePostComment();
            }}
            commentSubmitting={commentSubmitting}
            commentError={commentError}
            commentsLoading={commentsLoading}
            comments={comments}
          />
        </div>

        {canViewTimeline ? (
          <IncidentTimelineSection
            logsLoading={logsLoading}
            logsError={logsError}
            logs={logs}
          />
        ) : null}
      </div>
    </div>
  );
};
