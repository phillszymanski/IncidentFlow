import { apiClient } from "../../api/axios";
import {
  type Incident,
  type IncidentComment,
  type IncidentLog,
  type IncidentCreateInput,
  type IncidentUpdateInput
} from "./types";

export type AssignableUser = {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: string;
};

export const getIncidents = async (): Promise<Incident[]> => {
  const response = await apiClient.get<Incident[]>("/Incident");
  return response.data;
};

export const createIncident = async (
  incident: IncidentCreateInput
): Promise<Incident> => {
  const response = await apiClient.post<Incident>("/Incident", incident);
  return response.data;
};

export const updateIncident = async (
  id: string,
  incident: IncidentUpdateInput
): Promise<void> => {
  await apiClient.put(`/Incident/${id}`, incident);
};

export const deleteIncident = async (id: string): Promise<void> => {
  await apiClient.delete(`/Incident/${id}`);
};

export const getIncidentComments = async (incidentId: string): Promise<IncidentComment[]> => {
  const response = await apiClient.get<IncidentComment[]>("/Comment", {
    params: { incidentId }
  });

  return response.data;
};

export const createIncidentComment = async (input: {
  incidentId: string;
  content: string;
}): Promise<void> => {
  await apiClient.post("/Comment", {
    incidentId: input.incidentId,
    content: input.content
  });
};

export const getIncidentLogs = async (incidentId: string): Promise<IncidentLog[]> => {
  const response = await apiClient.get<IncidentLog[]>(`/IncidentLog/incident/${incidentId}`);
  return response.data;
};

export const getAssignableUsers = async (): Promise<AssignableUser[]> => {
  const response = await apiClient.get<AssignableUser[]>("/User/assignable");
  return response.data;
};

export const getUserDirectory = async (): Promise<AssignableUser[]> => {
  const response = await apiClient.get<AssignableUser[]>("/User/directory");
  return response.data;
};
