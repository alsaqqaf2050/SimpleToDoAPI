// src/pages/ProfilePage.tsx
import { useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { motion } from 'framer-motion';
import { User, Mail, Shield, Key } from 'lucide-react';

export default function ProfilePage() {
  const user = useAuthStore((s) => s.user);

  return (
    <div className="fade-in">
      <div className="page-header">
        <h1>الملف الشخصي</h1>
        <p>إدارة بيانات حسابك</p>
      </div>

      <div className="grid grid-2" style={{ gap: '1.5rem' }}>
        {/* User Info Card */}
        <motion.div
          className="glass-card"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          style={{ padding: '2rem' }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', marginBottom: '2rem' }}>
            <div style={{
              width: 80, height: 80, borderRadius: '50%',
              background: 'var(--color-primary)', display: 'flex',
              alignItems: 'center', justifyContent: 'center',
              fontSize: '2rem', fontWeight: 'bold', color: 'white'
            }}>
              {user?.fullName?.split(' ').map(n => n[0]).join('').substring(0, 2) || '?'}
            </div>
            <div>
              <h2 style={{ fontSize: '1.5rem', marginBottom: '0.25rem' }}>{user?.fullName}</h2>
              <p style={{ color: 'var(--color-text-secondary)' }}>{user?.email}</p>
            </div>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '1rem', background: 'var(--color-bg-input)', borderRadius: 'var(--radius-md)' }}>
              <User size={20} style={{ color: 'var(--color-primary-light)' }} />
              <div>
                <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>الاسم الكامل</div>
                <div>{user?.fullName}</div>
              </div>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '1rem', background: 'var(--color-bg-input)', borderRadius: 'var(--radius-md)' }}>
              <Mail size={20} style={{ color: 'var(--color-primary-light)' }} />
              <div>
                <div style={{ fontSize: '0.8rem', color: 'var(--color-text-muted)' }}>البريد الإلكتروني</div>
                <div>{user?.email}</div>
              </div>
            </div>
          </div>
        </motion.div>

        {/* Roles & Permissions Card */}
        <motion.div
          className="glass-card"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          style={{ padding: '2rem' }}
        >
          <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem' }}>
            <Shield size={20} style={{ color: 'var(--color-warning)' }} /> الأدوار والصلاحيات
          </h3>

          <div style={{ marginBottom: '1.5rem' }}>
            <h4 style={{ fontSize: '0.9rem', color: 'var(--color-text-muted)', marginBottom: '0.5rem' }}>الأدوار (Roles)</h4>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
              {user?.roles?.map(role => (
                <span key={role} className="badge badge-primary">{role}</span>
              ))}
              {!user?.roles?.length && <span style={{ color: 'var(--color-text-muted)', fontSize: '0.875rem' }}>لا توجد أدوار</span>}
            </div>
          </div>

          <div>
            <h4 style={{ fontSize: '0.9rem', color: 'var(--color-text-muted)', marginBottom: '0.5rem' }}>الصلاحيات (Permissions)</h4>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
              {user?.permissions?.map(perm => (
                <span key={perm} className="badge" style={{ background: 'var(--color-bg-input)', border: '1px solid var(--color-border)' }}>
                  <Key size={12} style={{ marginRight: '0.25rem', display: 'inline' }} /> {perm}
                </span>
              ))}
              {!user?.permissions?.length && <span style={{ color: 'var(--color-text-muted)', fontSize: '0.875rem' }}>لا توجد صلاحيات محددة</span>}
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
