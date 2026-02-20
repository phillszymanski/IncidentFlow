import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import App from "./App";
import type { AuthUser } from "./features/auth/authApi";

const mockUseAuthSession = vi.fn();

vi.mock("./features/auth/AuthSessionContext", () => ({
  useAuthSession: () => mockUseAuthSession()
}));

vi.mock("./features/auth/pages/AuthPage", () => ({
  AuthPage: ({ onAuthenticated }: { onAuthenticated: (user: AuthUser) => void }) => (
    <div>
      <span data-testid="auth-page">auth-page</span>
      <button
        type="button"
        onClick={() =>
          onAuthenticated({
            id: "u-1",
            username: "demo",
            email: "demo@test.com",
            role: "User",
            permissions: ["incidents:read"]
          })
        }
      >
        Complete Auth
      </button>
    </div>
  )
}));

vi.mock("./features/incidents/pages/IncidentListPage", () => ({
  IncidentListPage: ({ onReportIncident }: { onReportIncident: () => void }) => (
    <div>
      <span data-testid="incident-list">incident-list</span>
      <button type="button" onClick={onReportIncident}>
        Go New
      </button>
    </div>
  )
}));

vi.mock("./features/incidents/pages/NewIncidentPage", () => ({
  NewIncidentPage: () => <span data-testid="new-incident">new-incident</span>
}));

describe("App", () => {
  it("shows loading view while auth is hydrating", () => {
    mockUseAuthSession.mockReturnValue({
      authLoading: true,
      currentUser: null,
      setAuthenticatedUser: vi.fn(),
      signOut: vi.fn(),
      refreshSession: vi.fn()
    });

    render(<App />);

    expect(screen.getByText("Loading session...")).toBeInTheDocument();
  });

  it("shows auth page when user is not authenticated", () => {
    mockUseAuthSession.mockReturnValue({
      authLoading: false,
      currentUser: null,
      setAuthenticatedUser: vi.fn(),
      signOut: vi.fn(),
      refreshSession: vi.fn()
    });

    render(<App />);

    expect(screen.getByTestId("auth-page")).toBeInTheDocument();
  });

  it("shows incident list for authenticated user and signs out", async () => {
    const signOut = vi.fn().mockResolvedValue(undefined);

    mockUseAuthSession.mockReturnValue({
      authLoading: false,
      currentUser: {
        id: "u-1",
        username: "demo",
        email: "demo@test.com",
        role: "Admin",
        permissions: ["incidents:read"]
      },
      setAuthenticatedUser: vi.fn(),
      signOut,
      refreshSession: vi.fn()
    });

    render(<App />);

    expect(screen.getByTestId("incident-list")).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Logout" }));

    await waitFor(() => {
      expect(signOut).toHaveBeenCalledTimes(1);
    });
  });
});
