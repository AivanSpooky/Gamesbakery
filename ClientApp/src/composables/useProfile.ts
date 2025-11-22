import { ref, computed, onMounted } from 'vue';
import { UserService } from '@/services/UserService';
import { useNotifications } from '@/composables/useNotifications';
import type { Profile } from '@/types';

export function useProfile() {
  const profile = ref<Profile>({
    username: '',
    role: '',
    email: '',
    country: '',
    balance: 0,
    totalSpent: 0,
  });
  const amountString = ref('');
  const { showError, showSuccess } = useNotifications();

  const formattedBalance = computed(() => `${profile.value.balance} ₽`);
  const formattedTotalSpent = computed(() => `${profile.value.totalSpent} ₽`);

  const replenish = async () => {
    const amount = parseFloat(amountString.value);
    if (isNaN(amount) || amount <= 0) {
      showError('Сумма должна быть положительной');
      return;
    }
    await UserService.updateBalance(amount);
    profile.value.balance += amount;
    amountString.value = '';
    showSuccess('Баланс успешно пополнен');
  };

  onMounted(async () => {
    profile.value = await UserService.getProfile();
  });

  return { profile, amountString, formattedBalance, formattedTotalSpent, replenish };
}
