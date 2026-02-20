import axios from "axios";

const csrfCookieName = import.meta.env.VITE_CSRF_COOKIE_NAME ?? "incidentflow_csrf";
const csrfHeaderName = import.meta.env.VITE_CSRF_HEADER_NAME ?? "X-CSRF-TOKEN";

const getCookieValue = (name: string): string | null => {
  if (typeof document === "undefined") {
    return null;
  }

  const encodedName = `${encodeURIComponent(name)}=`;
  const cookies = document.cookie.split(";");
  for (const cookie of cookies) {
    const trimmed = cookie.trim();
    if (trimmed.startsWith(encodedName)) {
      return decodeURIComponent(trimmed.substring(encodedName.length));
    }
  }

  return null;
};

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5238/api",
  withCredentials: true,
  headers: {
    "Content-Type": "application/json"
  }
});

apiClient.interceptors.request.use((config) => {
  const method = config.method?.toUpperCase();
  const isMutating = method === "POST" || method === "PUT" || method === "PATCH" || method === "DELETE";

  if (!isMutating) {
    return config;
  }

  const csrfToken = getCookieValue(csrfCookieName);
  if (!csrfToken) {
    return config;
  }

  config.headers.set(csrfHeaderName, csrfToken);
  return config;
});
