import { useState, useEffect } from 'react';
import { Search, Bell, Settings, Moon, Sun, X } from 'lucide-react';
import { useAuthStore } from '../store/authStore';
import { motion, AnimatePresence } from 'framer-motion';
export default function Topbar() {
  const { user } = useAuthStore();
  const [theme, setTheme] = useState<'dark' | 'light'>('dark');
  const [isNotifOpen, setIsNotifOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  // Toggle Theme
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => prev === 'dark' ? 'light' : 'dark');
  };

  return (
    <header className="topbar">
      <div className="topbar-left">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input 
            type="text" 
            placeholder="ابحث في المهام، المستخدمين، أو التقارير..." 
            className="search-input" 
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          
          <AnimatePresence>
            {searchQuery && (
              <motion.div 
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: 10 }}
                className="search-results-dropdown glass-card"
                style={{
                  position: 'absolute', top: '110%', left: 0, right: 0, padding: '1rem',
                  display: 'flex', flexDirection: 'column', gap: '0.5rem', zIndex: 100
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.8rem', color: 'var(--color-text-secondary)' }}>نتائج البحث</span>
                  <X size={14} style={{ cursor: 'pointer', color: 'var(--color-text-secondary)' }} onClick={() => setSearchQuery('')} />
                </div>
                <div className="search-result-item" style={{ padding: '0.5rem', borderRadius: '8px', cursor: 'pointer', background: 'var(--color-bg-input)' }}>
                  مستخدم: <strong>أحمد</strong> (مثال)
                </div>
                <div className="search-result-item" style={{ padding: '0.5rem', borderRadius: '8px', cursor: 'pointer', background: 'var(--color-bg-input)' }}>
                  مهمة: إصلاح خطأ <strong>{searchQuery}</strong>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>

      <div className="topbar-right">
        <button className="icon-btn" onClick={toggleTheme} data-tooltip={theme === 'dark' ? 'الوضع النهاري' : 'الوضع الليلي'}>
          {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
        </button>
        
        <button className="icon-btn" data-tooltip="الإعدادات">
          <Settings size={20} />
        </button>
        
        <div className="notification-wrapper">
          <button className="icon-btn" data-tooltip="الإشعارات" onClick={() => setIsNotifOpen(!isNotifOpen)}>
            <Bell size={20} />
            <span className="notification-badge">3</span>
          </button>

          <AnimatePresence>
            {isNotifOpen && (
              <motion.div
                initial={{ opacity: 0, y: 10, scale: 0.95 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 10, scale: 0.95 }}
                className="glass-card"
                style={{
                  position: 'absolute', top: '120%', left: 0, width: '300px', padding: '1rem',
                  display: 'flex', flexDirection: 'column', gap: '0.75rem', zIndex: 100
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', borderBottom: '1px solid var(--color-border)', paddingBottom: '0.5rem' }}>
                  <strong style={{ fontSize: '0.9rem' }}>الإشعارات</strong>
                  <span className="badge badge-primary">3 جديدة</span>
                </div>
                
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  <div style={{ fontSize: '0.8rem', padding: '0.5rem', background: 'rgba(99, 102, 241, 0.1)', borderRadius: '6px' }}>
                    <strong>تسجيل جديد:</strong> قام مستخدم جديد بتسجيل الدخول.
                  </div>
                  <div style={{ fontSize: '0.8rem', padding: '0.5rem', background: 'rgba(239, 68, 68, 0.1)', borderRadius: '6px' }}>
                    <strong>تنبيه أمني:</strong> محاولة فاشلة لتسجيل الدخول.
                  </div>
                  <div style={{ fontSize: '0.8rem', padding: '0.5rem', background: 'rgba(34, 197, 94, 0.1)', borderRadius: '6px' }}>
                    <strong>نظام:</strong> تم إجراء نسخة احتياطية بنجاح.
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
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
