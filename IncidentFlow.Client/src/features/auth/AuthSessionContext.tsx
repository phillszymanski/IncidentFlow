/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { getCurrentUser, logout, type AuthUser } from "./authApi";

type AuthSessionContextValue = {
  authLoading: boolean;
  currentUser: AuthUser | null;
  setAuthenticatedUser: (user: AuthUser | null) => void;
  signOut: () => Promise<void>;
  refreshSession: () => Promise<void>;
};

const AuthSessionContext = createContext<AuthSessionContextValue | undefined>(undefined);

type AuthSessionProviderProps = {
  children: ReactNode;
};

export const AuthSessionProvider = ({ children }: AuthSessionProviderProps) => {
  const [authLoading, setAuthLoading] = useState(true);
  const [currentUser, setCurrentUser] = useState<AuthUser | null>(null);

  const refreshSession = async () => {
    const user = await getCurrentUser();
    setCurrentUser(user);
  };

  useEffect(() => {
    const hydrateSession = async () => {
      try {
        await refreshSession();
      } finally {
        setAuthLoading(false);
      }
    };

    void hydrateSession();
  }, []);

  const signOut = async () => {
    await logout();
    setCurrentUser(null);
  };

  const value = useMemo<AuthSessionContextValue>(
    () => ({
      authLoading,
      currentUser,
      setAuthenticatedUser: setCurrentUser,
      signOut,
      refreshSession
    }),
    [authLoading, currentUser]
  );

  return <AuthSessionContext.Provider value={value}>{children}</AuthSessionContext.Provider>;
};

export const useAuthSession = (): AuthSessionContextValue => {
  const context = useContext(AuthSessionContext);
  if (!context) {
    throw new Error("useAuthSession must be used within AuthSessionProvider.");
  }

  return context;
};
