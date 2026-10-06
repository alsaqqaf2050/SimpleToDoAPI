import { useState } from 'react';
import { useContentStore } from '../store/contentStore';
import { Megaphone, MessageCircleQuestion, Send, Save, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { motion } from 'framer-motion';

export default function ContentManagement() {
  const { announcement, setAnnouncement, faqText, setFaqText } = useContentStore();
  
  const [draftAnnouncement, setDraftAnnouncement] = useState(announcement || '');
  const [draftFaq, setDraftFaq] = useState(faqText || '');

  const handleSaveAnnouncement = () => {
    setAnnouncement(draftAnnouncement.trim() !== '' ? draftAnnouncement : null);
    toast.success('تم تحديث الإعلان العام بنجاح');
  };

  const handleClearAnnouncement = () => {
    setDraftAnnouncement('');
    setAnnouncement(null);
    toast.success('تم إزالة الإعلان');
  };

  const handleSaveFaq = () => {
    setFaqText(draftFaq);
    toast.success('تم تحديث الأسئلة الشائعة/التلميحات');
  };

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '2rem' }}>
      
      {/* Announcements */}
      <motion.div 
        className="glass-card" 
        style={{ padding: '2rem' }}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1.5rem' }}>
          <div style={{ padding: '0.75rem', borderRadius: '12px', background: 'rgba(99, 102, 241, 0.1)', color: 'var(--color-primary)' }}>
            <Megaphone size={24} />
          </div>
          <h2 style={{ fontSize: '1.25rem', margin: 0 }}>نظام الإعلانات</h2>
        </div>
        
        <p style={{ color: 'var(--color-text-secondary)', marginBottom: '1.5rem', fontSize: '0.9rem' }}>
          اكتب رسالة لتظهر كإعلان لجميع المستخدمين في التطبيق (مثال: تحديثات النظام، رسائل ترحيبية).
        </p>

        <textarea 
          className="form-control" 
          rows={4}
          placeholder="اكتب الإعلان هنا..."
          value={draftAnnouncement}
          onChange={(e) => setDraftAnnouncement(e.target.value)}
          style={{ marginBottom: '1rem', resize: 'vertical' }}
        ></textarea>

        <div style={{ display: 'flex', gap: '1rem' }}>
          <button className="btn btn-primary" onClick={handleSaveAnnouncement} style={{ flex: 1, display: 'flex', justifyContent: 'center', gap: '0.5rem' }}>
            <Send size={18} /> نشر الإعلان
          </button>
          {announcement && (
            <button className="btn btn-ghost" onClick={handleClearAnnouncement} style={{ color: 'var(--color-danger)' }} title="إزالة الإعلان الحالي">
              <Trash2 size={18} />
            </button>
          )}
        </div>
      </motion.div>

      {/* Tooltips & FAQ */}
      <motion.div 
        className="glass-card" 
        style={{ padding: '2rem' }}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1.5rem' }}>
          <div style={{ padding: '0.75rem', borderRadius: '12px', background: 'rgba(34, 197, 94, 0.1)', color: '#22c55e' }}>
            <MessageCircleQuestion size={24} />
          </div>
          <h2 style={{ fontSize: '1.25rem', margin: 0 }}>تلميحات المستخدمين</h2>
        </div>

        <p style={{ color: 'var(--color-text-secondary)', marginBottom: '1.5rem', fontSize: '0.9rem' }}>
          قم بتعديل النصوص والملاحظات الإرشادية التي تظهر للمستخدمين الجدد أو في قسم الأسئلة الشائعة لتسهيل استخدام التطبيق.
        </p>

        <textarea 
          className="form-control" 
          rows={4}
          placeholder="اكتب التلميحات أو الأسئلة الشائعة..."
          value={draftFaq}
          onChange={(e) => setDraftFaq(e.target.value)}
          style={{ marginBottom: '1rem', resize: 'vertical' }}
        ></textarea>

        <button className="btn" onClick={handleSaveFaq} style={{ width: '100%', display: 'flex', justifyContent: 'center', gap: '0.5rem', background: '#22c55e', color: 'white', border: 'none' }}>
          <Save size={18} /> حفظ التعديلات
        </button>
      </motion.div>
      
    </div>
  );
}
