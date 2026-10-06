import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface ContentState {
  announcement: string | null;
  faqText: string;
  setAnnouncement: (text: string | null) => void;
  setFaqText: (text: string) => void;
}

export const useContentStore = create<ContentState>()(
  persist(
    (set) => ({
      announcement: null,
      faqText: 'مرحباً بك في تطبيق المهام. يمكنك البدء بإضافة مهام جديدة من الزر أدناه.',
      setAnnouncement: (text) => set({ announcement: text }),
      setFaqText: (text) => set({ faqText: text }),
    }),
    {
      name: 'content-storage',
    }
  )
);
