import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { GiftService } from '@/services/GiftService';
import { OrderService } from '@/services/OrderService';
import { useNotifications } from '@/composables/useNotifications';
import type { GiftItem } from '@/types/gift';

export function useGiftCreate() {
  const recipientId = ref('');
  const items = ref<GiftItem[]>([]);
  const showModal = ref(false);
  const selectedOrderItemId = ref('');
  const purchasedItems = ref<GiftItem[]>([]);
  const sentGiftIds = ref<string[]>([]);
  const { showSuccess, showError } = useNotifications();
  const router = useRouter();

  const available = computed(() =>
    purchasedItems.value.filter(a => !sentGiftIds.value.includes(a.id) && !items.value.some(i => i.id === a.id))
  );

  const addItem = () => { showModal.value = true; };
  const addSelected = () => {
    if (selectedOrderItemId.value) {
      const selected = purchasedItems.value.find(a => a.id === selectedOrderItemId.value);
      if (selected) items.value.push(selected);
      selectedOrderItemId.value = '';
      showModal.value = false;
    }
  };
  const closeModal = () => {
    selectedOrderItemId.value = '';
    showModal.value = false;
  };

  const sendGift = async () => {
    if (!recipientId.value.trim()) {
      showError('Введите ID получателя');
      return;
    }
    if (items.value.length === 0) {
      showError('Добавьте хотя бы один ключ');
      return;
    }
    try {
      await GiftService.create(recipientId.value, items.value[0].id);
      showSuccess('Подарок успешно отправлен!');
      router.push('/gift/index');
    } catch (err: any) {
      showError(err.response?.data?.message || 'Ошибка отправки подарка');
    }
  };

  const cancel = () => router.push('/gift/index');

  onMounted(async () => {
    try {
      const rawOrders = await OrderService.getAll({ page: 1, limit: 100 });
      const completedOrders = rawOrders.items || [];
      const details = await Promise.all(completedOrders.map((o: any) => OrderService.getById(o.orderId)));
      purchasedItems.value = details.flatMap((d: any) => (d.orderItems || []).map((oi: any) => ({
        id: oi.id.toString(),
        gameTitle: oi.gameTitle,
      })));
      const sentGifts = await GiftService.getAll('sent');
      sentGiftIds.value = sentGifts.map((g: any) => g.orderItemId.toString());
    } catch (err: any) {
      showError('Ошибка загрузки доступных ключей');
    }
  });

  return { recipientId, items, showModal, selectedOrderItemId, available, addItem, addSelected, closeModal, sendGift, cancel };
}
