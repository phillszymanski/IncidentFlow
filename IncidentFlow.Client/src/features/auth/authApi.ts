import { apiClient } from "../../api/axios";

export type AuthUser = {
  id: string;
  username: string;
  email: string;
  role: string;
};

type LoginResponse = {
  user: AuthUser;
};

type MeResponse = {
  userId?: string;
  username?: string;
  email?: string;
  role?: string;
};

export const login = async (usernameOrEmail: string, password: string): Promise<AuthUser> => {
  const response = await apiClient.post<LoginResponse>("/Auth/login", {
    usernameOrEmail,
    password
  });

  return response.data.user;
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
      role: response.data.role
    };
  } catch {
    return null;
  }
};
