import { useNotificationStore } from '@/stores/notification';

export const useNotifications = () => {
  const store = useNotificationStore();
  const showSuccess = (message: string) => {
    store.add('success', message);
  };
  const showWarning = (message: string) => {
    store.add('warning', message);
  };
  const showError = (message: string) => {
    store.add('error', message);
  };
  return { showSuccess, showWarning, showError };
};
