export interface Incident {
  id: string;
  title: string;
  description: string;
  status: string;
  severity: string;
  createdBy: string;
  assignedTo?: string | null;
  createdAt: string;
  resolvedAt?: string | null;
  updatedAt: string;
}

export interface IncidentCreateInput {
  title: string;
  description: string;
  severity: number;
  createdBy: string;
  assignedTo?: string | null;
}

export interface IncidentUpdateInput {
  title?: string;
  description?: string;
  status?: number;
  severity?: number;
  assignedTo?: string | null;
  resolvedAt?: string | null;
}

export interface IncidentComment {
  id: string;
  incidentId: string;
  content: string;
  createdAt: string;
  createdByUserId: string;
}

export interface IncidentLog {
  id: string;
  incidentId: string;
  action: string;
  details: string;
  createdAt: string;
  performedByUserId: string;
}

export enum IncidentStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

export enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical
}