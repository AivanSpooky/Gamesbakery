import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { SellerService } from '@/services/SellerService';
import { useNotifications } from '@/composables/useNotifications';

export function useKeyCreate() {
  const gameId = ref('');
  const key = ref('');
  const router = useRouter();
  const { showSuccess, showError } = useNotifications();

  const createKey = async () => {
    if (!gameId.value || !key.value) {
      showError('Заполните все поля');
      return;
    }
    await SellerService.createKey(gameId.value, key.value);
    showSuccess('Ключ создан!');
    router.push('/seller/orderitems');
  };

  const cancel = () => router.push('/seller/orderitems');

  return { gameId, key, createKey, cancel };
}
