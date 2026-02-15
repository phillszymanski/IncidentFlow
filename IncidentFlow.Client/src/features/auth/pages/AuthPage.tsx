import { useState } from "react";
import type { FormEvent } from "react";
import { login, register, type AuthUser } from "../authApi";

type AuthPageProps = {
  onAuthenticated: (user: AuthUser) => void;
};

type AuthMode = "login" | "register";

export const AuthPage = ({ onAuthenticated }: AuthPageProps) => {
  const [mode, setMode] = useState<AuthMode>("login");
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isRegister = mode === "register";

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!username.trim() || !password.trim()) {
      setError("Username and password are required.");
      return;
    }

    if (isRegister && !email.trim()) {
      setError("Email is required for registration.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      if (isRegister) {
        await register({
          username: username.trim(),
          email: email.trim(),
          password: password.trim()
        });
      }

      const user = await login(isRegister ? username.trim() : username.trim(), password.trim());
      onAuthenticated(user);
    } catch (authError) {
      setError(authError instanceof Error ? authError.message : "Authentication failed.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="mx-auto flex min-h-screen w-full max-w-5xl items-center px-6 py-12">
      <div className="relative w-full overflow-hidden rounded-3xl border border-slate-800/70 bg-slate-900/70 p-8 shadow-2xl shadow-slate-950/60 backdrop-blur">
        <div className="pointer-events-none absolute -right-20 -top-16 h-56 w-56 rounded-full bg-sky-500/20 blur-3xl" />
        <div className="pointer-events-none absolute -bottom-20 -left-16 h-56 w-56 rounded-full bg-indigo-500/15 blur-3xl" />

        <div className="relative mx-auto max-w-md">
          <p className="text-center text-xs font-semibold uppercase tracking-[0.18em] text-sky-300/90">IncidentFlow</p>
          <h1 className="mt-2 text-center text-3xl font-semibold tracking-tight text-white">
            {isRegister ? "Create Account" : "Sign In"}
          </h1>
          <p className="mt-2 text-center text-sm text-slate-300">
            {isRegister
              ? "Register with username, email, and password."
              : "Sign in with your username (or email) and password."}
          </p>

          <form onSubmit={handleSubmit} className="mt-8 grid gap-4">
            {error ? (
              <div className="rounded-xl border border-rose-400/25 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">{error}</div>
            ) : null}

            <div>
              <label htmlFor="username" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
                Username{!isRegister ? " or Email" : ""}
              </label>
              <input
                id="username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
                placeholder={!isRegister ? "username or email" : "username"}
                required
              />
            </div>

            {isRegister ? (
              <div>
                <label htmlFor="email" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
                  placeholder="you@example.com"
                  required
                />
              </div>
            ) : null}

            <div>
              <label htmlFor="password" className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-300">
                Password
              </label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="w-full rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-sky-400/70 focus:ring-2 focus:ring-sky-400/30"
                placeholder="••••••••"
                required
              />
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="mt-2 inline-flex items-center justify-center rounded-xl bg-sky-400 px-5 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-sky-300 disabled:cursor-not-allowed disabled:opacity-70"
            >
              {submitting ? "Submitting..." : isRegister ? "Register & Sign In" : "Sign In"}
            </button>
          </form>

          <button
            type="button"
            onClick={() => {
              setMode((currentMode) => (currentMode === "login" ? "register" : "login"));
              setError(null);
            }}
            className="mt-5 w-full text-sm font-medium text-sky-300 transition hover:text-sky-200"
          >
            {isRegister ? "Already have an account? Sign in" : "Need an account? Register"}
          </button>
        </div>
      </div>
    </section>
  );
};
