import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { auditService } from '../services/auditService';
import { systemService } from '../services/systemService';
import { ShieldAlert, Activity, Filter, Power } from 'lucide-react';
import { motion } from 'framer-motion';

function MaintenanceToggle() {
  const queryClient = useQueryClient();
  
  const { data, isLoading } = useQuery({
    queryKey: ['maintenance-status'],
    queryFn: () => systemService.getMaintenanceStatus(),
  });

  const mutation = useMutation({
    mutationFn: (newStatus: boolean) => systemService.toggleMaintenanceMode(newStatus),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenance-status'] });
    }
  });

  if (isLoading) return <div className="skeleton" style={{ height: 60, marginBottom: '1.5rem' }} />;

  const isMaintenanceMode = data?.isMaintenanceMode || false;

  return (
    <div className="glass-card" style={{ padding: '1.5rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between', border: isMaintenanceMode ? '1px solid var(--color-warning)' : undefined }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
        <div style={{ width: 48, height: 48, borderRadius: '50%', background: isMaintenanceMode ? 'rgba(234, 179, 8, 0.1)' : 'rgba(34, 197, 94, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Power size={24} color={isMaintenanceMode ? 'var(--color-warning)' : 'var(--color-success)'} />
        </div>
        <div>
          <h3 style={{ margin: 0, fontSize: '1.1rem' }}>وضع الصيانة (Maintenance Mode)</h3>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
            {isMaintenanceMode 
              ? 'النظام حالياً في وضع الصيانة. لا يمكن للمستخدمين العاديين تسجيل الدخول.'
              : 'النظام يعمل بشكل طبيعي. جميع المستخدمين يمكنهم الوصول.'}
          </p>
        </div>
      </div>
      <button 
        className={`btn ${isMaintenanceMode ? 'btn-ghost' : 'btn-danger'}`}
        onClick={() => mutation.mutate(!isMaintenanceMode)}
        disabled={mutation.isPending}
      >
        {mutation.isPending ? 'جاري التغيير...' : isMaintenanceMode ? 'إيقاف الصيانة' : 'تفعيل الصيانة'}
      </button>
    </div>
  );
}

export default function SystemSecurity() {
  const [page, setPage] = useState(1);
  const [filterAction, setFilterAction] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['system-audit', page, filterAction],
    queryFn: () => auditService.getLogs({ pageNumber: page, pageSize: 20, action: filterAction || undefined }),
  });

  const logs = data?.data ?? [];
  const pagination = data?.pagination;

  return (
    <div className="fade-in">
      <div className="glass-card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
          <ShieldAlert size={24} color="var(--color-danger)" />
          <h2 style={{ fontSize: '1.25rem', margin: 0 }}>سجلات النظام والأمان</h2>
        </div>
        <p style={{ color: 'var(--color-text-secondary)', marginBottom: '1.5rem' }}>
          مراقبة جميع الأنشطة والإجراءات التي تمت في النظام لضمان الأمان والامتثال.
        </p>

        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <Filter size={18} color="var(--color-text-muted)" />
          <select 
            className="input" 
            style={{ minWidth: '200px' }}
            value={filterAction}
            onChange={(e) => { setFilterAction(e.target.value); setPage(1); }}
          >
            <option value="">جميع الإجراءات</option>
            <option value="Create">إنشاء (Create)</option>
            <option value="Update">تحديث (Update)</option>
            <option value="Delete">حذف (Delete)</option>
            <option value="Login">تسجيل دخول ناجح (Login)</option>
            <option value="FailedLogin">فشل تسجيل الدخول (FailedLogin)</option>
          </select>
        </div>
      </div>

      <MaintenanceToggle />


      <div className="glass-card" style={{ overflowX: 'auto' }}>
        {isLoading ? (
          <div style={{ padding: '2rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {[...Array(5)].map((_, i) => <div key={i} className="skeleton" style={{ height: 60 }} />)}
          </div>
        ) : (
          <table style={{ width: '100%', textAlign: 'right', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--color-border)', background: 'rgba(0,0,0,0.02)' }}>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>التاريخ والوقت</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>المستخدم</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>الإجراء</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>الكيان (الجدول)</th>
                <th style={{ padding: '1.2rem 1rem', color: 'var(--color-text-muted)' }}>التفاصيل</th>
              </tr>
            </thead>
            <tbody>
              {logs.map(log => (
                <motion.tr 
                  initial={{ opacity: 0 }} animate={{ opacity: 1 }}
                  key={log.id} 
                  style={{ borderBottom: '1px solid var(--color-border)', transition: 'background 0.2s' }}
                  className="table-row-hover"
                >
                  <td style={{ padding: '1rem', color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
                    {new Date(log.timestamp).toLocaleString('ar-SA')}
                  </td>
                  <td style={{ padding: '1rem', fontWeight: 600 }}>
                    {log.userId || 'نظام / مجهول'}
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <span className={`badge ${log.action === 'Delete' ? 'badge-danger' : log.action === 'Update' ? 'badge-accent' : 'badge-primary'}`}>
                      {log.action}
                    </span>
                  </td>
                  <td style={{ padding: '1rem', color: 'var(--color-text-secondary)' }}>
                    {log.tableName}
                  </td>
                  <td style={{ padding: '1rem', maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }} title={log.newValues || log.oldValues}>
                      {log.newValues || log.oldValues || 'لا توجد بيانات تفصيلية'}
                    </span>
                  </td>
                </motion.tr>
              ))}
              {logs.length === 0 && (
                <tr>
                  <td colSpan={5} style={{ textAlign: 'center', padding: '3rem', color: 'var(--color-text-muted)' }}>
                    <Activity size={48} style={{ margin: '0 auto 1rem', opacity: 0.5 }} />
                    لا توجد سجلات أمان مطابقة.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}

        {/* Pagination */}
        {pagination && pagination.totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', padding: '1.5rem' }}>
            <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p - 1)} disabled={page === 1}>
              السابق
            </button>
            <span style={{ padding: '0.5rem 1rem', color: 'var(--color-text-muted)' }}>
              صفحة {page} من {pagination.totalPages}
            </span>
            <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p + 1)} disabled={page === pagination.totalPages}>
              التالي
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
