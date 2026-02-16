import { useEffect, useState } from "react"
import { AuthPage } from "./features/auth/pages/AuthPage"
import { getCurrentUser, logout, type AuthUser } from "./features/auth/authApi"
import { IncidentListPage } from "./features/incidents/pages/IncidentListPage"
import { NewIncidentPage } from "./features/incidents/pages/NewIncidentPage"
import type { Incident } from "./features/incidents/types"

type AppPage = "list" | "new" | "edit"

function App() {
  const [page, setPage] = useState<AppPage>("list")
  const [listVersion, setListVersion] = useState(0)
  const [editingIncident, setEditingIncident] = useState<Incident | null>(null)
  const [authLoading, setAuthLoading] = useState(true)
  const [currentUser, setCurrentUser] = useState<AuthUser | null>(null)

  useEffect(() => {
    const hydrateSession = async () => {
      const user = await getCurrentUser()
      setCurrentUser(user)
      setAuthLoading(false)
    }

    void hydrateSession()
  }, [])

  const handleReportIncident = () => {
    setEditingIncident(null)
    setPage("new")
  }

  const handleEditIncident = (incident: Incident) => {
    setEditingIncident(incident)
    setPage("edit")
  }

  const handleCancelCreate = () => {
    setEditingIncident(null)
    setPage("list")
  }

  const handleSaved = () => {
    setEditingIncident(null)
    setListVersion((currentVersion) => currentVersion + 1)
    setPage("list")
  }

  const handleAuthenticated = (user: AuthUser) => {
    setCurrentUser(user)
    setPage("list")
  }

  const handleLogout = async () => {
    await logout()
    setCurrentUser(null)
    setEditingIncident(null)
    setPage("list")
  }

  if (authLoading) {
    return (
      <main className="min-h-screen bg-slate-950 text-slate-100 antialiased">
        <section className="mx-auto flex min-h-screen w-full max-w-5xl items-center px-6 py-12">
          <div className="w-full rounded-3xl border border-slate-800/80 bg-slate-900/60 p-10 shadow-2xl shadow-slate-950/60 backdrop-blur">
            <p className="text-lg font-medium text-slate-200">Loading session...</p>
            <div className="mt-6 h-2 w-full overflow-hidden rounded-full bg-slate-800">
              <div className="h-full w-1/2 animate-pulse rounded-full bg-sky-400" />
            </div>
          </div>
        </section>
      </main>
    )
  }

  if (!currentUser) {
    return (
      <main className="min-h-screen bg-slate-950 text-slate-100 antialiased">
        <AuthPage onAuthenticated={handleAuthenticated} />
      </main>
    )
  }

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 antialiased">
      <div className="mx-auto flex w-full max-w-5xl items-center justify-between px-6 pt-6 text-xs text-slate-300">
        <span>
          Signed in as <span className="font-semibold text-slate-100">{currentUser.username}</span> ({currentUser.role})
        </span>
        <button
          type="button"
          onClick={() => {
            void handleLogout()
          }}
          className="rounded-lg border border-slate-700 px-3 py-1.5 font-semibold text-slate-200 transition hover:border-slate-500"
        >
          Logout
        </button>
      </div>
      {page === "list" ? (
        <IncidentListPage
          key={listVersion}
          currentUser={currentUser}
          onReportIncident={handleReportIncident}
          onEditIncident={handleEditIncident}
        />
      ) : (
        <NewIncidentPage
          currentUser={currentUser}
          mode={page === "edit" ? "edit" : "create"}
          incident={editingIncident}
          onCancel={handleCancelCreate}
          onSaved={handleSaved}
        />
      )}
    </main>
  )
}

export default App
