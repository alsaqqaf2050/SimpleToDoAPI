// src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import TodosPage from './pages/TodosPage';
import ProfilePage from './pages/ProfilePage';
import AdminPage from './pages/AdminPage';
import Layout from './components/Layout';

// Protected Route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

// Admin Route wrapper
function AdminRoute({ children }: { children: React.ReactNode }) {
  const isAdmin = useAuthStore((s) => s.isAdmin());
  return isAdmin ? <>{children}</> : <Navigate to="/dashboard" replace />;
}

// Public Route (redirect if logged in)
function PublicRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return !isAuthenticated ? <>{children}</> : <Navigate to="/dashboard" replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
        <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />

        {/* Protected Routes */}
        <Route
          path="/"
          element={<ProtectedRoute><Layout /></ProtectedRoute>}
        >
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="todos" element={<TodosPage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
