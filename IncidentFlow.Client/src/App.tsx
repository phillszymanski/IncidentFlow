import { useState } from "react"
import { IncidentListPage } from "./features/incidents/pages/IncidentListPage"
import { NewIncidentPage } from "./features/incidents/pages/NewIncidentPage"
import type { Incident } from "./features/incidents/types"

type AppPage = "list" | "new" | "edit"

function App() {
  const [page, setPage] = useState<AppPage>("list")
  const [listVersion, setListVersion] = useState(0)
  const [editingIncident, setEditingIncident] = useState<Incident | null>(null)

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

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 antialiased">
      {page === "list" ? (
        <IncidentListPage
          key={listVersion}
          onReportIncident={handleReportIncident}
          onEditIncident={handleEditIncident}
        />
      ) : (
        <NewIncidentPage
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
