// src/pages/AdminPage.tsx
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Shield, Users, AlertTriangle, Activity, ShieldAlert, FileText } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { dashboardService } from '../services/dashboardService';

import UserManagement from '../components/UserManagement';
import SystemSecurity from '../components/SystemSecurity';
import AdminDashboard from '../components/AdminDashboard';
import ContentManagement from '../components/ContentManagement';

export default function AdminPage() {
  const [activeTab, setActiveTab] = useState<'dashboard' | 'users' | 'security' | 'content'>('dashboard');

  const { data: statsData, isLoading: isLoadingStats, error } = useQuery({
    queryKey: ['dashboardStats'],
    queryFn: dashboardService.getStats,
  });

  if (error) {
    return (
      <div className="empty-state">
        <AlertTriangle size={48} color="var(--color-danger)" style={{ marginBottom: '1rem' }} />
        <h3>غير مصرح لك</h3>
        <p>لا تملك الصلاحيات الكافية لعرض هذه الصفحة.</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', flexWrap: 'wrap', gap: '1rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <Shield size={28} color="var(--color-primary-light)" />
          <div>
            <h1>لوحة الإدارة</h1>
            <p>إدارة المستخدمين والإحصائيات</p>
          </div>
        </div>
        
        {/* Tabs */}
        <div style={{ display: 'flex', gap: '0.5rem', background: 'var(--color-bg-elevated)', padding: '0.5rem', borderRadius: '12px' }}>
          <button 
            className={`btn ${activeTab === 'dashboard' ? 'btn-primary' : 'btn-ghost'}`}
            onClick={() => setActiveTab('dashboard')}
            style={{ borderRadius: '8px' }}
          >
            <Activity size={18} /> نظرة عامة
          </button>
          <button 
            className={`btn ${activeTab === 'users' ? 'btn-primary' : 'btn-ghost'}`}
            onClick={() => setActiveTab('users')}
            style={{ borderRadius: '8px' }}
          >
            <Users size={18} /> إدارة المستخدمين
          </button>
          <button 
            className={`btn ${activeTab === 'security' ? 'btn-primary' : 'btn-ghost'}`}
            onClick={() => setActiveTab('security')}
            style={{ borderRadius: '8px' }}
          >
            <ShieldAlert size={18} /> أدوات الأمان
          </button>
          <button 
            className={`btn ${activeTab === 'content' ? 'btn-primary' : 'btn-ghost'}`}
            onClick={() => setActiveTab('content')}
            style={{ borderRadius: '8px' }}
          >
            <FileText size={18} /> إدارة المحتوى
          </button>
        </div>
      </div>

      <AnimatePresence mode="wait">
        {activeTab === 'dashboard' ? (
          <motion.div 
            key="dashboard"
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 20 }}
            transition={{ duration: 0.2 }}
          >
            {/* Stats Section */}
      {!isLoadingStats && statsData && (
        <AdminDashboard statsData={statsData} />
      )}
      </motion.div>
      ) : activeTab === 'users' ? (
          <motion.div
            key="users"
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.2 }}
          >
            <UserManagement />
          </motion.div>
        ) : activeTab === 'content' ? (
          <motion.div
            key="content"
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.2 }}
          >
            <ContentManagement />
          </motion.div>
        ) : (
          <motion.div
            key="security"
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.2 }}
          >
            <SystemSecurity />
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
