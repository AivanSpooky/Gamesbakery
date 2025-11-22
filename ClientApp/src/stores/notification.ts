import { defineStore } from 'pinia';

interface Notification {
  id: number;
  type: 'success' | 'warning' | 'error';
  message: string;
}

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as Notification[],
  }),
  actions: {
    add(type: Notification['type'], message: string) {
      const id = Date.now();
      this.notifications.push({ id, type, message });
      setTimeout(() => this.remove(id), 5000);
    },
    remove(id: number) {
      this.notifications = this.notifications.filter(n => n.id !== id);
    },
  },
});
