import { IncidentSeverity, IncidentStatus } from "./types";

export const incidentStatusStyles: Record<string, string> = {
  Open: "bg-sky-500/15 text-sky-300 ring-1 ring-sky-400/30",
  InProgress: "bg-amber-500/15 text-amber-300 ring-1 ring-amber-400/30",
  Resolved: "bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-400/30",
  Closed: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30"
};

export const incidentSeverityStyles: Record<string, string> = {
  Low: "bg-slate-500/15 text-slate-300 ring-1 ring-slate-400/30",
  Medium: "bg-cyan-500/15 text-cyan-300 ring-1 ring-cyan-400/30",
  High: "bg-orange-500/20 text-orange-300 ring-1 ring-orange-400/40",
  Critical: "bg-rose-500/15 text-rose-300 ring-1 ring-rose-400/30"
};

export const incidentStatusOptions = ["Open", "InProgress", "Resolved", "Closed"] as const;
export type IncidentStatusOption = (typeof incidentStatusOptions)[number];

export const normalizeIncidentStatus = (rawStatus: string): string => {
  if (rawStatus in IncidentStatus) {
    const normalizedStatus = IncidentStatus[rawStatus as keyof typeof IncidentStatus];
    if (typeof normalizedStatus === "string") {
      return normalizedStatus;
    }
  }

  return rawStatus;
};

export const normalizeIncidentSeverity = (rawSeverity: string): string => {
  if (rawSeverity in IncidentSeverity) {
    const normalizedSeverity = IncidentSeverity[rawSeverity as keyof typeof IncidentSeverity];
    if (typeof normalizedSeverity === "string") {
      return normalizedSeverity;
    }
  }

  return rawSeverity;
};
