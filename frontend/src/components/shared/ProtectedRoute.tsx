import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export function ProtectedRoute() {
  const { token } = useAuth();
  return token ? <Outlet /> : <Navigate to="/login" replace />;
}

export function ManagerRoute() {
  const { isManager } = useAuth();
  return isManager ? <Outlet /> : <Navigate to="/library" replace />;
}
