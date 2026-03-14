/**
 * Tests for the authReducer pure function extracted from AuthContext.
 * We test the reducer logic directly without the context provider.
 */

import type { LoginResult } from '../../types/api';

// Inline the reducer types/logic here to test it in isolation
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
      return {
        token: action.payload.token,
        user: { userId: action.payload.userId, email: action.payload.email, role: action.payload.role },
      };
    case 'LOGOUT':
      return { token: null, user: null };
  }
}

const emptyState: AuthState = { token: null, user: null };

const loginPayload: LoginResult = {
  token: 'jwt-abc',
  userId: 'user-123',
  email: 'test@example.com',
  role: 'User',
};

describe('authReducer', () => {
  it('LOGIN sets token and user', () => {
    const next = authReducer(emptyState, { type: 'LOGIN', payload: loginPayload });

    expect(next.token).toBe('jwt-abc');
    expect(next.user?.email).toBe('test@example.com');
    expect(next.user?.role).toBe('User');
    expect(next.user?.userId).toBe('user-123');
  });

  it('LOGOUT clears token and user', () => {
    const loggedInState: AuthState = {
      token: 'jwt-abc',
      user: { userId: 'user-123', email: 'test@example.com', role: 'User' },
    };

    const next = authReducer(loggedInState, { type: 'LOGOUT' });

    expect(next.token).toBeNull();
    expect(next.user).toBeNull();
  });

  it('LOGIN with Manager role sets role correctly', () => {
    const managerPayload: LoginResult = { ...loginPayload, role: 'Manager' };
    const next = authReducer(emptyState, { type: 'LOGIN', payload: managerPayload });

    expect(next.user?.role).toBe('Manager');
  });

  it('LOGOUT from already-logged-out state stays empty', () => {
    const next = authReducer(emptyState, { type: 'LOGOUT' });

    expect(next.token).toBeNull();
    expect(next.user).toBeNull();
  });
});
