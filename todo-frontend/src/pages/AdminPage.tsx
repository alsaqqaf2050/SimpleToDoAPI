// src/pages/AdminPage.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { Shield, Trash2, Users, AlertTriangle } from 'lucide-react';
import { motion } from 'framer-motion';
import { userService } from '../services/userService';
import { useAuthStore } from '../store/authStore';
import type { AppUser } from '../types';

export default function AdminPage() {
  const queryClient = useQueryClient();
  const currentUser = useAuthStore((s) => s.user);
  const [page, setPage] = useState(1);

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
            <p>إدارة المستخدمين والصلاحيات</p>
          </div>
        </div>
      </div>

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
