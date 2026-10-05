// src/components/Layout.tsx
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store/authStore';
import { authService } from '../services/authService';
import { LayoutDashboard, CheckSquare, Shield, UserCircle, LogOut } from 'lucide-react';
import Topbar from './Topbar';

const navItems = [
  { path: '/dashboard', label: 'لوحة التحكم', icon: LayoutDashboard },
  { path: '/todos', label: 'مهامي', icon: CheckSquare },
  { path: '/profile', label: 'الملف الشخصي', icon: UserCircle },
];

export default function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { clearAuth, isAdmin } = useAuthStore();

  const logoutMutation = useMutation({
    mutationFn: authService.logout,
    onSuccess: () => {
      clearAuth();
      navigate('/login');
      toast.success('تم تسجيل الخروج بنجاح');
    },
    onError: () => {
      clearAuth();
      navigate('/login');
    },
  });

  return (
    <div style={{ display: 'flex' }}>
      {/* Sidebar */}
      <aside className="sidebar">
        {/* Brand */}
        <div className="brand">
          <div className="brand-icon">✓</div>
          <span className="brand-name">TodoApp</span>
        </div>

        {/* Navigation */}
        <nav style={{ flex: 1 }}>
          {navItems.map(({ path, label, icon: Icon }) => (
            <div
              key={path}
              className={`nav-item ${location.pathname === path ? 'active' : ''}`}
              onClick={() => navigate(path)}
            >
              <span className="nav-icon"><Icon size={18} /></span>
              {label}
            </div>
          ))}

          {isAdmin() && (
            <div
              className={`nav-item ${location.pathname.startsWith('/admin') ? 'active' : ''}`}
              style={{ marginTop: '0.5rem' }}
              onClick={() => navigate('/admin')}
            >
              <span className="nav-icon"><Shield size={18} /></span>
              لوحة الإدارة
              <span className="badge badge-accent" style={{ marginRight: 'auto', fontSize: '0.65rem', padding: '2px 6px' }}>Admin</span>
            </div>
          )}
        </nav>

        {/* Logout Button */}
        <div style={{ padding: '0.5rem', marginTop: 'auto' }}>
          <button
            className="btn btn-ghost"
            style={{ width: '100%', justifyContent: 'flex-start', color: 'var(--color-text-secondary)' }}
            onClick={() => logoutMutation.mutate()}
          >
            <LogOut size={18} />
            تسجيل خروج
          </button>
        </div>
      </aside>

      {/* Main */}
      <main className="main-content">
        <Topbar />
        <Outlet />
      </main>
    </div>
  );
}
