import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle2, Circle, Users, Briefcase } from 'lucide-react';
import { useAuthStore } from '../store/authStore';
import { useContentStore } from '../store/contentStore';
import { Megaphone, MessageCircleQuestion, X } from 'lucide-react';
import { useState } from 'react';

export default function DashboardPage() {
  const user = useAuthStore(state => state.user);
  const { announcement, faqText } = useContentStore();
  const [hideAnnouncement, setHideAnnouncement] = useState(false);
  
  const { data: stats, isLoading, isError } = useQuery({
    queryKey: ['dashboard_stats'],
    queryFn: dashboardService.getStats,
  });

  if (isLoading) {
    return (
      <div className="fade-in" style={{ padding: '2rem' }}>
        <div className="skeleton" style={{ height: 120, marginBottom: '2rem' }}></div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1.5rem' }}>
          <div className="skeleton" style={{ height: 150 }}></div>
          <div className="skeleton" style={{ height: 150 }}></div>
          <div className="skeleton" style={{ height: 150 }}></div>
        </div>
      </div>
    );
  }

  if (isError || !stats) {
    return (
      <div className="fade-in" style={{ padding: '2rem', textAlign: 'center' }}>
        <h3 style={{ color: 'var(--color-danger)' }}>خطأ في تحميل الإحصائيات</h3>
        <p>يرجى التأكد من اتصالك أو تسجيل الدخول مرة أخرى.</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      
      {/* Global Announcement Banner */}
      <AnimatePresence>
        {announcement && !hideAnnouncement && (
          <motion.div 
            initial={{ opacity: 0, height: 0 }} 
            animate={{ opacity: 1, height: 'auto' }} 
            exit={{ opacity: 0, height: 0 }}
            style={{ marginBottom: '2rem', overflow: 'hidden' }}
          >
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', backgroundColor: 'rgba(99, 102, 241, 0.1)', padding: '1rem 1.5rem', borderRadius: '12px', border: '1px solid rgba(99, 102, 241, 0.2)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', color: 'var(--color-primary)' }}>
                <Megaphone size={20} />
                <span style={{ fontWeight: 500 }}>{announcement}</span>
              </div>
              <button onClick={() => setHideAnnouncement(true)} style={{ background: 'transparent', border: 'none', cursor: 'pointer', color: 'var(--color-text-muted)' }}>
                <X size={20} />
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem', marginBottom: '2rem' }}>
        <div>
          <h1>لوحة التحكم</h1>
          <p>مرحباً بك {user?.fullName}، هذه نظرة عامة على أدائك والمهام.</p>
        </div>
      </div>
      
      {/* Tips / FAQ Section */}
      {faqText && (
        <motion.div className="glass-card" initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} style={{ padding: '1.25rem', marginBottom: '2rem', borderLeft: '4px solid #22c55e', display: 'flex', gap: '1rem', alignItems: 'flex-start' }}>
          <MessageCircleQuestion size={24} color="#22c55e" style={{ flexShrink: 0 }} />
          <div>
            <h4 style={{ margin: '0 0 0.25rem 0', color: '#22c55e' }}>تلميح مفيد</h4>
            <p style={{ margin: 0, fontSize: '0.95rem', color: 'var(--color-text-secondary)', lineHeight: 1.5 }}>
              {faqText}
            </p>
          </div>
        </motion.div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1.5rem', marginBottom: '3rem' }}>
        <motion.div className="glass-card" initial={{ opacity: 0, y: 15 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', padding: '1rem', borderRadius: '50%', color: '#3b82f6' }}>
              <Briefcase size={28} />
            </div>
            <div>
              <h3 style={{ fontSize: '2rem', fontWeight: 700, margin: 0 }}>{stats.totalTasks}</h3>
              <p style={{ color: 'var(--color-text-muted)', margin: 0, fontSize: '0.9rem' }}>إجمالي المهام</p>
            </div>
          </div>
        </motion.div>

        <motion.div className="glass-card" initial={{ opacity: 0, y: 15 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div style={{ backgroundColor: 'rgba(34, 197, 94, 0.1)', padding: '1rem', borderRadius: '50%', color: '#22c55e' }}>
              <CheckCircle2 size={28} />
            </div>
            <div>
              <h3 style={{ fontSize: '2rem', fontWeight: 700, margin: 0 }}>{stats.completedTasks}</h3>
              <p style={{ color: 'var(--color-text-muted)', margin: 0, fontSize: '0.9rem' }}>المهام المنجزة</p>
            </div>
          </div>
        </motion.div>

        <motion.div className="glass-card" initial={{ opacity: 0, y: 15 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div style={{ backgroundColor: 'rgba(234, 179, 8, 0.1)', padding: '1rem', borderRadius: '50%', color: '#eab308' }}>
              <Circle size={28} />
            </div>
            <div>
              <h3 style={{ fontSize: '2rem', fontWeight: 700, margin: 0 }}>{stats.pendingTasks}</h3>
              <p style={{ color: 'var(--color-text-muted)', margin: 0, fontSize: '0.9rem' }}>قيد التنفيذ</p>
            </div>
          </div>
        </motion.div>
      </div>

      <div className="glass-card" style={{ padding: '1.5rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem' }}>
          <Users size={20} className="text-primary" />
          <h2 style={{ fontSize: '1.25rem', fontWeight: 600, margin: 0 }}>إحصائيات الموظفين</h2>
        </div>

        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'right' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--color-border)' }}>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>الموظف</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>إجمالي المهام</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>منجزة</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>قيد التنفيذ</th>
                <th style={{ padding: '1rem 0.5rem', color: 'var(--color-text-muted)' }}>معدل الإنجاز</th>
              </tr>
            </thead>
            <tbody>
              {stats.employeeStats.map((emp, idx) => (
                <tr key={emp.userId} style={{ borderBottom: '1px solid var(--color-border)', backgroundColor: idx % 2 === 0 ? 'transparent' : 'rgba(0,0,0,0.02)' }}>
                  <td style={{ padding: '1rem 0.5rem', fontWeight: 500 }}>{emp.fullName}</td>
                  <td style={{ padding: '1rem 0.5rem' }}>{emp.totalTasks}</td>
                  <td style={{ padding: '1rem 0.5rem', color: '#22c55e' }}>{emp.completedTasks}</td>
                  <td style={{ padding: '1rem 0.5rem', color: '#eab308' }}>{emp.pendingTasks}</td>
                  <td style={{ padding: '1rem 0.5rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <div style={{ flex: 1, backgroundColor: 'var(--color-border)', height: '6px', borderRadius: '3px', overflow: 'hidden' }}>
                        <div 
                          style={{ 
                            width: `${emp.totalTasks > 0 ? Math.round((emp.completedTasks / emp.totalTasks) * 100) : 0}%`, 
                            backgroundColor: '#3b82f6', 
                            height: '100%' 
                          }}
                        />
                      </div>
                      <span style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>
                        {emp.totalTasks > 0 ? Math.round((emp.completedTasks / emp.totalTasks) * 100) : 0}%
                      </span>
                    </div>
                  </td>
                </tr>
              ))}
              {stats.employeeStats.length === 0 && (
                <tr>
                  <td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: 'var(--color-text-muted)' }}>
                    لا يوجد مهام لأي موظف حالياً
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
