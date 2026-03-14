import { useAuth } from '../../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

export function AppHeader() {
  const { user, logout, isManager } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-3 flex items-center justify-between">
      <div className="flex items-center gap-6">
        <Link to="/library" className="text-lg font-semibold text-gray-900 hover:text-blue-600">
          Legal Doc Comparator
        </Link>
        {isManager && (
          <nav className="flex gap-4 text-sm">
            <Link to="/library" className="text-gray-600 hover:text-blue-600">My Library</Link>
            <Link to="/manager" className="text-gray-600 hover:text-blue-600">Manager Dashboard</Link>
          </nav>
        )}
      </div>
      <div className="flex items-center gap-3">
        <span className="text-sm text-gray-600">{user?.email}</span>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${isManager ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>
          {user?.role}
        </span>
        <button
          onClick={handleLogout}
          className="text-sm text-gray-500 hover:text-red-600 transition-colors"
        >
          Logout
        </button>
      </div>
    </header>
  );
}
