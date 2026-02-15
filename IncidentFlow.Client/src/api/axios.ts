import axios from "axios";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5238/api",
  withCredentials: true,
  headers: {
    "Content-Type": "application/json"
  }
});
