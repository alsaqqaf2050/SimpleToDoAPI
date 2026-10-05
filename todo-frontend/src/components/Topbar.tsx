// src/components/Topbar.tsx
import { Search, Bell, Settings } from 'lucide-react';
import { useAuthStore } from '../store/authStore';

export default function Topbar() {
  const { user } = useAuthStore();

  return (
    <header className="topbar">
      <div className="topbar-left">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="ابحث في المهام، المستخدمين، أو التقارير..." className="search-input" />
        </div>
      </div>

      <div className="topbar-right">
        <button className="icon-btn" data-tooltip="الإعدادات">
          <Settings size={20} />
        </button>
        
        <div className="notification-wrapper">
          <button className="icon-btn" data-tooltip="الإشعارات">
            <Bell size={20} />
            <span className="notification-badge">3</span>
          </button>
        </div>

        <div className="topbar-divider"></div>

        <div className="topbar-user">
          <div className="topbar-user-info">
            <span className="topbar-user-name">{user?.fullName}</span>
            <span className="topbar-user-role">{user?.roles?.[0] || 'مستخدم'}</span>
          </div>
          <div className="topbar-avatar">
            {user?.fullName?.split(' ').slice(0, 2).map(n => n[0]).join('') ?? '?'}
          </div>
        </div>
      </div>
    </header>
  );
}
