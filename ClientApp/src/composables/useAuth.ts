import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { AuthService } from '@/services/AuthService';
import { useNotifications } from '@/composables/useNotifications';

export function useLogin() {
  const username = ref('');
  const password = ref('');
  const router = useRouter();
  const authStore = useAuthStore();
  const { showSuccess, showError } = useNotifications();
  const handleLogin = async () => {
    if (!username.value.trim() || !password.value.trim()) {
      showError('Пожалуйста, заполните все поля');
      return;
    }
    try {
      const response = await AuthService.login(username.value, password.value);
      authStore.login(response.token, response.role, response.username, response.userId, response.sellerId || null);
      showSuccess('Вход выполнен успешно!');
      router.push('/');
    } catch (err) {
      showError('Ошибка входа');
    }
  };
  return { username, password, handleLogin };
}

export function useRegister() {
  const username = ref('');
  const email = ref('');
  const password = ref('');
  const country = ref('');
  const router = useRouter();
  const authStore = useAuthStore();
  const { showSuccess, showError } = useNotifications();
  const handleRegister = async () => {
    try {
      const data = await AuthService.register(username.value, email.value, password.value, country.value);
      authStore.login(data.token, data.role, username.value, data.userId);
      showSuccess('Регистрация успешна!');
      router.push('/');
    } catch (err: any) {
      showError(err.response?.data?.message || 'Ошибка регистрации');
    }
  };
  return { username, email, password, country, handleRegister };
}
