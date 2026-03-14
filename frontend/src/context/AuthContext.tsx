import { createContext, useContext, useReducer, useCallback, type ReactNode } from 'react';
import type { LoginResult } from '../types/api';

interface AuthState {
  token: string | null;
  user: Omit<LoginResult, 'token'> | null;
}

type AuthAction =
  | { type: 'LOGIN'; payload: LoginResult }
  | { type: 'LOGOUT' };

function authReducer(_state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case 'LOGIN':
      return { token: action.payload.token, user: { userId: action.payload.userId, email: action.payload.email, role: action.payload.role } };
    case 'LOGOUT':
      return { token: null, user: null };
  }
}

function loadInitialState(): AuthState {
  try {
    const token = localStorage.getItem('token');
    const userRaw = localStorage.getItem('user');
    if (token && userRaw) return { token, user: JSON.parse(userRaw) };
  } catch { /* ignore */ }
  return { token: null, user: null };
}

interface AuthContextValue extends AuthState {
  login: (result: LoginResult) => void;
  logout: () => void;
  isManager: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, undefined, loadInitialState);

  const loginFn = useCallback((result: LoginResult) => {
    localStorage.setItem('token', result.token);
    localStorage.setItem('user', JSON.stringify({ userId: result.userId, email: result.email, role: result.role }));
    dispatch({ type: 'LOGIN', payload: result });
  }, []);

  const logoutFn = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    dispatch({ type: 'LOGOUT' });
  }, []);

  const value: AuthContextValue = {
    ...state,
    login: loginFn,
    logout: logoutFn,
    isManager: state.user?.role === 'Manager',
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
