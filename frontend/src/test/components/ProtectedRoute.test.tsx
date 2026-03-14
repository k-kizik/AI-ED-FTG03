import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { vi } from 'vitest';
import { ProtectedRoute, ManagerRoute } from '../../components/shared/ProtectedRoute';
import * as AuthContextModule from '../../context/AuthContext';

function DummyPage({ label }: { label: string }) {
  return <div data-testid="page">{label}</div>;
}

function renderWithRoutes(token: string | null, isManager: boolean) {
  vi.spyOn(AuthContextModule, 'useAuth').mockReturnValue({
    token,
    user: token ? { userId: 'u1', email: 'e@e.com', role: isManager ? 'Manager' : 'User' } : null,
    isManager,
    login: vi.fn(),
    logout: vi.fn(),
  });

  render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/protected" element={<DummyPage label="Protected Content" />} />
        </Route>
        <Route path="/login" element={<DummyPage label="Login Page" />} />
        <Route path="/library" element={<DummyPage label="Library Page" />} />
        <Route element={<ManagerRoute />}>
          <Route path="/manager" element={<DummyPage label="Manager Content" />} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe('ProtectedRoute', () => {
  afterEach(() => vi.restoreAllMocks());

  it('renders children when token is present', () => {
    renderWithRoutes('jwt-token', false);
    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('redirects to /login when no token', () => {
    renderWithRoutes(null, false);
    expect(screen.getByText('Login Page')).toBeInTheDocument();
  });
});

describe('ManagerRoute', () => {
  afterEach(() => vi.restoreAllMocks());

  function renderManagerRoute(isManager: boolean) {
    vi.spyOn(AuthContextModule, 'useAuth').mockReturnValue({
      token: 'jwt',
      user: { userId: 'u1', email: 'e@e.com', role: isManager ? 'Manager' : 'User' },
      isManager,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={['/manager']}>
        <Routes>
          <Route element={<ManagerRoute />}>
            <Route path="/manager" element={<DummyPage label="Manager Content" />} />
          </Route>
          <Route path="/library" element={<DummyPage label="Library Page" />} />
        </Routes>
      </MemoryRouter>,
    );
  }

  it('renders children when user is manager', () => {
    renderManagerRoute(true);
    expect(screen.getByText('Manager Content')).toBeInTheDocument();
  });

  it('redirects to /library when user is not manager', () => {
    renderManagerRoute(false);
    expect(screen.getByText('Library Page')).toBeInTheDocument();
  });
});
