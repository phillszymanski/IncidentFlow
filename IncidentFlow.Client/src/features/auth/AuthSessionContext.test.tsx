import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthSessionProvider, useAuthSession } from "./AuthSessionContext";
import { getCurrentUser, logout } from "./authApi";
import type { AuthUser } from "./authApi";

vi.mock("./authApi", async () => {
  const actual = await vi.importActual<typeof import("./authApi")>("./authApi");
  return {
    ...actual,
    getCurrentUser: vi.fn(),
    logout: vi.fn()
  };
});

const mockedGetCurrentUser = vi.mocked(getCurrentUser);
const mockedLogout = vi.mocked(logout);

const baseUser: AuthUser = {
  id: "u-1",
  username: "alex",
  email: "alex@test.com",
  role: "Admin",
  permissions: ["incidents:read"]
};

const Probe = () => {
  const { authLoading, currentUser, signOut } = useAuthSession();

  return (
    <div>
      <div data-testid="loading">{authLoading ? "loading" : "ready"}</div>
      <div data-testid="username">{currentUser?.username ?? "none"}</div>
      <button
        type="button"
        onClick={() => {
          void signOut();
        }}
      >
        Sign out
      </button>
    </div>
  );
};

describe("AuthSessionProvider", () => {
  beforeEach(() => {
    mockedGetCurrentUser.mockReset();
    mockedLogout.mockReset();
  });

  it("hydrates session from API and exposes user", async () => {
    mockedGetCurrentUser.mockResolvedValue(baseUser);

    render(
      <AuthSessionProvider>
        <Probe />
      </AuthSessionProvider>
    );

    expect(screen.getByTestId("loading")).toHaveTextContent("loading");

    await waitFor(() => {
      expect(screen.getByTestId("loading")).toHaveTextContent("ready");
    });

    expect(screen.getByTestId("username")).toHaveTextContent("alex");
    expect(mockedGetCurrentUser).toHaveBeenCalledTimes(1);
  });

  it("signOut clears current user", async () => {
    mockedGetCurrentUser.mockResolvedValue(baseUser);
    mockedLogout.mockResolvedValue();

    render(
      <AuthSessionProvider>
        <Probe />
      </AuthSessionProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("loading")).toHaveTextContent("ready");
    });

    await userEvent.click(screen.getByRole("button", { name: "Sign out" }));

    await waitFor(() => {
      expect(screen.getByTestId("username")).toHaveTextContent("none");
    });

    expect(mockedLogout).toHaveBeenCalledTimes(1);
  });
});
