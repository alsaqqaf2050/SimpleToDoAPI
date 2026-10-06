import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService } from '../services/userService';
import { auditService } from '../services/auditService';
import { Shield, Trash2, AlertCircle, Ban, Search, Download, CheckCircle, Activity, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuthStore } from '../store/authStore';
import type { AppUser } from '../types';

export default function UserManagement() {
  const queryClient = useQueryClient();
  const currentUser = useAuthStore((s) => s.user);

  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedUserForActivity, setSelectedUserForActivity] = useState<AppUser | null>(null);

  // Fetch users (fetching 100 for client-side search simulation, or just sticking to 10 with limited search)
  // To provide "real-time" search as requested, let's fetch a larger page size (e.g. 50) and search client-side.
  const { data, isLoading } = useQuery({
    queryKey: ['users', page],
    queryFn: () => userService.getAll(page, 50),
  });

  const users = data?.data ?? [];
  const pagination = data?.pagination;

  // Search Filter
  const filteredUsers = useMemo(() => {
    if (!searchTerm) return users;
    const lower = searchTerm.toLowerCase();
    return users.filter(u => 
      u.fullName.toLowerCase().includes(lower) || 
      u.email.toLowerCase().includes(lower) ||
      u.roles.some(r => r.toLowerCase().includes(lower))
    );
  }, [users, searchTerm]);

  // Mutations
  const suspendMutation = useMutation({
    mutationFn: (user: AppUser) => userService.update(user.id, { fullName: user.fullName, email: user.email, isActive: !user.isActive }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('تم تحديث حالة المستخدم');
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'حدث خطأ')
  });

  const promoteMutation = useMutation({
    mutationFn: (userId: string) => userService.promoteToAdmin(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('تم ترقية المستخدم إلى مدير بنجاح');
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'حدث خطأ أثناء الترقية')
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => userService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('تم حذف المستخدم بنجاح');
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'فشل حذف المستخدم')
  });

  const handleExportCSV = () => {
    if (!users.length) return;
    const headers = ['ID', 'Full Name', 'Email', 'Active', 'Roles', 'Created Date'];
    const rows = filteredUsers.map(u => [
      u.id, 
      u.fullName, 
      u.email, 
      u.isActive ? 'Yes' : 'No', 
      u.roles.join(' - '), 
      new Date(u.createdDate).toLocaleDateString('ar-SA')
    ]);
    
    let csvContent = "data:text/csv;charset=utf-8," + [headers, ...rows].map(e => e.join(",")).join("\n");
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", "users_export.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    toast.success('تم تصدير البيانات بنجاح');
  };

  return (
    <div className="fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div className="search-container" style={{ position: 'relative', minWidth: '300px' }}>
          <Search size={18} style={{ position: 'absolute', right: '1rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
          <input
            type="text"
            className="input"
            placeholder="بحث عن مستخدم..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ paddingRight: '2.5rem', width: '100%', background: 'var(--color-bg-elevated)' }}
          />
        </div>
        <button className="btn btn-outline" onClick={handleExportCSV}>
          <Download size={18} />
          تصدير CSV
        </button>
      </div>

      <div className="glass-card" style={{ overflowX: 'auto' }}>
        {isLoading ? (
          <div style={{ padding: '2rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {[...Array(5)].map((_, i) => <div key={i} className="skeleton" style={{ height: 60 }} />)}
          </div>
        ) : (
          <table style={{ width: '100%', textAlign: 'right', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--color-border)', background: 'rgba(0,0,0,0.02)' }}>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>المستخدم</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>الحالة</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>الأدوار</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>تاريخ الانضمام</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>إجراءات سريعة</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map((u) => (
                <motion.tr 
                  initial={{ opacity: 0 }} animate={{ opacity: 1 }}
                  key={u.id} 
                  style={{ borderBottom: '1px solid var(--color-border)', transition: 'background 0.2s' }}
                  className="table-row-hover"
                >
                  <td style={{ padding: '1rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
                    <div style={{ width: 40, height: 40, borderRadius: '50%', background: 'var(--color-primary)', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold' }}>
                      {u.fullName.charAt(0)}
                    </div>
                    <div>
                      <div style={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        {u.fullName}
                        {u.id === currentUser?.id && <span className="badge badge-accent" style={{ fontSize: '0.6rem' }}>أنت</span>}
                      </div>
                      <div style={{ color: 'var(--color-text-secondary)', fontSize: '0.85rem' }}>{u.email}</div>
                    </div>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    {u.isActive ? (
                      <span className="badge" style={{ background: 'rgba(34, 197, 94, 0.1)', color: '#22c55e' }}>
                        <CheckCircle size={14} style={{ marginRight: '0.2rem' }} /> نشط
                      </span>
                    ) : (
                      <span className="badge" style={{ background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444' }}>
                        <Ban size={14} style={{ marginRight: '0.2rem' }} /> موقوف
                      </span>
                    )}
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', gap: '0.3rem', flexWrap: 'wrap' }}>
                      {u.roles?.map(r => (
                        <span key={r} className={`badge ${r === 'Admin' ? 'badge-primary' : 'badge-accent'}`} style={{ fontSize: '0.75rem' }}>
                          {r === 'Admin' ? <Shield size={12} style={{ marginLeft: 4 }}/> : null}
                          {r}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td style={{ padding: '1rem', color: 'var(--color-text-secondary)' }}>
                    {new Date(u.createdDate).toLocaleDateString('ar-SA')}
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <button 
                        className="icon-btn" 
                        onClick={() => setSelectedUserForActivity(u)}
                        data-tooltip="سجل النشاط"
                        style={{ color: '#3b82f6', background: 'rgba(59, 130, 246, 0.1)' }}
                      >
                        <Activity size={16} />
                      </button>
                      {u.id !== currentUser?.id && (
                        <>
                          <button
                            className="icon-btn"
                            onClick={() => {
                              if(confirm(`هل أنت متأكد من ترقية ${u.fullName} إلى مدير؟`)){
                                promoteMutation.mutate(u.id);
                              }
                            }}
                            disabled={u.roles.includes('Admin') || promoteMutation.isPending}
                            data-tooltip="ترقية كمدير"
                            style={{ color: '#8b5cf6', background: 'rgba(139, 92, 246, 0.1)' }}
                          >
                            <Shield size={16} />
                          </button>
                          <button
                            className="icon-btn"
                            onClick={() => {
                              if(confirm(`هل أنت متأكد من ${u.isActive ? 'إيقاف' : 'تفعيل'} الحساب؟`)){
                                suspendMutation.mutate(u);
                              }
                            }}
                            disabled={suspendMutation.isPending}
                            data-tooltip={u.isActive ? "إيقاف الحساب" : "تفعيل الحساب"}
                            style={{ color: u.isActive ? '#f59e0b' : '#22c55e', background: u.isActive ? 'rgba(245, 158, 11, 0.1)' : 'rgba(34, 197, 94, 0.1)' }}
                          >
                            {u.isActive ? <Ban size={16} /> : <CheckCircle size={16} />}
                          </button>
                          <button
                            className="icon-btn danger"
                            onClick={() => {
                              if (confirm(`هل أنت متأكد من حذف المستخدم "${u.fullName}" بشكل نهائي؟`)) {
                                deleteMutation.mutate(u.id);
                              }
                            }}
                            disabled={deleteMutation.isPending}
                            data-tooltip="حذف نهائي"
                          >
                            <Trash2 size={16} />
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </motion.tr>
              ))}
              {filteredUsers.length === 0 && !isLoading && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '3rem', color: 'var(--color-text-muted)' }}>
                    لا يوجد مستخدمين يطابقون بحثك.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}

        {/* Pagination */}
        {pagination && pagination.totalPages > 1 && !searchTerm && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', padding: '1.5rem' }}>
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
      </div>

      {/* User Activity Modal */}
      <AnimatePresence>
        {selectedUserForActivity && (
          <UserActivityModal user={selectedUserForActivity} onClose={() => setSelectedUserForActivity(null)} />
        )}
      </AnimatePresence>
    </div>
  );
}

// Activity Modal Component
function UserActivityModal({ user, onClose }: { user: AppUser, onClose: () => void }) {
  const { data: logsData, isLoading } = useQuery({
    queryKey: ['auditLogs', user.id],
    queryFn: () => auditService.getLogs({ userId: user.id, pageSize: 20 }),
  });

  const logs = logsData?.data || [];

  return (
    <div className="modal-overlay" onClick={onClose} style={{
      position: 'fixed', top: 0, left: 0, width: '100%', height: '100%', 
      background: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(4px)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 9999
    }}>
      <motion.div 
        className="glass-card" 
        onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.95 }}
        style={{ width: '90%', maxWidth: '600px', maxHeight: '85vh', display: 'flex', flexDirection: 'column' }}
      >
        <div style={{ padding: '1.5rem', borderBottom: '1px solid var(--color-border)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h2 style={{ fontSize: '1.25rem', margin: 0 }}>سجل نشاط المستخدم</h2>
            <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0 0', fontSize: '0.9rem' }}>{user.fullName} ({user.email})</p>
          </div>
          <button className="icon-btn" onClick={onClose}><X size={20} /></button>
        </div>
        <div style={{ padding: '1.5rem', overflowY: 'auto', flex: 1 }}>
          {isLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem' }}>جاري تحميل السجل...</div>
          ) : logs.length > 0 ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {logs.map(log => (
                <div key={log.id} style={{ display: 'flex', gap: '1rem', padding: '1rem', background: 'var(--color-bg-elevated)', borderRadius: '8px', borderLeft: '4px solid var(--color-primary)' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                      <span style={{ fontWeight: 600, color: 'var(--color-primary)' }}>{log.action}</span>
                      <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>{new Date(log.timestamp).toLocaleString('ar-SA')}</span>
                    </div>
                    <div style={{ fontSize: '0.9rem', color: 'var(--color-text-secondary)' }}>
                      الجدول: <strong>{log.tableName}</strong>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--color-text-muted)' }}>
              <AlertCircle size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
              لا توجد نشاطات مسجلة لهذا المستخدم.
            </div>
          )}
        </div>
      </motion.div>
    </div>
  );
}
