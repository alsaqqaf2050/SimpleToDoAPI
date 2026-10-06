// src/pages/AdminPage.tsx
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Shield, Users, AlertTriangle, CheckCircle, Clock, ListTodo, Activity, ShieldAlert } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { dashboardService } from '../services/dashboardService';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip as RechartsTooltip, Legend, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import UserManagement from '../components/UserManagement';
import SystemSecurity from '../components/SystemSecurity';

export default function AdminPage() {
  const [activeTab, setActiveTab] = useState<'dashboard' | 'users' | 'security'>('dashboard');

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
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem', marginBottom: '2rem' }}>
          {/* Quick Stats Cards */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1.5rem' }}>
            <motion.div className="glass-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }} style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(99, 102, 241, 0.1)', color: '#6366f1' }}>
                <ListTodo size={24} />
              </div>
              <div>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>إجمالي المهام</p>
                <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.totalTasks}</h3>
              </div>
            </motion.div>
            
            <motion.div className="glass-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }} style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(34, 197, 94, 0.1)', color: '#22c55e' }}>
                <CheckCircle size={24} />
              </div>
              <div>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>المهام المنجزة</p>
                <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.completedTasks}</h3>
              </div>
            </motion.div>
            
            <motion.div className="glass-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' }}>
                <Clock size={24} />
              </div>
              <div>
                <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>قيد الانتظار</p>
                <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.pendingTasks}</h3>
              </div>
            </motion.div>
          </div>

          {/* Charts Section */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem' }}>
            <motion.div className="glass-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }} style={{ padding: '1.5rem' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>حالة المهام</h3>
              <div style={{ height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={[
                        { name: 'منجزة', value: statsData.completedTasks },
                        { name: 'قيد الانتظار', value: statsData.pendingTasks }
                      ]}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={80}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      <Cell fill="#22c55e" />
                      <Cell fill="#f59e0b" />
                    </Pie>
                    <RechartsTooltip contentStyle={{ borderRadius: '8px', border: 'none', background: 'var(--color-bg-elevated)', color: 'var(--color-text)' }} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </motion.div>

            <motion.div className="glass-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} style={{ padding: '1.5rem' }}>
              <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>أداء المستخدمين</h3>
              <div style={{ height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={statsData.employeeStats.slice(0, 5)}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                    <XAxis dataKey="fullName" stroke="var(--color-text-muted)" fontSize={12} tickLine={false} axisLine={false} />
                    <YAxis stroke="var(--color-text-muted)" fontSize={12} tickLine={false} axisLine={false} />
                    <RechartsTooltip contentStyle={{ borderRadius: '8px', border: 'none', background: 'var(--color-bg-elevated)', color: 'var(--color-text)' }} cursor={{fill: 'var(--color-border)', opacity: 0.4}} />
                    <Legend />
                    <Bar dataKey="completedTasks" name="منجزة" fill="#22c55e" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="pendingTasks" name="قيد الانتظار" fill="#f59e0b" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </motion.div>
          </div>
        </div>
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
