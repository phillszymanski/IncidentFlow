import { apiClient } from "../../api/axios";

export type AuthUser = {
  id: string;
  username: string;
  email: string;
  role: string;
  permissions: string[];
};

type LoginResponse = {
  user: AuthUser;
};

type MeResponse = {
  userId?: string;
  username?: string;
  email?: string;
  role?: string;
  permissions?: string[];
};

const getPermissionsForRole = (role: string): string[] => {
  switch (role) {
    case "Admin":
      return [
        "incidents:read",
        "incidents:create",
        "incidents:edit:any",
        "incidents:status:any",
        "incidents:severity:any",
        "incidents:assign",
        "incidents:delete",
        "incidents:restore",
        "incidents:audit:read",
        "users:manage",
        "roles:manage",
        "dashboard:basic",
        "dashboard:full"
      ];
    case "Manager":
    case "Responder":
      return [
        "incidents:read",
        "incidents:create",
        "incidents:edit:any",
        "incidents:status:any",
        "incidents:severity:any",
        "incidents:assign",
        "dashboard:basic",
        "dashboard:full"
      ];
    case "User":
      return [
        "incidents:read",
        "incidents:create",
        "incidents:edit:own",
        "incidents:status:limited",
        "dashboard:basic"
      ];
    default:
      return ["incidents:read", "dashboard:basic"];
  }
};

export const login = async (usernameOrEmail: string, password: string): Promise<AuthUser> => {
  const response = await apiClient.post<LoginResponse>("/Auth/login", {
    usernameOrEmail,
    password
  });

  const currentUser = await getCurrentUser();
  if (currentUser) {
    return currentUser;
  }

  return {
    ...response.data.user,
    permissions: getPermissionsForRole(response.data.user.role)
  };
};

export const register = async (input: {
  username: string;
  email: string;
  password: string;
}): Promise<void> => {
  await apiClient.post("/User", {
    username: input.username,
    email: input.email,
    fullName: input.username,
    password: input.password
  });
};

export const logout = async (): Promise<void> => {
  await apiClient.post("/Auth/logout");
};

export const getCurrentUser = async (): Promise<AuthUser | null> => {
  try {
    const response = await apiClient.get<MeResponse>("/Auth/me");
    if (!response.data.userId || !response.data.username || !response.data.email || !response.data.role) {
      return null;
    }

    return {
      id: response.data.userId,
      username: response.data.username,
      email: response.data.email,
      role: response.data.role,
      permissions: response.data.permissions ?? getPermissionsForRole(response.data.role)
    };
  } catch {
    return null;
  }
};
