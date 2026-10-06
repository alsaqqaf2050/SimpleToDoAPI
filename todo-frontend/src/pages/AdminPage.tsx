// src/pages/AdminPage.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { Shield, Trash2, Users, AlertTriangle, CheckCircle, Clock, ListTodo } from 'lucide-react';
import { motion } from 'framer-motion';
import { userService } from '../services/userService';
import { dashboardService } from '../services/dashboardService';
import { useAuthStore } from '../store/authStore';
import type { AppUser } from '../types';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip as RechartsTooltip, Legend, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';

export default function AdminPage() {
  const queryClient = useQueryClient();
  const currentUser = useAuthStore((s) => s.user);
  const [page, setPage] = useState(1);

  const { data: statsData, isLoading: isLoadingStats } = useQuery({
    queryKey: ['dashboardStats'],
    queryFn: dashboardService.getStats,
  });

  const { data, isLoading, error } = useQuery({
    queryKey: ['users', page],
    queryFn: () => userService.getAll(page, 10),
  });

  const users = data?.data ?? [];
  const pagination = data?.pagination;

  const deleteMutation = useMutation({
    mutationFn: (id: string) => userService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('تم حذف المستخدم بنجاح');
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'فشل حذف المستخدم');
    }
  });

  const handleDelete = (user: AppUser) => {
    if (user.id === currentUser?.id) {
      toast.error('لا يمكنك حذف حسابك الحالي');
      return;
    }
    if (confirm(`هل أنت متأكد من حذف المستخدم "${user.fullName}" بشكل نهائي؟`)) {
      deleteMutation.mutate(user.id);
    }
  };

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
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <Shield size={28} color="var(--color-primary-light)" />
          <div>
            <h1>لوحة الإدارة</h1>
            <p>إدارة المستخدمين والإحصائيات</p>
          </div>
        </div>
      </div>

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

      <motion.div
        className="glass-card"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        style={{ padding: '1.5rem', overflowX: 'auto' }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem' }}>
          <Users size={18} />
          <h2 style={{ fontSize: '1.1rem', fontWeight: 600 }}>قائمة المستخدمين</h2>
        </div>

        {isLoading ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {[...Array(5)].map((_, i) => <div key={i} className="skeleton" style={{ height: 60 }} />)}
          </div>
        ) : (
          <table style={{ width: '100%', textAlign: 'right', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--color-border)' }}>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>الاسم</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>البريد الإلكتروني</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>الأدوار</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>تاريخ الانضمام</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} style={{ borderBottom: '1px solid var(--color-border)' }}>
                  <td style={{ padding: '1rem 0.5rem', fontWeight: 600 }}>
                    {u.fullName} {u.id === currentUser?.id && <span className="badge" style={{ fontSize: '0.6rem', padding: '2px 4px' }}>أنت</span>}
                  </td>
                  <td style={{ padding: '1rem 0.5rem', color: 'var(--color-text-secondary)' }}>{u.email}</td>
                  <td style={{ padding: '1rem 0.5rem' }}>
                    <div style={{ display: 'flex', gap: '0.25rem', flexWrap: 'wrap' }}>
                      {u.roles?.map(r => <span key={r} className="badge badge-accent" style={{ fontSize: '0.7rem' }}>{r}</span>)}
                    </div>
                  </td>
                  <td style={{ padding: '1rem 0.5rem', color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>
                    {new Date(u.createdDate).toLocaleDateString('ar-SA')}
                  </td>
                  <td style={{ padding: '1rem 0.5rem' }}>
                    <button
                      className="icon-btn danger"
                      onClick={() => handleDelete(u)}
                      disabled={u.id === currentUser?.id || deleteMutation.isPending}
                      data-tooltip="حذف المستخدم"
                    >
                      <Trash2 size={16} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {/* Pagination */}
        {pagination && pagination.totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '2rem' }}>
            <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p - 1)} disabled={page === 1}>
              السابق
            </button>
            {[...Array(pagination.totalPages)].map((_, i) => (
              <button
                key={i}
                className={`btn btn-sm ${page === i + 1 ? 'btn-primary' : 'btn-ghost'}`}
                onClick={() => setPage(i + 1)}
              >
                {i + 1}
              </button>
            ))}
            <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p + 1)} disabled={page === pagination.totalPages}>
              التالي
            </button>
          </div>
        )}
      </motion.div>
    </div>
  );
}
